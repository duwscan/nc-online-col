using System.ComponentModel.DataAnnotations;

namespace wnc.Models;

public class AdmissionApplicationViewModel
{
    public Guid Id { get; set; }
    public string ApplicationCode { get; set; } = string.Empty;
    public Guid CandidateId { get; set; }
    public string CandidateName { get; set; } = string.Empty;
    public string CandidateEmail { get; set; } = string.Empty;
    public string CandidatePhone { get; set; } = string.Empty;
    public string? NationalId { get; set; }
    public Guid RoundProgramId { get; set; }
    public string ProgramName { get; set; } = string.Empty;
    public string RoundName { get; set; } = string.Empty;
    public int AdmissionYear { get; set; }
    public string CurrentStatus { get; set; } = "DRAFT";
    public string StatusText { get; set; } = string.Empty;
    public int SubmissionNumber { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime? LastResubmittedAt { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int DocumentCount { get; set; }
    public int PreferenceCount { get; set; }
}

public class CreateAdmissionApplicationViewModel
{
    [Required(ErrorMessage = "Candidate is required")]
    public Guid CandidateId { get; set; }

    [Required(ErrorMessage = "Round Program is required")]
    public Guid RoundProgramId { get; set; }
}

public class UpdateStatusViewModel
{
    public Guid ApplicationId { get; set; }

    [Required(ErrorMessage = "Status is required")]
    [MaxLength(30)]
    public string Status { get; set; } = string.Empty;

    public string? Reason { get; set; }
}

public class AdmissionApplicationSearchViewModel
{
    public string? SearchTerm { get; set; }
    public string? Status { get; set; }
    public Guid? RoundProgramId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class AdmissionApplicationPagedResult
{
    public List<AdmissionApplicationViewModel> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}

public class StatusHistoryViewModel
{
    public Guid Id { get; set; }
    public string OldStatus { get; set; } = string.Empty;
    public string NewStatus { get; set; } = string.Empty;
    public string? ChangedByUserName { get; set; }
    public DateTime ChangedAt { get; set; }
    public string? Reason { get; set; }
}
