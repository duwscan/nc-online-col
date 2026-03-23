using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using wnc.Data;
using wnc.Features.Admin.Rounds.ViewModels;
using wnc.Models;

namespace wnc.Features.Admin.Rounds.Controllers;

[Authorize(Roles = "ADMIN,ADMISSION_OFFICER")]
public class AdminRoundsController(AppDbContext dbContext) : Controller
{
    private static readonly string[] RoundStatuses = ["DRAFT", "PUBLISHED", "CLOSED"];

    [HttpPost("/admin/rounds/test-debug")]
    [ValidateAntiForgeryToken]
    public IActionResult TestDebug(EditRoundViewModel model)
    {
        var form = HttpContext.Request.Form;
        var addProgramIds = form["addProgramIds"].ToString();
        var removeProgramIds = form["removeRoundProgramIds"].ToString();
        var selectedMethodIds = string.Join(", ", form["SelectedMethodIds"].ToList());

        return Json(new
        {
            modelId = model.Id,
            addProgramIds,
            removeProgramIds,
            selectedMethodIds,
            roundCode = model.RoundCode,
            roundName = model.RoundName
        });
    }

    [HttpPost("/admin/rounds/test-documents-debug")]
    [ValidateAntiForgeryToken]
    public IActionResult TestDocumentsDebug(EditRoundDocumentsViewModel model)
    {
        var form = HttpContext.Request.Form;
        var docKeys = form.Keys.Where(k => k.StartsWith("doc_")).ToList();
        var requiredKeys = form.Keys.Where(k => k.StartsWith("required_")).ToList();

        return Json(new
        {
            roundId = model.RoundId,
            docKeys,
            requiredKeys,
            formKeys = form.Keys.ToList()
        });
    }

    [HttpGet("/admin/rounds")]
    public async Task<IActionResult> Index(string? searchTerm = null, string? status = null, int page = 1)
    {
        var normalizedSearchTerm = string.IsNullOrWhiteSpace(searchTerm) ? null : searchTerm.Trim();
        var normalizedStatus = NormalizeStatus(status);
        var currentPage = page < 1 ? 1 : page;

        var query = dbContext.AdmissionRounds
            .AsNoTracking()
            .Where(round => round.DeletedAt == null);

        if (!string.IsNullOrWhiteSpace(normalizedSearchTerm))
        {
            query = query.Where(round =>
                round.RoundCode.Contains(normalizedSearchTerm) ||
                round.RoundName.Contains(normalizedSearchTerm));
        }

        if (!string.IsNullOrWhiteSpace(normalizedStatus))
        {
            query = query.Where(round => round.Status == normalizedStatus);
        }

        const int pageSize = 10;
        var totalItems = await query.CountAsync();
        var rounds = await query
            .OrderByDescending(round => round.AdmissionYear)
            .ThenByDescending(round => round.StartAt)
            .Skip((currentPage - 1) * pageSize)
            .Take(pageSize)
            .Select(round => new RoundListItemViewModel
            {
                Id = round.Id,
                RoundCode = round.RoundCode,
                RoundName = round.RoundName,
                AdmissionYear = round.AdmissionYear,
                StartAt = round.StartAt,
                EndAt = round.EndAt,
                Status = round.Status
            })
            .ToListAsync();

        var model = new RoundsListViewModel
        {
            Rounds = rounds,
            AvailableStatuses = RoundStatuses,
            SearchTerm = normalizedSearchTerm,
            Status = normalizedStatus,
            Page = currentPage,
            PageSize = pageSize,
            TotalItems = totalItems,
            SuccessMessage = TempData["SuccessMessage"] as string
        };

        return View("~/Features/Admin/Rounds/Views/Index.cshtml", model);
    }

    [HttpGet("/admin/rounds/create")]
    public async Task<IActionResult> Create()
    {
        var availableMethods = await dbContext.AdmissionMethods
            .AsNoTracking()
            .Where(m => m.Status == "ACTIVE")
            .OrderBy(m => m.MethodName)
            .Select(m => new AdmissionMethodOption
            {
                Id = m.Id,
                MethodCode = m.MethodCode,
                MethodName = m.MethodName
            })
            .ToListAsync();

        var availableDocumentTypes = await dbContext.DocumentTypes
            .AsNoTracking()
            .Where(dt => dt.Status == "ACTIVE")
            .OrderBy(dt => dt.DocumentName)
            .Select(dt => new DocumentTypeOption
            {
                Id = dt.Id,
                DocumentCode = dt.DocumentCode,
                DocumentName = dt.DocumentName
            })
            .ToListAsync();

        var model = new CreateRoundViewModel
        {
            AvailableStatuses = RoundStatuses,
            AvailableMethods = availableMethods,
            AvailableDocumentTypes = availableDocumentTypes
        };

        return View("~/Features/Admin/Rounds/Views/Create.cshtml", model);
    }

    [HttpPost("/admin/rounds/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateRoundViewModel model)
    {
        Normalize(model);
        await ValidateRoundInputAsync(model.RoundCode, model.Status, null);

        if (!ModelState.IsValid)
        {
            model.AvailableStatuses = RoundStatuses;
            model.AvailableMethods = await dbContext.AdmissionMethods
                .AsNoTracking()
                .Where(m => m.Status == "ACTIVE")
                .OrderBy(m => m.MethodName)
                .Select(m => new AdmissionMethodOption
                {
                    Id = m.Id,
                    MethodCode = m.MethodCode,
                    MethodName = m.MethodName
                })
                .ToListAsync();
            model.AvailableDocumentTypes = await dbContext.DocumentTypes
                .AsNoTracking()
                .Where(dt => dt.Status == "ACTIVE")
                .OrderBy(dt => dt.DocumentName)
                .Select(dt => new DocumentTypeOption
                {
                    Id = dt.Id,
                    DocumentCode = dt.DocumentCode,
                    DocumentName = dt.DocumentName
                })
                .ToListAsync();
            return View("~/Features/Admin/Rounds/Views/Create.cshtml", model);
        }

        var now = DateTime.UtcNow;
        var round = new AdmissionRound
        {
            Id = Guid.NewGuid(),
            RoundCode = model.RoundCode,
            RoundName = model.RoundName,
            AdmissionYear = model.AdmissionYear,
            StartAt = model.StartAt,
            EndAt = model.EndAt,
            Status = model.Status,
            Notes = model.Notes,
            AllowEnrollmentConfirmation = model.AllowEnrollmentConfirmation,
            CreatedBy = GetCurrentUserId(),
            CreatedAt = now,
            UpdatedAt = now,
            DeletedAt = null
        };

        dbContext.AdmissionRounds.Add(round);
        await dbContext.SaveChangesAsync();

        TempData["SuccessMessage"] = "Tạo đợt xét tuyển thành công.";
        return Redirect("/admin/rounds");
    }

    [HttpGet("/admin/rounds/edit/{id:guid}")]
    public async Task<IActionResult> Edit(Guid id)
    {
        ViewData["CurrentRoundId"] = id;

        var round = await dbContext.AdmissionRounds
            .AsNoTracking()
            .Include(r => r.RoundPrograms)
                .ThenInclude(rp => rp.Program)
            .Include(r => r.RoundPrograms)
                .ThenInclude(rp => rp.AdmissionMethods)
            .SingleOrDefaultAsync(item => item.Id == id && item.DeletedAt == null);

        if (round is null)
        {
            return NotFound();
        }

        var availablePrograms = await dbContext.TrainingPrograms
            .AsNoTracking()
            .Where(p => p.Status == "ACTIVE")
            .OrderBy(p => p.DisplayOrder)
            .ThenBy(p => p.ProgramName)
            .Select(p => new ProgramOptionViewModel
            {
                Id = p.Id,
                ProgramCode = p.ProgramCode,
                ProgramName = p.ProgramName,
                EducationType = p.EducationType
            })
            .ToListAsync();

        var availableMethods = await dbContext.AdmissionMethods
            .AsNoTracking()
            .Where(m => m.Status == "ACTIVE")
            .OrderBy(m => m.MethodName)
            .Select(m => new AdmissionMethodOption
            {
                Id = m.Id,
                MethodCode = m.MethodCode,
                MethodName = m.MethodName
            })
            .ToListAsync();

        var model = new EditRoundViewModel
        {
            Id = round.Id,
            RoundCode = round.RoundCode,
            RoundName = round.RoundName,
            AdmissionYear = round.AdmissionYear,
            StartAt = round.StartAt,
            EndAt = round.EndAt,
            Status = round.Status,
            Notes = round.Notes,
            AllowEnrollmentConfirmation = round.AllowEnrollmentConfirmation,
            AvailableStatuses = RoundStatuses,
            AssignedPrograms = round.RoundPrograms.Select(rp => new RoundProgramItemViewModel
            {
                Id = rp.Id,
                ProgramId = rp.ProgramId,
                ProgramCode = rp.Program.ProgramCode,
                ProgramName = rp.Program.ProgramName,
                EducationType = rp.Program.EducationType,
                Quota = rp.Quota,
                PublishedQuota = rp.PublishedQuota,
                Status = rp.Status,
                AssignedMethodIds = rp.AdmissionMethods.Select(a => a.Id).ToList()
            }).ToList(),
            AvailablePrograms = availablePrograms,
            AvailableMethods = availableMethods,
            SelectedMethodIds = round.RoundPrograms
                .SelectMany(rp => rp.AdmissionMethods.Select(a => a.Id))
                .Distinct()
                .ToList()
        };

        return View("~/Features/Admin/Rounds/Views/Edit.cshtml", model);
    }

    [HttpPost("/admin/rounds/edit/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, EditRoundViewModel model)
    {
        if (id != model.Id)
        {
            return NotFound();
        }

        Normalize(model);
        await ValidateRoundInputAsync(model.RoundCode, model.Status, id);

        var round = await dbContext.AdmissionRounds
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == id && item.DeletedAt == null);

        if (round is null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return await LoadEditViewModel(id, model);
        }

        // Update the round entity first
        dbContext.AdmissionRounds.Update(round);
        round.RoundCode = model.RoundCode;
        round.RoundName = model.RoundName;
        round.AdmissionYear = model.AdmissionYear;
        round.StartAt = model.StartAt;
        round.EndAt = model.EndAt;
        round.Status = model.Status;
        round.Notes = model.Notes;
        round.AllowEnrollmentConfirmation = model.AllowEnrollmentConfirmation;
        round.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();

        // Now handle programs separately
        await HandleRoundProgramsAsync(id, model);

        TempData["SuccessMessage"] = "Cập nhật đợt xét tuyển thành công.";
        return Redirect("/admin/rounds");
    }

    private async Task HandleRoundProgramsAsync(Guid roundId, EditRoundViewModel model)
    {
        var form = HttpContext.Request.Form;
        var addedProgramIdsRaw = form["addProgramIds"].FirstOrDefault() ?? string.Empty;
        var addedProgramIds = addedProgramIdsRaw
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Where(v => Guid.TryParse(v.Trim(), out _))
            .Select(v => Guid.Parse(v.Trim()))
            .ToHashSet();
        var removedRoundProgramIdsRaw = form["removeRoundProgramIds"].FirstOrDefault() ?? string.Empty;
        var removedRoundProgramIds = removedRoundProgramIdsRaw
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Where(v => Guid.TryParse(v.Trim(), out _))
            .Select(v => Guid.Parse(v.Trim()))
            .ToHashSet();

        // Handle removals
        if (removedRoundProgramIds.Count > 0)
        {
            // First delete related RoundAdmissionMethods
            var relatedMethods = await dbContext.RoundAdmissionMethods
                .Where(ram => removedRoundProgramIds.Contains(ram.RoundProgramId))
                .ToListAsync();
            dbContext.RoundAdmissionMethods.RemoveRange(relatedMethods);

            // Then delete related RoundDocumentRequirements
            var relatedDocs = await dbContext.RoundDocumentRequirements
                .Where(rdr => removedRoundProgramIds.Contains(rdr.RoundProgramId))
                .ToListAsync();
            dbContext.RoundDocumentRequirements.RemoveRange(relatedDocs);

            await dbContext.SaveChangesAsync();

            // Now delete the RoundPrograms
            var toRemove = await dbContext.RoundPrograms
                .Where(rp => removedRoundProgramIds.Contains(rp.Id))
                .ToListAsync();
            dbContext.RoundPrograms.RemoveRange(toRemove);
            await dbContext.SaveChangesAsync();
        }

        // Handle additions
        if (addedProgramIds.Count > 0)
        {
            var existingProgramIds = await dbContext.RoundPrograms
                .Where(rp => rp.RoundId == roundId)
                .Select(rp => rp.ProgramId)
                .ToListAsync();
            var toAdd = addedProgramIds.Where(pid => !existingProgramIds.Contains(pid)).ToList();

            foreach (var programId in toAdd)
            {
                var program = await dbContext.TrainingPrograms.FindAsync(programId);
                if (program != null)
                {
                    var newRp = new RoundProgram
                    {
                        Id = Guid.NewGuid(),
                        RoundId = roundId,
                        ProgramId = programId,
                        Quota = program.Quota,
                        PublishedQuota = program.Quota,
                        Status = "ACTIVE",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    dbContext.RoundPrograms.Add(newRp);
                }
            }
            await dbContext.SaveChangesAsync();
        }

        // Handle methods - RoundAdmissionMethod join table
        var selectedMethodIdsRaw = form["SelectedMethodIds"].ToList();
        var selectedMethodIds = selectedMethodIdsRaw
            .Where(v => !string.IsNullOrEmpty(v) && Guid.TryParse(v, out _))
            .Select(v => Guid.Parse(v!))
            .ToHashSet();

        var roundPrograms = await dbContext.RoundPrograms
            .Include(rp => rp.AdmissionMethods)
            .Where(rp => rp.RoundId == roundId)
            .ToListAsync();

        foreach (var rp in roundPrograms)
        {
            var existingMethodIds = rp.AdmissionMethods.Select(a => a.MethodId).ToHashSet();

            // Add new methods
            foreach (var methodId in selectedMethodIds.Where(id => !existingMethodIds.Contains(id)))
            {
                dbContext.RoundAdmissionMethods.Add(new RoundAdmissionMethod
                {
                    Id = Guid.NewGuid(),
                    RoundProgramId = rp.Id,
                    MethodId = methodId,
                    Status = "ACTIVE",
                    CreatedAt = DateTime.UtcNow
                });
            }

            // Remove deselected methods
            var toRemove = rp.AdmissionMethods
                .Where(a => !selectedMethodIds.Contains(a.MethodId))
                .ToList();
            dbContext.RoundAdmissionMethods.RemoveRange(toRemove);
        }

        await dbContext.SaveChangesAsync();

        // Handle document requirements
        var selectedDocumentTypeIdsRaw = form["SelectedDocumentTypeIds"].ToList();
        var selectedDocumentTypeIds = selectedDocumentTypeIdsRaw
            .Where(v => !string.IsNullOrEmpty(v) && Guid.TryParse(v, out _))
            .Select(v => Guid.Parse(v!))
            .ToHashSet();

        var roundProgramsWithDocs = await dbContext.RoundPrograms
            .Where(rp => rp.RoundId == roundId)
            .ToListAsync();

        foreach (var rp in roundProgramsWithDocs)
        {
            var existingDocs = await dbContext.RoundDocumentRequirements
                .Where(rdr => rdr.RoundProgramId == rp.Id)
                .Select(rdr => rdr.DocumentTypeId)
                .ToListAsync();

            var existingDocIds = existingDocs.ToHashSet();

            // Add new document requirements
            foreach (var docTypeId in selectedDocumentTypeIds.Where(did => !existingDocIds.Contains(did)))
            {
                dbContext.RoundDocumentRequirements.Add(new RoundDocumentRequirement
                {
                    Id = Guid.NewGuid(),
                    RoundProgramId = rp.Id,
                    DocumentTypeId = docTypeId,
                    IsRequired = true,
                    RequiresNotarization = false,
                    RequiresOriginalCopy = false,
                    MaxFiles = 1,
                    CreatedAt = DateTime.UtcNow
                });
            }

            // Remove deselected document requirements
            var docsToRemove = await dbContext.RoundDocumentRequirements
                .Where(rdr => rdr.RoundProgramId == rp.Id && !selectedDocumentTypeIds.Contains(rdr.DocumentTypeId))
                .ToListAsync();
            dbContext.RoundDocumentRequirements.RemoveRange(docsToRemove);
        }

        await dbContext.SaveChangesAsync();
    }

    private async Task<IActionResult> LoadEditViewModel(Guid id, EditRoundViewModel? model = null)
    {
        var round = await dbContext.AdmissionRounds
            .AsNoTracking()
            .Include(r => r.RoundPrograms)
                .ThenInclude(rp => rp.Program)
            .Include(r => r.RoundPrograms)
                .ThenInclude(rp => rp.AdmissionMethods)
            .Include(r => r.RoundPrograms)
                .ThenInclude(rp => rp.DocumentRequirements)
            .SingleOrDefaultAsync(item => item.Id == id && item.DeletedAt == null);

        if (round is null)
        {
            return NotFound();
        }

        var availablePrograms = await dbContext.TrainingPrograms
            .AsNoTracking()
            .Where(p => p.Status == "ACTIVE")
            .OrderBy(p => p.DisplayOrder)
            .ThenBy(p => p.ProgramName)
            .Select(p => new ProgramOptionViewModel
            {
                Id = p.Id,
                ProgramCode = p.ProgramCode,
                ProgramName = p.ProgramName,
                EducationType = p.EducationType
            })
            .ToListAsync();

        var availableMethods = await dbContext.AdmissionMethods
            .AsNoTracking()
            .Where(m => m.Status == "ACTIVE")
            .OrderBy(m => m.MethodName)
            .Select(m => new AdmissionMethodOption
            {
                Id = m.Id,
                MethodCode = m.MethodCode,
                MethodName = m.MethodName
            })
            .ToListAsync();

        var availableDocumentTypes = await dbContext.DocumentTypes
            .AsNoTracking()
            .Where(dt => dt.Status == "ACTIVE")
            .OrderBy(dt => dt.DocumentName)
            .Select(dt => new DocumentTypeOption
            {
                Id = dt.Id,
                DocumentCode = dt.DocumentCode,
                DocumentName = dt.DocumentName
            })
            .ToListAsync();

        var viewModel = model ?? new EditRoundViewModel();
        viewModel.AvailableStatuses = RoundStatuses;
        viewModel.AssignedPrograms = round.RoundPrograms.Select(rp => new RoundProgramItemViewModel
        {
            Id = rp.Id,
            ProgramId = rp.ProgramId,
            ProgramCode = rp.Program.ProgramCode,
            ProgramName = rp.Program.ProgramName,
            EducationType = rp.Program.EducationType,
            Quota = rp.Quota,
            PublishedQuota = rp.PublishedQuota,
            Status = rp.Status,
            AssignedMethodIds = rp.AdmissionMethods.Select(a => a.Id).ToList(),
            AssignedDocumentTypeIds = rp.DocumentRequirements.Select(rdr => rdr.DocumentTypeId).ToList()
        }).ToList();
        viewModel.AvailablePrograms = availablePrograms;
        viewModel.AvailableMethods = availableMethods;
        viewModel.AvailableDocumentTypes = availableDocumentTypes;
        viewModel.SelectedMethodIds = round.RoundPrograms
            .SelectMany(rp => rp.AdmissionMethods.Select(a => a.Id))
            .Distinct()
            .ToList();
        viewModel.SelectedDocumentTypeIds = round.RoundPrograms
            .SelectMany(rp => rp.DocumentRequirements.Select(rdr => rdr.DocumentTypeId))
            .Distinct()
            .ToList();

        return View("~/Features/Admin/Rounds/Views/Edit.cshtml", viewModel);
    }

    [HttpPost("/admin/rounds/delete/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var round = await dbContext.AdmissionRounds
            .SingleOrDefaultAsync(item => item.Id == id && item.DeletedAt == null);

        if (round is null)
        {
            return NotFound();
        }

        round.DeletedAt = DateTime.UtcNow;
        round.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();

        TempData["SuccessMessage"] = "Xóa đợt xét tuyển thành công.";
        return Redirect("/admin/rounds");
    }

    [HttpGet("/admin/rounds/{roundId:guid}/documents")]
    public async Task<IActionResult> EditDocuments(Guid roundId)
    {
        var round = await dbContext.AdmissionRounds
            .AsNoTracking()
            .Include(r => r.RoundPrograms)
                .ThenInclude(rp => rp.Program)
            .Include(r => r.RoundPrograms)
                .ThenInclude(rp => rp.Major)
            .Include(r => r.RoundPrograms)
                .ThenInclude(rp => rp.DocumentRequirements)
                    .ThenInclude(rdr => rdr.DocumentType)
            .FirstOrDefaultAsync(r => r.Id == roundId && r.DeletedAt == null);

        if (round is null)
            return NotFound();

        var documentTypes = await dbContext.DocumentTypes
            .Where(dt => dt.Status == "ACTIVE")
            .AsNoTracking()
            .ToListAsync();

        var model = new EditRoundDocumentsViewModel
        {
            RoundId = round.Id,
            RoundCode = round.RoundCode,
            RoundName = round.RoundName,
            Programs = round.RoundPrograms.Select(rp => new RoundProgramDocumentsViewModel
            {
                Id = rp.Id,
                ProgramName = rp.Program.ProgramName,
                MajorName = rp.Major?.MajorName,
                Requirements = rp.DocumentRequirements.Select(rdr => new RoundDocumentRequirementViewModel
                {
                    Id = rdr.Id,
                    RoundProgramId = rdr.RoundProgramId,
                    DocumentTypeId = rdr.DocumentTypeId,
                    DocumentName = rdr.DocumentType.DocumentName,
                    IsRequired = rdr.IsRequired,
                    RequiresNotarization = rdr.RequiresNotarization,
                    RequiresOriginalCopy = rdr.RequiresOriginalCopy,
                    MaxFiles = rdr.MaxFiles,
                    Notes = rdr.Notes
                }).ToList(),
                AvailableDocumentTypes = documentTypes.Select(dt => new DocumentTypeOption
                {
                    Id = dt.Id,
                    DocumentName = dt.DocumentName,
                    DocumentCode = dt.DocumentCode
                }).ToList()
            }).ToList()
        };

        return View("~/Features/Admin/Rounds/Views/EditDocuments.cshtml", model);
    }

    [HttpPost("/admin/rounds/{roundId:guid}/documents")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditDocumentsSave(Guid roundId, EditRoundDocumentsViewModel model)
    {
        var roundExists = await dbContext.AdmissionRounds
            .AnyAsync(r => r.Id == roundId && r.DeletedAt == null);

        if (!roundExists)
            return NotFound();

        var form = HttpContext.Request.Form;

        // Get all round programs for this round
        var roundPrograms = await dbContext.RoundPrograms
            .Where(rp => rp.RoundId == roundId)
            .ToListAsync();

        // Parse submitted document type IDs - keys like "doc_<docTypeId>"
        var submittedDocTypeIds = form.Keys
            .Where(k => k.StartsWith("doc_"))
            .Select(k => k.Replace("doc_", ""))
            .Where(k => Guid.TryParse(k, out _))
            .Select(k => Guid.Parse(k))
            .Distinct()
            .ToList();

        foreach (var rp in roundPrograms)
        {
            // Get existing requirements for this round program
            var existingReqs = await dbContext.RoundDocumentRequirements
                .Where(rdr => rdr.RoundProgramId == rp.Id)
                .ToListAsync();

            // Find submitted doc type IDs for this program
            var submittedForRp = submittedDocTypeIds
                .Where(docTypeId => form.Keys.Contains($"doc_{docTypeId}"))
                .ToList();

            // Remove deselected
            var toRemove = existingReqs
                .Where(rdr => !submittedForRp.Contains(rdr.DocumentTypeId))
                .ToList();
            dbContext.RoundDocumentRequirements.RemoveRange(toRemove);

            // Add or update
            foreach (var docTypeId in submittedForRp)
            {
                var isRequired = form[$"required_{rp.Id}_{docTypeId}"].FirstOrDefault() == "true";
                var requiresNotarization = form[$"notarization_{rp.Id}_{docTypeId}"].FirstOrDefault() == "true";
                var requiresOriginalCopy = form[$"original_{rp.Id}_{docTypeId}"].FirstOrDefault() == "true";
                var maxFilesStr = form[$"maxfiles_{rp.Id}_{docTypeId}"].FirstOrDefault();
                int.TryParse(maxFilesStr, out var maxFiles);

                var existing = existingReqs.FirstOrDefault(rdr => rdr.DocumentTypeId == docTypeId);
                if (existing != null)
                {
                    existing.IsRequired = isRequired;
                    existing.RequiresNotarization = requiresNotarization;
                    existing.RequiresOriginalCopy = requiresOriginalCopy;
                    existing.MaxFiles = maxFiles > 0 ? maxFiles : 1;
                }
                else
                {
                    dbContext.RoundDocumentRequirements.Add(new RoundDocumentRequirement
                    {
                        Id = Guid.NewGuid(),
                        RoundProgramId = rp.Id,
                        DocumentTypeId = docTypeId,
                        IsRequired = isRequired,
                        RequiresNotarization = requiresNotarization,
                        RequiresOriginalCopy = requiresOriginalCopy,
                        MaxFiles = maxFiles > 0 ? maxFiles : 1,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
        }

        await dbContext.SaveChangesAsync();
        TempData["SuccessMessage"] = "Cập nhật yêu cầu hồ sơ thành công.";
        return Redirect($"/admin/rounds/{roundId}/documents");
    }

    [HttpGet("/admin/rounds/{roundId:guid}/methods")]
    public async Task<IActionResult> EditMethods(Guid roundId)
    {
        var round = await dbContext.AdmissionRounds
            .AsNoTracking()
            .Include(r => r.RoundPrograms)
                .ThenInclude(rp => rp.Program)
            .Include(r => r.RoundPrograms)
                .ThenInclude(rp => rp.Major)
            .Include(r => r.RoundPrograms)
                .ThenInclude(rp => rp.AdmissionMethods)
                    .ThenInclude(ram => ram.Method)
            .FirstOrDefaultAsync(r => r.Id == roundId && r.DeletedAt == null);

        if (round is null)
            return NotFound();

        var methods = await dbContext.AdmissionMethods
            .Where(m => m.Status == "ACTIVE")
            .AsNoTracking()
            .ToListAsync();

        var model = new EditRoundMethodsViewModel
        {
            RoundId = round.Id,
            RoundCode = round.RoundCode,
            RoundName = round.RoundName,
            Programs = round.RoundPrograms.Select(rp => new RoundProgramMethodsViewModel
            {
                Id = rp.Id,
                ProgramName = rp.Program.ProgramName,
                MajorName = rp.Major?.MajorName,
                Methods = rp.AdmissionMethods.Select(ram => new RoundMethodViewModel
                {
                    Id = ram.Id,
                    RoundProgramId = rp.Id,
                    MethodId = ram.MethodId,
                    MethodName = ram.Method.MethodName,
                    MethodCode = ram.Method.MethodCode,
                    MinimumScore = ram.MinimumScore,
                    CalculationRule = ram.CalculationRule,
                    CombinationCode = ram.CombinationCode,
                    PriorityPolicy = ram.PriorityPolicy,
                    Status = ram.Status
                }).ToList(),
                AvailableMethods = methods.Select(m => new AdmissionMethodOption
                {
                    Id = m.Id,
                    MethodName = m.MethodName,
                    MethodCode = m.MethodCode
                }).ToList()
            }).ToList()
        };

        return View("~/Features/Admin/Rounds/Views/EditMethods.cshtml", model);
    }

    [HttpPost("/admin/rounds/{roundId:guid}/methods")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditMethodsSave(Guid roundId, EditRoundMethodsViewModel model)
    {
        var round = await dbContext.AdmissionRounds
            .Include(r => r.RoundPrograms)
                .ThenInclude(rp => rp.AdmissionMethods)
                    .ThenInclude(ram => ram.Method)
            .FirstOrDefaultAsync(r => r.Id == roundId && r.DeletedAt == null);

        if (round is null)
            return NotFound();

        var form = HttpContext.Request.Form;
        var submittedMethodIds = form.Keys
            .Where(k => k.StartsWith("method_"))
            .Select(k => Guid.Parse(k.Replace("method_", "")))
            .Distinct()
            .ToList();

        foreach (var rp in round.RoundPrograms)
        {
            var existingMethodIds = rp.AdmissionMethods.Select(ram => ram.MethodId).ToHashSet();
            var submittedForRp = submittedMethodIds
                .Where(methodId => form.Keys.Contains($"method_{methodId}_{rp.Id}"))
                .ToHashSet();

            // Remove deselected
            var methodsToRemove = rp.AdmissionMethods
                .Where(ram => !submittedForRp.Contains(ram.MethodId))
                .ToList();
            foreach (var ram in methodsToRemove)
                dbContext.RoundAdmissionMethods.Remove(ram);

            // Add new
            foreach (var methodId in submittedForRp.Where(id => !existingMethodIds.Contains(id)))
            {
                dbContext.RoundAdmissionMethods.Add(new RoundAdmissionMethod
                {
                    Id = Guid.NewGuid(),
                    RoundProgramId = rp.Id,
                    MethodId = methodId,
                    Status = "ACTIVE",
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        await dbContext.SaveChangesAsync();
        TempData["SuccessMessage"] = "Cập nhật phương thức xét tuyển thành công.";
        return Redirect($"/admin/rounds/{roundId}/methods");
    }

    private async Task ValidateRoundInputAsync(string roundCode, string? status, Guid? currentRoundId)
    {
        if (!RoundStatuses.Contains(NormalizeStatus(status) ?? string.Empty, StringComparer.OrdinalIgnoreCase))
        {
            ModelState.AddModelError("Status", "Trạng thái không hợp lệ.");
        }

        var duplicateRoundCodeExists = await dbContext.AdmissionRounds
            .AnyAsync(round =>
                round.RoundCode == roundCode &&
                round.Id != currentRoundId &&
                round.DeletedAt == null);

        if (duplicateRoundCodeExists)
        {
            ModelState.AddModelError("RoundCode", "Mã đợt xét tuyển đã tồn tại.");
        }
    }

    private Guid? GetCurrentUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(userId, out var parsedUserId) ? parsedUserId : null;
    }

    private static string? NormalizeStatus(string? status)
    {
        return string.IsNullOrWhiteSpace(status) ? null : status.Trim().ToUpperInvariant();
    }

    private static void Normalize(CreateRoundViewModel model)
    {
        model.RoundCode = model.RoundCode.Trim().ToUpperInvariant();
        model.RoundName = model.RoundName.Trim();
        model.Status = NormalizeStatus(model.Status) ?? "DRAFT";
        model.Notes = string.IsNullOrWhiteSpace(model.Notes) ? null : model.Notes.Trim();
    }

    private static void Normalize(EditRoundViewModel model)
    {
        model.RoundCode = model.RoundCode.Trim().ToUpperInvariant();
        model.RoundName = model.RoundName.Trim();
        model.Status = NormalizeStatus(model.Status) ?? "DRAFT";
        model.Notes = string.IsNullOrWhiteSpace(model.Notes) ? null : model.Notes.Trim();
    }
}
