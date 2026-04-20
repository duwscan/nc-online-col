namespace wnc.Features.Admin.Applications.ViewModels;

public class AdminApplicationsIndexViewModel
{
    public List<AdminApplicationListItemViewModel> Applications { get; set; } = [];

    public IReadOnlyList<string> AvailableStatuses { get; set; } = ["DRAFT", "SUBMITTED", "UNDER_REVIEW", "APPROVED", "REJECTED", "CANCELLED"];

    public IReadOnlyList<AdminApplicationRoundOptionViewModel> AvailableRounds { get; set; } = [];

    public string? SearchTerm { get; set; }

    public string? Status { get; set; }

    public Guid? RoundId { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 10;

    public int TotalItems { get; set; }

    public string? SuccessMessage { get; set; }

    public string? ErrorMessage { get; set; }

    public AdminApplicationStatusSummaryViewModel StatusSummary { get; set; } = new();

    public int TotalPages => TotalItems == 0 ? 1 : (int)Math.Ceiling(TotalItems / (double)PageSize);

    public int StartItem => TotalItems == 0 ? 0 : ((Page - 1) * PageSize) + 1;

    public int EndItem => TotalItems == 0 ? 0 : Math.Min(Page * PageSize, TotalItems);
}

public class AdminApplicationListItemViewModel
{
    public Guid Id { get; set; }

    public string ApplicationCode { get; set; } = string.Empty;

    public string CandidateName { get; set; } = string.Empty;

    public string CandidateEmail { get; set; } = string.Empty;

    public string CandidatePhoneNumber { get; set; } = string.Empty;

    public string RoundName { get; set; } = string.Empty;

    public string ProgramName { get; set; } = string.Empty;

    public string? MajorName { get; set; }

    public string CurrentStatus { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime? SubmittedAt { get; set; }
}

public class AdminApplicationRoundOptionViewModel
{
    public Guid Id { get; set; }

    public string RoundName { get; set; } = string.Empty;
}

public class AdminApplicationStatusSummaryViewModel
{
    public int Total { get; set; }

    public int Draft { get; set; }

    public int Submitted { get; set; }

    public int UnderReview { get; set; }

    public int Approved { get; set; }

    public int Rejected { get; set; }

    public int Cancelled { get; set; }
}
