namespace wnc.ViewModels.Shared;

public class PagerViewModel
{
    public string Action { get; init; } = "Index";

    public int Page { get; init; }

    public int TotalPages { get; init; }

    public IDictionary<string, string?> RouteValues { get; init; } = new Dictionary<string, string?>();
}
