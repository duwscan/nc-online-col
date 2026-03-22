namespace wnc.Features.Admin.TrainingProgramManagement.ViewModels;

public class TrainingProgramListViewModel
{
    public List<TrainingProgramListItemViewModel> Programs { get; set; } = [];

    public IReadOnlyList<TrainingProgramFilterOptionViewModel> AvailableEducationTypes { get; set; } = [];

    public IReadOnlyList<string> AvailableStatuses { get; set; } = ["ACTIVE", "INACTIVE"];

    public string? SearchTerm { get; set; }

    public string? EducationType { get; set; }

    public string? Status { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 10;

    public int TotalItems { get; set; }

    public int TotalPages => TotalItems == 0 ? 1 : (int)Math.Ceiling(TotalItems / (double)PageSize);

    public int StartItem => TotalItems == 0 ? 0 : ((Page - 1) * PageSize) + 1;

    public int EndItem => TotalItems == 0 ? 0 : Math.Min(Page * PageSize, TotalItems);
}

public class TrainingProgramListItemViewModel
{
    public Guid Id { get; set; }

    public string ProgramCode { get; set; } = string.Empty;

    public string ProgramName { get; set; } = string.Empty;

    public string EducationType { get; set; } = string.Empty;

    public string EducationTypeLabel { get; set; } = string.Empty;

    public decimal? TuitionFee { get; set; }

    public int Quota { get; set; }

    public string Status { get; set; } = string.Empty;

    public int DisplayOrder { get; set; }
}

public class TrainingProgramFilterOptionViewModel
{
    public string Value { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;
}
