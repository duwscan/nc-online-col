namespace wnc.Features.Admin.StaffManagement.ViewModels;

public class StaffListViewModel
{
    public List<StaffListItemViewModel> StaffMembers { get; set; } = [];

    public List<StaffFilterOptionViewModel> AvailableRoles { get; set; } = [];

    public IReadOnlyList<string> AvailableStatuses { get; set; } = ["ACTIVE", "INACTIVE"];

    public string? SearchTerm { get; set; }

    public string? RoleCode { get; set; }

    public string? Status { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 10;

    public int TotalItems { get; set; }

    public int TotalPages => TotalItems == 0 ? 1 : (int)Math.Ceiling(TotalItems / (double)PageSize);

    public int StartItem => TotalItems == 0 ? 0 : ((Page - 1) * PageSize) + 1;

    public int EndItem => TotalItems == 0 ? 0 : Math.Min(Page * PageSize, TotalItems);
}

public class StaffListItemViewModel
{
    public Guid Id { get; set; }

    public string Username { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public List<string> RoleNames { get; set; } = [];

    public string Status { get; set; } = string.Empty;

    public DateTime? LastLoginAt { get; set; }
}

public class StaffFilterOptionViewModel
{
    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
}
