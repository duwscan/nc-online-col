namespace wnc.Features.Admin.Rounds.ViewModels;

public class RoundsListViewModel
{
    public List<RoundListItemViewModel> Rounds { get; set; } = [];

    public IReadOnlyList<string> AvailableStatuses { get; set; } = ["DRAFT", "PUBLISHED", "CLOSED"];

    public string? SearchTerm { get; set; }

    public string? Status { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 10;

    public int TotalItems { get; set; }

    public string? SuccessMessage { get; set; }

    public int TotalPages => TotalItems == 0 ? 1 : (int)Math.Ceiling(TotalItems / (double)PageSize);

    public int StartItem => TotalItems == 0 ? 0 : ((Page - 1) * PageSize) + 1;

    public int EndItem => TotalItems == 0 ? 0 : Math.Min(Page * PageSize, TotalItems);
}

public class RoundListItemViewModel
{
    public Guid Id { get; set; }

    public string RoundCode { get; set; } = string.Empty;

    public string RoundName { get; set; } = string.Empty;

    public int AdmissionYear { get; set; }

    public DateTime StartAt { get; set; }

    public DateTime EndAt { get; set; }

    public string Status { get; set; } = string.Empty;
}
