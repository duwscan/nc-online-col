using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using wnc.Data;
using wnc.Features.Admin.Applications.ViewModels;
using wnc.Models;

namespace wnc.Features.Admin.Applications.Controllers;

[Authorize(Roles = "ADMIN,ADMISSION_OFFICER")]
public class AdminApplicationsController(AppDbContext dbContext) : Controller
{
    private static readonly string[] ApplicationStatuses = ["DRAFT", "SUBMITTED", "UNDER_REVIEW", "APPROVED", "REJECTED", "CANCELLED"];
    private static readonly string[] ManageableStatuses = ["SUBMITTED", "UNDER_REVIEW", "APPROVED", "REJECTED", "CANCELLED"];

    [HttpGet("/admin/applications")]
    public async Task<IActionResult> Index(string? searchTerm = null, string? status = null, Guid? roundId = null, int page = 1)
    {
        var normalizedSearchTerm = string.IsNullOrWhiteSpace(searchTerm) ? null : searchTerm.Trim();
        var normalizedStatus = NormalizeStatus(status);
        var normalizedRoundId = roundId == Guid.Empty ? null : roundId;
        var currentPage = page < 1 ? 1 : page;

        var availableRounds = await dbContext.AdmissionRounds
            .AsNoTracking()
            .Where(round => round.DeletedAt == null)
            .OrderByDescending(round => round.AdmissionYear)
            .ThenByDescending(round => round.StartAt)
            .Select(round => new AdminApplicationRoundOptionViewModel
            {
                Id = round.Id,
                RoundName = $"{round.RoundName} ({round.RoundCode})"
            })
            .ToListAsync();

        if (normalizedRoundId.HasValue && availableRounds.All(round => round.Id != normalizedRoundId.Value))
        {
            normalizedRoundId = null;
        }

        var filteredQuery = CreateApplicationsQuery();

        if (!string.IsNullOrWhiteSpace(normalizedSearchTerm))
        {
            filteredQuery = ApplySearch(filteredQuery, normalizedSearchTerm);
        }

        if (!string.IsNullOrWhiteSpace(normalizedStatus))
        {
            filteredQuery = filteredQuery.Where(application => application.CurrentStatus == normalizedStatus);
        }

        if (normalizedRoundId.HasValue)
        {
            filteredQuery = filteredQuery.Where(application => application.RoundProgram.RoundId == normalizedRoundId.Value);
        }

        var summarySourceQuery = CreateApplicationsQuery();

        if (!string.IsNullOrWhiteSpace(normalizedSearchTerm))
        {
            summarySourceQuery = ApplySearch(summarySourceQuery, normalizedSearchTerm);
        }

        if (normalizedRoundId.HasValue)
        {
            summarySourceQuery = summarySourceQuery.Where(application => application.RoundProgram.RoundId == normalizedRoundId.Value);
        }

        var groupedStatusCounts = await summarySourceQuery
            .GroupBy(application => application.CurrentStatus)
            .Select(group => new { Status = group.Key, Count = group.Count() })
            .ToListAsync();

        const int pageSize = 12;
        var totalItems = await filteredQuery.CountAsync();
        var applications = await filteredQuery
            .OrderByDescending(application => application.SubmittedAt ?? application.CreatedAt)
            .ThenByDescending(application => application.CreatedAt)
            .Skip((currentPage - 1) * pageSize)
            .Take(pageSize)
            .Select(application => new AdminApplicationListItemViewModel
            {
                Id = application.Id,
                ApplicationCode = application.ApplicationCode,
                CandidateName = application.Candidate.FullName,
                CandidateEmail = application.Candidate.Email,
                CandidatePhoneNumber = application.Candidate.PhoneNumber,
                RoundName = application.RoundProgram.Round.RoundName,
                ProgramName = application.RoundProgram.Program.ProgramName,
                MajorName = application.RoundProgram.Major != null ? application.RoundProgram.Major.MajorName : null,
                CurrentStatus = application.CurrentStatus,
                CreatedAt = application.CreatedAt,
                SubmittedAt = application.SubmittedAt
            })
            .ToListAsync();

        var groupedCounts = groupedStatusCounts.ToDictionary(item => item.Status, item => item.Count, StringComparer.OrdinalIgnoreCase);

        var model = new AdminApplicationsIndexViewModel
        {
            Applications = applications,
            AvailableStatuses = ApplicationStatuses,
            AvailableRounds = availableRounds,
            SearchTerm = normalizedSearchTerm,
            Status = normalizedStatus,
            RoundId = normalizedRoundId,
            Page = currentPage,
            PageSize = pageSize,
            TotalItems = totalItems,
            SuccessMessage = TempData["SuccessMessage"] as string,
            ErrorMessage = TempData["ErrorMessage"] as string,
            StatusSummary = new AdminApplicationStatusSummaryViewModel
            {
                Total = groupedCounts.Values.Sum(),
                Draft = GetStatusCount(groupedCounts, "DRAFT"),
                Submitted = GetStatusCount(groupedCounts, "SUBMITTED"),
                UnderReview = GetStatusCount(groupedCounts, "UNDER_REVIEW"),
                Approved = GetStatusCount(groupedCounts, "APPROVED"),
                Rejected = GetStatusCount(groupedCounts, "REJECTED"),
                Cancelled = GetStatusCount(groupedCounts, "CANCELLED")
            }
        };

        return View("~/Features/Admin/Applications/Views/Index.cshtml", model);
    }

    [HttpGet("/admin/applications/{id:guid}")]
    public async Task<IActionResult> Detail(Guid id)
    {
        var application = await dbContext.AdmissionApplications
            .AsNoTracking()
            .Include(currentApplication => currentApplication.Candidate)
            .Include(currentApplication => currentApplication.RoundProgram)
                .ThenInclude(roundProgram => roundProgram.Round)
            .Include(currentApplication => currentApplication.RoundProgram)
                .ThenInclude(roundProgram => roundProgram.Program)
            .Include(currentApplication => currentApplication.RoundProgram)
                .ThenInclude(roundProgram => roundProgram.Major)
            .FirstOrDefaultAsync(currentApplication => currentApplication.Id == id);

        if (application is null)
        {
            return NotFound();
        }

        var documents = await dbContext.ApplicationDocuments
            .AsNoTracking()
            .Include(document => document.DocumentType)
            .Where(document => document.ApplicationId == id && document.IsLatest)
            .OrderBy(document => document.DocumentType.DocumentName)
            .ThenByDescending(document => document.UploadedAt)
            .Select(document => new AdminApplicationDocumentViewModel
            {
                DocumentId = document.Id,
                DocumentTypeName = document.DocumentType.DocumentName,
                FileName = document.FileName,
                StoragePath = document.StoragePath,
                FileSize = document.FileSize,
                ValidationStatus = document.ValidationStatus,
                UploadedAt = document.UploadedAt
            })
            .ToListAsync();

        var timeline = await dbContext.ApplicationStatusHistories
            .AsNoTracking()
            .Include(entry => entry.ChangedByUser)
            .Where(entry => entry.ApplicationId == id)
            .OrderByDescending(entry => entry.ChangedAt)
            .Select(entry => new AdminApplicationTimelineEntryViewModel
            {
                FromStatus = entry.FromStatus,
                ToStatus = entry.ToStatus,
                ChangedAt = entry.ChangedAt,
                ChangedByName = entry.ChangedByUser != null
                    ? entry.ChangedByUser.Username ?? entry.ChangedByUser.Email ?? entry.ChangedByUser.PhoneNumber
                    : null,
                Reason = entry.Reason,
                PublicNote = entry.PublicNote,
                InternalNote = entry.InternalNote
            })
            .ToListAsync();

        var model = new AdminApplicationDetailViewModel
        {
            ApplicationId = application.Id,
            ApplicationCode = application.ApplicationCode,
            CurrentStatus = application.CurrentStatus,
            CandidateName = application.Candidate.FullName,
            CandidateEmail = application.Candidate.Email,
            CandidatePhoneNumber = application.Candidate.PhoneNumber,
            CandidateAddress = FormatAddress(application.Candidate),
            RoundName = application.RoundProgram.Round.RoundName,
            RoundCode = application.RoundProgram.Round.RoundCode,
            ProgramName = application.RoundProgram.Program.ProgramName,
            MajorName = application.RoundProgram.Major?.MajorName,
            CreatedAt = application.CreatedAt,
            SubmittedAt = application.SubmittedAt,
            RejectionReason = application.RejectionReason,
            Documents = documents,
            Timeline = timeline,
            SuccessMessage = TempData["SuccessMessage"] as string,
            ErrorMessage = TempData["ErrorMessage"] as string,
            StatusUpdate = new AdminApplicationStatusUpdateInputModel
            {
                ApplicationId = application.Id,
                CurrentStatus = application.CurrentStatus,
                NewStatus = application.CurrentStatus,
                AvailableStatuses = GetAvailableTargetStatuses(application.CurrentStatus)
            }
        };

        return View("~/Features/Admin/Applications/Views/Detail.cshtml", model);
    }

    [HttpPost("/admin/applications/{id:guid}/status")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(Guid id, AdminApplicationStatusUpdateInputModel model)
    {
        if (id != model.ApplicationId)
        {
            return NotFound();
        }

        var application = await dbContext.AdmissionApplications
            .FirstOrDefaultAsync(currentApplication => currentApplication.Id == id);

        if (application is null)
        {
            return NotFound();
        }

        var normalizedTargetStatus = NormalizeStatus(model.NewStatus);
        if (string.IsNullOrWhiteSpace(normalizedTargetStatus))
        {
            TempData["ErrorMessage"] = "Trạng thái mới không hợp lệ.";
            return Redirect($"/admin/applications/{id}");
        }

        if (!CanTransition(application.CurrentStatus, normalizedTargetStatus))
        {
            TempData["ErrorMessage"] = "Không thể chuyển hồ sơ sang trạng thái đã chọn.";
            return Redirect($"/admin/applications/{id}");
        }

        var normalizedReason = string.IsNullOrWhiteSpace(model.Reason) ? null : model.Reason.Trim();
        var normalizedPublicNote = string.IsNullOrWhiteSpace(model.PublicNote) ? null : model.PublicNote.Trim();
        var normalizedInternalNote = string.IsNullOrWhiteSpace(model.InternalNote) ? null : model.InternalNote.Trim();

        if (string.Equals(normalizedTargetStatus, "REJECTED", StringComparison.Ordinal) && string.IsNullOrWhiteSpace(normalizedReason))
        {
            TempData["ErrorMessage"] = "Cần nhập lý do khi chuyển hồ sơ sang trạng thái từ chối.";
            return Redirect($"/admin/applications/{id}");
        }

        var previousStatus = application.CurrentStatus;
        var changedAt = DateTime.UtcNow;

        application.CurrentStatus = normalizedTargetStatus;
        application.UpdatedAt = changedAt;

        if (string.Equals(normalizedTargetStatus, "REJECTED", StringComparison.Ordinal))
        {
            application.RejectionReason = normalizedReason;
        }
        else
        {
            application.RejectionReason = null;
        }

        if (string.Equals(normalizedTargetStatus, "SUBMITTED", StringComparison.Ordinal) && application.SubmittedAt == null)
        {
            application.SubmittedAt = changedAt;
        }

        dbContext.ApplicationStatusHistories.Add(new ApplicationStatusHistory
        {
            Id = Guid.NewGuid(),
            ApplicationId = application.Id,
            FromStatus = previousStatus,
            ToStatus = normalizedTargetStatus,
            ChangedBy = ResolveAuthenticatedUserId(),
            ChangedAt = changedAt,
            Reason = normalizedReason,
            PublicNote = normalizedPublicNote,
            InternalNote = normalizedInternalNote
        });

        await dbContext.SaveChangesAsync();

        TempData["SuccessMessage"] = "Đã cập nhật trạng thái hồ sơ.";
        return Redirect($"/admin/applications/{id}");
    }

    private IQueryable<AdmissionApplication> CreateApplicationsQuery()
    {
        return dbContext.AdmissionApplications
            .AsNoTracking()
            .Include(application => application.Candidate)
            .Include(application => application.RoundProgram)
                .ThenInclude(roundProgram => roundProgram.Round)
            .Include(application => application.RoundProgram)
                .ThenInclude(roundProgram => roundProgram.Program)
            .Include(application => application.RoundProgram)
                .ThenInclude(roundProgram => roundProgram.Major);
    }

    private static IQueryable<AdmissionApplication> ApplySearch(IQueryable<AdmissionApplication> query, string searchTerm)
    {
        return query.Where(application =>
            application.ApplicationCode.Contains(searchTerm) ||
            application.Candidate.FullName.Contains(searchTerm) ||
            (application.Candidate.Email != null && application.Candidate.Email.Contains(searchTerm)) ||
            application.Candidate.PhoneNumber.Contains(searchTerm) ||
            application.RoundProgram.Program.ProgramName.Contains(searchTerm) ||
            (application.RoundProgram.Major != null && application.RoundProgram.Major.MajorName.Contains(searchTerm)));
    }

    private Guid? ResolveAuthenticatedUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(userId, out var parsedUserId) ? parsedUserId : null;
    }

    private static string? NormalizeStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return null;
        }

        var normalizedStatus = status.Trim().ToUpperInvariant();
        return ApplicationStatuses.Contains(normalizedStatus, StringComparer.Ordinal) ? normalizedStatus : null;
    }

    private static IReadOnlyList<string> GetAvailableTargetStatuses(string currentStatus)
    {
        var normalizedCurrentStatus = NormalizeStatus(currentStatus) ?? string.Empty;

        return normalizedCurrentStatus switch
        {
            "DRAFT" => ["SUBMITTED", "CANCELLED"],
            "SUBMITTED" => ["UNDER_REVIEW", "APPROVED", "REJECTED"],
            "UNDER_REVIEW" => ["SUBMITTED", "APPROVED", "REJECTED"],
            "APPROVED" => ["UNDER_REVIEW", "REJECTED"],
            "REJECTED" => ["UNDER_REVIEW", "APPROVED"],
            _ => []
        };
    }

    private static bool CanTransition(string currentStatus, string nextStatus)
    {
        var normalizedCurrentStatus = NormalizeStatus(currentStatus);
        if (normalizedCurrentStatus == null || !ManageableStatuses.Contains(nextStatus, StringComparer.Ordinal))
        {
            return false;
        }

        return GetAvailableTargetStatuses(normalizedCurrentStatus).Contains(nextStatus, StringComparer.Ordinal);
    }

    private static int GetStatusCount(IReadOnlyDictionary<string, int> groupedCounts, string status)
    {
        return groupedCounts.TryGetValue(status, out var count) ? count : 0;
    }

    private static string? FormatAddress(Candidate candidate)
    {
        var parts = new[]
        {
            candidate.AddressLine,
            candidate.Ward,
            candidate.District,
            candidate.ProvinceCode
        };

        var availableParts = parts
            .Where(part => !string.IsNullOrWhiteSpace(part))
            .Select(part => part!.Trim())
            .ToArray();

        return availableParts.Length == 0 ? null : string.Join(", ", availableParts);
    }
}
