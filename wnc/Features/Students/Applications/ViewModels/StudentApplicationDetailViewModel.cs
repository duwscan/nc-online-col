using System;

namespace wnc.Features.Students.Applications.ViewModels;

public class StudentApplicationDetailViewModel
{
    public Guid ApplicationId { get; set; }
    public string PageTitle { get; set; } = "Chi tiết đơn đăng ký";

    // Summary fields
    public string ApplicationCode { get; set; } = string.Empty;
    public string CurrentStatus { get; set; } = string.Empty;
    public string RoundName { get; set; } = string.Empty;
    public string ProgramName { get; set; } = string.Empty;
    public string? MajorName { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsDraft { get; set; }
    public string? RejectionReason { get; set; }

    // Banner flag
    public bool ShowDuplicateBanner { get; set; }

    // Continue-submit link for drafts
    public string? ContinueSubmitLink { get; set; }

    // Documents
    public List<DocumentSummaryViewModel> Documents { get; set; } = [];

    // Timeline
    public List<TimelineEntryViewModel> Timeline { get; set; } = [];
}

public class DocumentSummaryViewModel
{
    public Guid DocumentId { get; set; }
    public string DocumentTypeName { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string StoragePath { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string UploadedAt { get; set; } = string.Empty;
    public string ValidationStatus { get; set; } = "PENDING";
}

public class TimelineEntryViewModel
{
    public string? FromStatus { get; set; }
    public string ToStatus { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; }
    public string? Reason { get; set; }
    public string? PublicNote { get; set; }
}
