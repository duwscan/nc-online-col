using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using wnc.Data;
using wnc.Features.Students.Applications;
using wnc.Features.Students.Applications.ViewModels;
using wnc.Models;

namespace wnc.Features.Students.Applications.Controllers;

[Authorize(Roles = "CANDIDATE")]
public class StudentApplicationsController(AppDbContext dbContext, IWebHostEnvironment webHostEnvironment) : Controller
{
    private const long MaxDocumentFileSizeBytes = 5 * 1024 * 1024;
    private static readonly string[] AllowedPdfMimeTypes = ["application/pdf", "application/x-pdf"];

    private readonly AppDbContext _dbContext = dbContext;
    private readonly StudentApplicationDocumentStorage _documentStorage = new(webHostEnvironment);

    [HttpGet("/student/applications")]
    public async Task<IActionResult> Index()
    {
        var (hasAuthenticatedUserId, candidate) = await ResolveCurrentCandidateAsync();
        if (!hasAuthenticatedUserId) return RedirectToAction("Login", "StudentLogin");
        if (candidate == null) return NotFound();

        var applications = await CreateCandidateApplicationsQuery(candidate.Id)
            .Include(application => application.RoundProgram)
                .ThenInclude(roundProgram => roundProgram.Round)
            .Include(application => application.RoundProgram)
                .ThenInclude(roundProgram => roundProgram.Program)
            .Include(application => application.RoundProgram)
                .ThenInclude(roundProgram => roundProgram.Major)
            .OrderByDescending(application => application.SubmittedAt)
            .ThenByDescending(application => application.CreatedAt)
            .ToListAsync();

        var model = new StudentApplicationsIndexViewModel
        {
            Applications = applications.Select(application => new ApplicationSummary
            {
                Id = application.Id,
                ApplicationCode = application.ApplicationCode,
                RoundName = application.RoundProgram.Round.RoundName,
                ProgramName = application.RoundProgram.Program.ProgramName,
                MajorName = application.RoundProgram.Major?.MajorName,
                SubmittedAt = application.SubmittedAt,
                CreatedAt = application.CreatedAt,
                CurrentStatus = application.CurrentStatus
            }).ToList()
        };

        return View("~/Views/Student/Applications/Index.cshtml", model);
    }

    [HttpGet("/student/applications/new/{roundProgramId:guid}")]
    public async Task<IActionResult> Create(Guid roundProgramId)
    {
        var (hasAuthenticatedUserId, candidate) = await ResolveCurrentCandidateAsync();
        if (!hasAuthenticatedUserId) return RedirectToAction("Login", "StudentLogin");
        if (candidate == null) return NotFound();

        var roundProgram = await _dbContext.RoundPrograms
            .Include(currentRoundProgram => currentRoundProgram.Round)
            .Include(currentRoundProgram => currentRoundProgram.DocumentRequirements)
                .ThenInclude(requirement => requirement.DocumentType)
            .FirstOrDefaultAsync(currentRoundProgram => currentRoundProgram.Id == roundProgramId);

        if (roundProgram == null)
        {
            return NotFound();
        }

        if (!IsEligibleForDraftCreation(roundProgram, DateTime.UtcNow))
        {
            return LocalRedirect($"/student/programs/{roundProgramId}");
        }

        var existingApplication = await CreateCandidateApplicationsQuery(candidate.Id)
            .FirstOrDefaultAsync(application => application.RoundProgramId == roundProgramId);

        if (existingApplication != null)
        {
            return LocalRedirect($"/student/applications/{existingApplication.Id}?source=existing");
        }

        var createdAt = DateTime.UtcNow;

        for (var attempt = 0; attempt < 5; attempt++)
        {
            var application = new AdmissionApplication
            {
                Id = Guid.NewGuid(),
                ApplicationCode = await GenerateUniqueApplicationCodeAsync(),
                CandidateId = candidate.Id,
                RoundProgramId = roundProgram.Id,
                CurrentStatus = "DRAFT",
                SubmissionNumber = 0,
                CancelledAt = null,
                CreatedAt = createdAt,
                UpdatedAt = createdAt
            };

            _dbContext.AdmissionApplications.Add(application);

            try
            {
                await _dbContext.SaveChangesAsync();
                return LocalRedirect($"/student/applications/{application.Id}/documents");
            }
            catch (DbUpdateException exception) when (IsDuplicateApplicationConflict(exception))
            {
                _dbContext.Entry(application).State = EntityState.Detached;

                var concurrentApplication = await CreateCandidateApplicationsQuery(candidate.Id)
                    .FirstOrDefaultAsync(currentApplication => currentApplication.RoundProgramId == roundProgramId);

                if (concurrentApplication != null)
                {
                    return LocalRedirect($"/student/applications/{concurrentApplication.Id}?source=existing");
                }
            }
        }

        throw new InvalidOperationException("Unable to create a unique admission application draft.");
    }

    [HttpGet("/student/applications/{id:guid}")]
    public async Task<IActionResult> Detail(Guid id, string? source)
    {
        var (hasAuthenticatedUserId, candidate) = await ResolveCurrentCandidateAsync();
        if (!hasAuthenticatedUserId) return RedirectToAction("Login", "StudentLogin");
        if (candidate == null) return NotFound();

        var application = await CreateCandidateApplicationsQuery(candidate.Id)
            .Include(application => application.RoundProgram)
                .ThenInclude(roundProgram => roundProgram.Round)
            .Include(application => application.RoundProgram)
                .ThenInclude(roundProgram => roundProgram.Program)
            .Include(application => application.RoundProgram)
                .ThenInclude(roundProgram => roundProgram.Major)
            .Include(application => application.RoundProgram)
                .ThenInclude(roundProgram => roundProgram.DocumentRequirements)
                    .ThenInclude(requirement => requirement.DocumentType)
            .FirstOrDefaultAsync(application => application.Id == id);

        if (application == null) return NotFound();

        var allowedDocumentTypeIds = application.RoundProgram.DocumentRequirements
            .Select(requirement => requirement.DocumentTypeId)
            .ToHashSet();

        var documents = await CreateEffectiveCurrentApplicationDocumentsQuery(id)
            .Include(document => document.DocumentType)
            .Where(document => allowedDocumentTypeIds.Contains(document.DocumentTypeId))
            .OrderBy(document => document.DocumentType.DocumentName)
            .ThenByDescending(document => document.UploadedAt)
            .ToListAsync();

        var history = await _dbContext.ApplicationStatusHistories
            .AsNoTracking()
            .Where(entry => entry.ApplicationId == id)
            .OrderBy(entry => entry.ChangedAt)
            .ToListAsync();

        var isDraft = string.Equals(application.CurrentStatus, "DRAFT", StringComparison.Ordinal);

        var model = new StudentApplicationDetailViewModel
        {
            ApplicationId = id,
            ApplicationCode = application.ApplicationCode,
            CurrentStatus = application.CurrentStatus,
            RoundName = application.RoundProgram.Round.RoundName,
            ProgramName = application.RoundProgram.Program.ProgramName,
            MajorName = application.RoundProgram.Major?.MajorName,
            SubmittedAt = application.SubmittedAt,
            CreatedAt = application.CreatedAt,
            IsDraft = isDraft,
            RejectionReason = application.RejectionReason,
            ShowDuplicateBanner = string.Equals(source, "existing", StringComparison.Ordinal),
            ContinueSubmitLink = isDraft ? $"/student/applications/{id}/documents" : null,
            Documents = documents.Select(doc => new DocumentSummaryViewModel
            {
                DocumentId = doc.Id,
                DocumentTypeName = doc.DocumentType.DocumentName,
                FileName = doc.FileName,
                StoragePath = doc.StoragePath,
                FileSize = doc.FileSize,
                UploadedAt = doc.UploadedAt.ToString("dd/MM/yyyy HH:mm"),
                ValidationStatus = doc.ValidationStatus
            }).ToList(),
            Timeline = history.Select(entry => new TimelineEntryViewModel
            {
                FromStatus = entry.FromStatus,
                ToStatus = entry.ToStatus,
                ChangedAt = entry.ChangedAt,
                Reason = entry.Reason,
                PublicNote = entry.PublicNote
            }).ToList()
        };

        return View("~/Views/Student/Applications/Detail.cshtml", model);
    }

    [HttpGet("/student/applications/{id:guid}/documents")]
    public async Task<IActionResult> Documents(Guid id)
    {
        var (hasAuthenticatedUserId, candidate) = await ResolveCurrentCandidateAsync();
        if (!hasAuthenticatedUserId) return RedirectToAction("Login", "StudentLogin");
        if (candidate == null) return NotFound();

        var application = await CreateCandidateApplicationsQuery(candidate.Id)
            .FirstOrDefaultAsync(application => application.Id == id);

        if (application == null) return NotFound();

        if (!string.Equals(application.CurrentStatus, "DRAFT", StringComparison.Ordinal))
        {
            return RedirectToAction(nameof(Detail), new { id });
        }

        var roundProgram = await _dbContext.RoundPrograms
            .AsNoTracking()
            .Include(roundProgram => roundProgram.DocumentRequirements)
                .ThenInclude(requirement => requirement.DocumentType)
            .FirstOrDefaultAsync(roundProgram => roundProgram.Id == application.RoundProgramId);

        if (roundProgram == null)
        {
            return NotFound();
        }

        var allowedDocumentTypeIds = roundProgram.DocumentRequirements
            .Select(requirement => requirement.DocumentTypeId)
            .ToHashSet();

        var uploadedDocuments = await CreateEffectiveCurrentApplicationDocumentsQuery(id)
            .Where(document => allowedDocumentTypeIds.Contains(document.DocumentTypeId))
            .OrderByDescending(document => document.UploadedAt)
            .ToListAsync();

        var documentsByType = uploadedDocuments
            .GroupBy(document => document.DocumentTypeId)
            .ToDictionary(group => group.Key, group => group.ToList());

        var requirementViewModels = roundProgram.DocumentRequirements
            .OrderBy(requirement => requirement.IsRequired ? 0 : 1)
            .ThenBy(requirement => requirement.DocumentType.DocumentName)
            .Select(requirement =>
            {
                var hasUploaded = documentsByType.TryGetValue(requirement.DocumentTypeId, out var docs) && docs.Count > 0;
                return new DocumentRequirementViewModel
                {
                    DocumentTypeId = requirement.DocumentTypeId,
                    DocumentName = requirement.DocumentType.DocumentName,
                    Description = requirement.DocumentType.Description,
                    IsRequired = requirement.IsRequired,
                    MaxFiles = requirement.MaxFiles,
                    RequiresNotarization = requirement.RequiresNotarization,
                    RequiresOriginalCopy = requirement.RequiresOriginalCopy,
                    Notes = requirement.Notes,
                    UploadedCount = hasUploaded ? docs!.Count : 0,
                    UploadedDocuments = hasUploaded
                        ? docs!.Select(doc => new UploadedDocumentViewModel
                        {
                            DocumentId = doc.Id,
                            FileName = doc.FileName,
                            StoragePath = doc.StoragePath,
                            FileSize = doc.FileSize,
                            UploadedAt = doc.UploadedAt.ToString("dd/MM/yyyy HH:mm"),
                            ValidationStatus = doc.ValidationStatus
                        }).ToList()
                        : []
                };
            })
            .ToList();

        var requiredRequirementIds = roundProgram.DocumentRequirements
            .Where(requirement => requirement.IsRequired)
            .Select(requirement => requirement.DocumentTypeId)
            .ToHashSet();

        var isComplete = requiredRequirementIds.All(requiredId =>
            documentsByType.TryGetValue(requiredId, out var docs) && docs.Count > 0);

        var model = new StudentApplicationDocumentsViewModel
        {
            ApplicationId = id,
            CurrentStatus = application.CurrentStatus,
            IsComplete = isComplete,
            DocumentRequirements = requirementViewModels
        };

        return View("~/Views/Student/Applications/Documents.cshtml", model);
    }

    [HttpPost("/student/applications/{id:guid}/upload")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadDocument(Guid id, Guid documentTypeId, IFormFile? file, CancellationToken cancellationToken)
    {
        var (hasAuthenticatedUserId, candidate) = await ResolveCurrentCandidateAsync();
        if (!hasAuthenticatedUserId) return RedirectToAction("Login", "StudentLogin");
        if (candidate == null) return NotFound();

        var application = await CreateCandidateApplicationsQuery(candidate.Id)
            .FirstOrDefaultAsync(application => application.Id == id, cancellationToken);

        if (application == null)
        {
            return NotFound();
        }

        if (application.CurrentStatus != "DRAFT")
        {
            return BadRequest("Only draft applications can have documents uploaded.");
        }

        if (file == null || file.Length == 0)
        {
            return BadRequest("A PDF file is required.");
        }

        if (file.Length > MaxDocumentFileSizeBytes)
        {
            return BadRequest("PDF files must be 5MB or smaller.");
        }

        var extension = Path.GetExtension(file.FileName);
        if (!string.Equals(extension, ".pdf", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("Only PDF files are allowed.");
        }

        if (!AllowedPdfMimeTypes.Contains(file.ContentType, StringComparer.OrdinalIgnoreCase))
        {
            return BadRequest("Only PDF files are allowed.");
        }

        var documentRequirement = await _dbContext.RoundDocumentRequirements
            .AsNoTracking()
            .Where(requirement => requirement.RoundProgramId == application.RoundProgramId && requirement.DocumentTypeId == documentTypeId)
            .Select(requirement => new
            {
                requirement.DocumentTypeId,
                requirement.MaxFiles
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (documentRequirement == null)
        {
            return BadRequest("The selected document type is not allowed for this application.");
        }

        if (documentRequirement.MaxFiles <= 0)
        {
            return BadRequest("This document type is not accepting uploads.");
        }

        var currentDocumentCount = await CreateEffectiveCurrentApplicationDocumentsQuery(id)
            .Where(document => document.DocumentTypeId == documentTypeId)
            .CountAsync(cancellationToken);

        if (currentDocumentCount >= documentRequirement.MaxFiles)
        {
            return BadRequest($"You can upload up to {documentRequirement.MaxFiles} file(s) for this document type.");
        }

        var uploadedAt = DateTime.UtcNow;
        var uploadedBy = ResolveAuthenticatedUserId();
        string? storagePath = null;

        try
        {
            storagePath = await _documentStorage.StoreAsync(id, documentTypeId, file, cancellationToken);

            var applicationDocument = new ApplicationDocument
            {
                Id = Guid.NewGuid(),
                ApplicationId = id,
                DocumentTypeId = documentTypeId,
                FileName = Path.GetFileName(file.FileName),
                StoragePath = storagePath,
                MimeType = "application/pdf",
                FileSize = file.Length,
                UploadedAt = uploadedAt,
                UploadedBy = uploadedBy,
                ValidationStatus = "PENDING",
                IsLatest = true
            };

            _dbContext.ApplicationDocuments.Add(applicationDocument);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            if (!string.IsNullOrWhiteSpace(storagePath))
            {
                await _documentStorage.DeleteAsync(storagePath, cancellationToken);
            }

            throw;
        }

        return RedirectToAction(nameof(Documents), new { id });
    }

    [HttpPost("/student/applications/{id:guid}/confirm")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmApplication(Guid id, CancellationToken cancellationToken)
    {
        var (hasAuthenticatedUserId, candidate) = await ResolveCurrentCandidateAsync();
        if (!hasAuthenticatedUserId) return RedirectToAction("Login", "StudentLogin");
        if (candidate == null) return NotFound();

        var application = await CreateTrackedCandidateApplicationsQuery(candidate.Id)
            .Include(currentApplication => currentApplication.RoundProgram)
                .ThenInclude(roundProgram => roundProgram.DocumentRequirements)
                    .ThenInclude(requirement => requirement.DocumentType)
            .FirstOrDefaultAsync(currentApplication => currentApplication.Id == id, cancellationToken);

        if (application == null)
        {
            return NotFound();
        }

        if (!string.Equals(application.CurrentStatus, "DRAFT", StringComparison.Ordinal))
        {
            return BadRequest("Only draft applications can be confirmed.");
        }

        var requiredDocumentRequirements = application.RoundProgram.DocumentRequirements
            .Where(requirement => requirement.IsRequired)
            .ToList();

        var allowedDocumentTypeIds = application.RoundProgram.DocumentRequirements
            .Select(requirement => requirement.DocumentTypeId)
            .ToHashSet();

        var uploadedDocumentTypeIds = (await CreateEffectiveCurrentApplicationDocumentsQuery(id)
            .Where(document => allowedDocumentTypeIds.Contains(document.DocumentTypeId))
            .Select(document => document.DocumentTypeId)
            .Distinct()
            .ToListAsync(cancellationToken))
            .ToHashSet();

        var missingDocumentNames = requiredDocumentRequirements
            .Where(requirement => !uploadedDocumentTypeIds.Contains(requirement.DocumentTypeId))
            .Select(requirement => requirement.DocumentType.DocumentName)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(documentName => documentName)
            .ToList();

        if (missingDocumentNames.Count > 0)
        {
            return BadRequest($"Missing required documents: {string.Join(", ", missingDocumentNames)}.");
        }

        var submittedAt = DateTime.UtcNow;
        var changedBy = ResolveAuthenticatedUserId();

        application.CurrentStatus = "SUBMITTED";
        application.SubmissionNumber = 1;
        application.SubmittedAt = submittedAt;
        application.UpdatedAt = submittedAt;

        _dbContext.ApplicationStatusHistories.Add(new ApplicationStatusHistory
        {
            Id = Guid.NewGuid(),
            ApplicationId = application.Id,
            FromStatus = "DRAFT",
            ToStatus = "SUBMITTED",
            ChangedBy = changedBy,
            ChangedAt = submittedAt
        });

        await _dbContext.SaveChangesAsync(cancellationToken);

        return RedirectToAction(nameof(Detail), new { id });
    }

    [HttpDelete("/student/applications/{id:guid}/documents/{documentId:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteDocument(Guid id, Guid documentId, CancellationToken cancellationToken)
    {
        var (hasAuthenticatedUserId, candidate) = await ResolveCurrentCandidateAsync();
        if (!hasAuthenticatedUserId) return RedirectToAction("Login", "StudentLogin");
        if (candidate == null) return NotFound();

        var application = await CreateCandidateApplicationsQuery(candidate.Id)
            .FirstOrDefaultAsync(application => application.Id == id, cancellationToken);

        if (application == null)
        {
            return NotFound();
        }

        if (application.CurrentStatus != "DRAFT")
        {
            return BadRequest("Only documents from draft applications can be deleted.");
        }

        var document = await _dbContext.ApplicationDocuments
            .FirstOrDefaultAsync(currentDocument => currentDocument.Id == documentId && currentDocument.ApplicationId == id, cancellationToken);

        if (document == null)
        {
            return NotFound();
        }

        _dbContext.ApplicationDocuments.Remove(document);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await _documentStorage.DeleteAsync(document.StoragePath, cancellationToken);

        return NoContent();
    }

    private async Task<(bool HasAuthenticatedUserId, Candidate? Candidate)> ResolveCurrentCandidateAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null || !Guid.TryParse(userId, out var parsedUserId))
        {
            return (false, null);
        }

        var candidate = await _dbContext.Candidates
            .FirstOrDefaultAsync(currentCandidate => currentCandidate.UserId == parsedUserId);

        return (true, candidate);
    }

    private IQueryable<AdmissionApplication> CreateCandidateApplicationsQuery(Guid candidateId)
    {
        return _dbContext.AdmissionApplications
            .AsNoTracking()
            .Where(application => application.CandidateId == candidateId);
    }

    private IQueryable<AdmissionApplication> CreateTrackedCandidateApplicationsQuery(Guid candidateId)
    {
        return _dbContext.AdmissionApplications
            .Where(application => application.CandidateId == candidateId);
    }

    private IQueryable<ApplicationDocument> CreateEffectiveCurrentApplicationDocumentsQuery(Guid applicationId)
    {
        return _dbContext.ApplicationDocuments
            .AsNoTracking()
            .Where(document => document.ApplicationId == applicationId && document.IsLatest);
    }

    private Guid? ResolveAuthenticatedUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(userId, out var parsedUserId) ? parsedUserId : null;
    }

    private static bool IsEligibleForDraftCreation(RoundProgram roundProgram, DateTime utcNow)
    {
        return roundProgram.Round.Status == "PUBLISHED"
               && roundProgram.Status == "ACTIVE"
               && roundProgram.Round.StartAt <= utcNow
               && roundProgram.Round.EndAt >= utcNow;
    }

    private async Task<string> GenerateUniqueApplicationCodeAsync()
    {
        while (true)
        {
            var suffix = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
            var applicationCode = $"APP-{DateTime.UtcNow:yyyyMMddHHmmss}-{suffix}";
            var exists = await _dbContext.AdmissionApplications
                .AsNoTracking()
                .AnyAsync(application => application.ApplicationCode == applicationCode);

            if (!exists)
            {
                return applicationCode;
            }
        }
    }

    private static bool IsDuplicateApplicationConflict(DbUpdateException exception)
    {
        return exception.InnerException is SqlException { Number: 2601 or 2627 };
    }
}
