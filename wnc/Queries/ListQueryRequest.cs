namespace wnc.Queries;

public sealed class ListQueryRequest
{
    public const int DefaultPage = 1;
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;

    public string? Keyword { get; init; }
    public string? SortBy { get; init; }
    public string? SortDirection { get; init; }
    public int Page { get; init; } = DefaultPage;
    public int PageSize { get; init; } = DefaultPageSize;

    public NormalizedListQueryRequest Normalize()
    {
        var page = Page < 1 ? DefaultPage : Page;
        var requestedPageSize = PageSize <= 0 ? DefaultPageSize : PageSize;
        var pageSize = Math.Clamp(requestedPageSize, 1, MaxPageSize);
        var sortDirection = ParseSortDirection(SortDirection);

        return new NormalizedListQueryRequest(
            Keyword?.Trim(),
            string.IsNullOrWhiteSpace(SortBy) ? null : SortBy.Trim(),
            sortDirection,
            page,
            pageSize);
    }

    private static QuerySortDirection ParseSortDirection(string? value)
    {
        return string.Equals(value, "desc", StringComparison.OrdinalIgnoreCase)
            ? QuerySortDirection.Descending
            : QuerySortDirection.Ascending;
    }
}

public sealed record NormalizedListQueryRequest(
    string? Keyword,
    string? SortBy,
    QuerySortDirection SortDirection,
    int Page,
    int PageSize);

public enum QuerySortDirection
{
    Ascending,
    Descending
}
