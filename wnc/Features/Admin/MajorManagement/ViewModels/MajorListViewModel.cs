namespace wnc.Features.Admin.MajorManagement.ViewModels;

public class MajorListViewModel
{
    public List<MajorListItemViewModel> Majors { get; set; } = [];

    public IReadOnlyList<MajorProgramOptionViewModel> AvailablePrograms { get; set; } = [];

    public IReadOnlyList<string> AvailableStatuses { get; set; } = ["ACTIVE", "INACTIVE"];

    public string? SearchTerm { get; set; }

    public Guid? ProgramId { get; set; }

    public string? Status { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 10;

    public int TotalItems { get; set; }

    public int TotalPages => TotalItems == 0 ? 1 : (int)Math.Ceiling(TotalItems / (double)PageSize);

    public int StartItem => TotalItems == 0 ? 0 : ((Page - 1) * PageSize) + 1;

    public int EndItem => TotalItems == 0 ? 0 : Math.Min(Page * PageSize, TotalItems);
}

public class MajorListItemViewModel
{
    public Guid Id { get; set; }

    public string MajorCode { get; set; } = string.Empty;

    public string MajorName { get; set; } = string.Empty;

    public string ProgramName { get; set; } = string.Empty;

    public int Quota { get; set; }

    public string Status { get; set; } = string.Empty;
}

public class MajorProgramOptionViewModel
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;
}
