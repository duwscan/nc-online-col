using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using wnc.Data;
using wnc.Models;

namespace wnc.Controllers;

public class AdmissionApplicationController : Controller
{
    private readonly AppDbContext _context;

    public AdmissionApplicationController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public IActionResult GetApplications([FromQuery] AdmissionApplicationSearchViewModel search)
    {
        var query = _context.AdmissionApplications
            .Include(a => a.Candidate)
            .Include(a => a.RoundProgram)
                .ThenInclude(rp => rp.Round)
            .Include(a => a.RoundProgram)
                .ThenInclude(rp => rp.Program)
            .Include(a => a.ApplicationDocuments)
            .Include(a => a.ApplicationPreferences)
            .AsQueryable();

        if (!string.IsNullOrEmpty(search.SearchTerm))
        {
            var term = search.SearchTerm.ToLower();
            query = query.Where(a =>
                a.ApplicationCode.ToLower().Contains(term) ||
                a.Candidate.FullName.ToLower().Contains(term) ||
                a.Candidate.Email.ToLower().Contains(term) ||
                (a.Candidate.NationalId != null && a.Candidate.NationalId.Contains(term)));
        }

        if (!string.IsNullOrEmpty(search.Status))
        {
            query = query.Where(a => a.CurrentStatus == search.Status);
        }

        if (search.RoundProgramId.HasValue)
        {
            query = query.Where(a => a.RoundProgramId == search.RoundProgramId.Value);
        }

        var totalCount = query.Count();

        var applications = query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((search.Page - 1) * search.PageSize)
            .Take(search.PageSize)
            .Select(a => new AdmissionApplicationViewModel
            {
                Id = a.Id,
                ApplicationCode = a.ApplicationCode,
                CandidateId = a.CandidateId,
                CandidateName = a.Candidate.FullName,
                CandidateEmail = a.Candidate.Email,
                CandidatePhone = a.Candidate.PhoneNumber,
                NationalId = a.Candidate.NationalId,
                RoundProgramId = a.RoundProgramId,
                ProgramName = a.RoundProgram.Program.ProgramName,
                RoundName = a.RoundProgram.Round.RoundName,
                AdmissionYear = a.RoundProgram.Round.AdmissionYear,
                CurrentStatus = a.CurrentStatus,
                StatusText = GetStatusText(a.CurrentStatus),
                SubmissionNumber = a.SubmissionNumber,
                SubmittedAt = a.SubmittedAt,
                LastResubmittedAt = a.LastResubmittedAt,
                RejectionReason = a.RejectionReason,
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt,
                DocumentCount = a.ApplicationDocuments.Count(),
                PreferenceCount = a.ApplicationPreferences.Count()
            })
            .ToList();

        var result = new AdmissionApplicationPagedResult
        {
            Items = applications,
            TotalCount = totalCount,
            PageNumber = search.Page,
            PageSize = search.PageSize
        };

        return Json(result);
    }

    [HttpGet]
    public IActionResult GetById(Guid id)
    {
        var application = _context.AdmissionApplications
            .Include(a => a.Candidate)
            .Include(a => a.RoundProgram)
                .ThenInclude(rp => rp.Round)
            .Include(a => a.RoundProgram)
                .ThenInclude(rp => rp.Program)
            .Include(a => a.ApplicationDocuments)
            .Include(a => a.ApplicationPreferences)
            .Include(a => a.ApplicationStatusHistories)
                .ThenInclude(h => h.ChangedByUser)
            .FirstOrDefault(a => a.Id == id);

        if (application == null)
        {
            return Json(new { success = false, message = "Application not found" });
        }

        var vm = new AdmissionApplicationViewModel
        {
            Id = application.Id,
            ApplicationCode = application.ApplicationCode,
            CandidateId = application.CandidateId,
            CandidateName = application.Candidate.FullName,
            CandidateEmail = application.Candidate.Email,
            CandidatePhone = application.Candidate.PhoneNumber,
            NationalId = application.Candidate.NationalId,
            RoundProgramId = application.RoundProgramId,
            ProgramName = application.RoundProgram.Program.ProgramName,
            RoundName = application.RoundProgram.Round.RoundName,
            AdmissionYear = application.RoundProgram.Round.AdmissionYear,
            CurrentStatus = application.CurrentStatus,
            StatusText = GetStatusText(application.CurrentStatus),
            SubmissionNumber = application.SubmissionNumber,
            SubmittedAt = application.SubmittedAt,
            LastResubmittedAt = application.LastResubmittedAt,
            RejectionReason = application.RejectionReason,
            CreatedAt = application.CreatedAt,
            UpdatedAt = application.UpdatedAt,
            DocumentCount = application.ApplicationDocuments.Count(),
            PreferenceCount = application.ApplicationPreferences.Count()
        };

        var histories = application.ApplicationStatusHistories
            .OrderByDescending(h => h.ChangedAt)
            .Select(h => new StatusHistoryViewModel
            {
                Id = h.Id,
                OldStatus = h.FromStatus ?? "",
                NewStatus = h.ToStatus,
                ChangedByUserName = h.ChangedByUser != null ? h.ChangedByUser.Username : null,
                ChangedAt = h.ChangedAt,
                Reason = h.Reason
            })
            .ToList();

        return Json(new { success = true, data = vm, histories });
    }

    [HttpPost]
    public IActionResult UpdateStatus([FromBody] UpdateStatusViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return Json(new { success = false, message = "Invalid data" });
        }

        var application = _context.AdmissionApplications
            .Include(a => a.ApplicationStatusHistories)
            .FirstOrDefault(a => a.Id == model.ApplicationId);

        if (application == null)
        {
            return Json(new { success = false, message = "Application not found" });
        }

        var oldStatus = application.CurrentStatus;
        application.CurrentStatus = model.Status;

        if (model.Status == "SUBMITTED" && application.SubmittedAt == null)
        {
            application.SubmittedAt = DateTime.UtcNow;
        }

        if (model.Status == "RESUBMITTED" || model.Status == "SUBMITTED")
        {
            application.LastResubmittedAt = DateTime.UtcNow;
        }

        if (model.Status == "REJECTED" && !string.IsNullOrEmpty(model.Reason))
        {
            application.RejectionReason = model.Reason;
        }

        application.UpdatedAt = DateTime.UtcNow;

        var history = new ApplicationStatusHistory
        {
            Id = Guid.NewGuid(),
            ApplicationId = application.Id,
            FromStatus = oldStatus,
            ToStatus = model.Status,
            Reason = model.Reason,
            ChangedAt = DateTime.UtcNow
        };

        _context.ApplicationStatusHistories.Add(history);
        _context.SaveChanges();

        return Json(new { success = true, message = "Status updated successfully" });
    }

    [HttpGet]
    public IActionResult GetStatuses()
    {
        var statuses = new[]
        {
            new { Value = "DRAFT", Text = "Nháp", Color = "secondary" },
            new { Value = "SUBMITTED", Text = "Đã nộp", Color = "info" },
            new { Value = "UNDER_REVIEW", Text = "Đang xét duyệt", Color = "primary" },
            new { Value = "REQUIRED_SUPPLEMENT", Text = "Yêu cầu bổ sung", Color = "warning" },
            new { Value = "RESUBMITTED", Text = "Đã bổ sung", Color = "info" },
            new { Value = "APPROVED", Text = "Đã duyệt", Color = "success" },
            new { Value = "REJECTED", Text = "Bị từ chối", Color = "danger" },
            new { Value = "CANCELLED", Text = "Đã hủy", Color = "dark" }
        };

        return Json(statuses);
    }

    [HttpGet]
    public IActionResult GetRoundPrograms()
    {
        var roundPrograms = _context.RoundPrograms
            .Include(rp => rp.Round)
            .Include(rp => rp.Program)
            .Where(rp => rp.Round.Status == "PUBLISHED" && rp.Status == "ACTIVE")
            .OrderBy(rp => rp.Round.AdmissionYear)
            .ThenBy(rp => rp.Round.RoundName)
            .Select(rp => new
            {
                rp.Id,
                RoundName = rp.Round.RoundName,
                ProgramName = rp.Program.ProgramName,
                Year = rp.Round.AdmissionYear
            })
            .ToList();

        return Json(roundPrograms);
    }

    [HttpGet]
    public IActionResult GetCandidates()
    {
        var candidates = _context.Candidates
            .OrderBy(c => c.FullName)
            .Select(c => new { c.Id, c.FullName, c.Email, c.PhoneNumber })
            .ToList();

        return Json(candidates);
    }

    private static string GetStatusText(string status)
    {
        return status switch
        {
            "DRAFT" => "Nháp",
            "SUBMITTED" => "Đã nộp",
            "UNDER_REVIEW" => "Đang xét duyệt",
            "REQUIRED_SUPPLEMENT" => "Yêu cầu bổ sung",
            "RESUBMITTED" => "Đã bổ sung",
            "APPROVED" => "Đã duyệt",
            "REJECTED" => "Bị từ chối",
            "CANCELLED" => "Đã hủy",
            _ => status
        };
    }
}
