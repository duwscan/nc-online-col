using System.ComponentModel.DataAnnotations;

namespace wnc.Features.Admin.Applications.ViewModels;

public class AdminApplicationDetailViewModel
{
    public Guid ApplicationId { get; set; }

    public string ApplicationCode { get; set; } = string.Empty;

    public string CurrentStatus { get; set; } = string.Empty;

    public string CandidateName { get; set; } = string.Empty;

    public string CandidateEmail { get; set; } = string.Empty;

    public string CandidatePhoneNumber { get; set; } = string.Empty;

    public string? CandidateAddress { get; set; }

    public string RoundName { get; set; } = string.Empty;

    public string RoundCode { get; set; } = string.Empty;

    public string ProgramName { get; set; } = string.Empty;

    public string? MajorName { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? SubmittedAt { get; set; }

    public string? RejectionReason { get; set; }

    public List<AdminApplicationDocumentViewModel> Documents { get; set; } = [];

    public List<AdminApplicationTimelineEntryViewModel> Timeline { get; set; } = [];

    public AdminApplicationStatusUpdateInputModel StatusUpdate { get; set; } = new();

    public string? SuccessMessage { get; set; }

    public string? ErrorMessage { get; set; }
}

public class AdminApplicationDocumentViewModel
{
    public Guid DocumentId { get; set; }

    public string DocumentTypeName { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;

    public string StoragePath { get; set; } = string.Empty;

    public long FileSize { get; set; }

    public string ValidationStatus { get; set; } = string.Empty;

    public DateTime UploadedAt { get; set; }
}

public class AdminApplicationTimelineEntryViewModel
{
    public string? FromStatus { get; set; }

    public string ToStatus { get; set; } = string.Empty;

    public DateTime ChangedAt { get; set; }

    public string? ChangedByName { get; set; }

    public string? Reason { get; set; }

    public string? PublicNote { get; set; }

    public string? InternalNote { get; set; }
}

public class AdminApplicationStatusUpdateInputModel
{
    public Guid ApplicationId { get; set; }

    public string CurrentStatus { get; set; } = string.Empty;

    [Required]
    public string NewStatus { get; set; } = string.Empty;

    public IReadOnlyList<string> AvailableStatuses { get; set; } = [];

    [MaxLength(1000)]
    public string? Reason { get; set; }

    [MaxLength(2000)]
    public string? PublicNote { get; set; }

    [MaxLength(2000)]
    public string? InternalNote { get; set; }
}
