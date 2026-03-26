namespace wnc.Features.Home.ViewModels;

public class HomeIndexViewModel
{
    public required PersistentUserStateViewModel PersistentState { get; init; }
    public required SessionUserStateViewModel SessionState { get; init; }
}

public class PersistentUserStateViewModel
{
    public int TotalVisits { get; init; }
    public DateTime? LastVisitedAtUtc { get; init; }
    public string FavoriteFeature { get; init; } = "Chua co du lieu";
}

public class SessionUserStateViewModel
{
    public int RequestCount { get; init; }
    public string? Portal { get; init; }
    public string? DisplayName { get; init; }
    public string[] Roles { get; init; } = [];
    public DateTime? AuthenticatedAtUtc { get; init; }
    public string? LastPath { get; init; }
}
