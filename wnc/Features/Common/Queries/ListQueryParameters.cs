namespace wnc.Features.Common.Queries;

public class ListQueryParameters
{
    private const int DefaultPageSize = 10;
    private const int MaxPageSize = 100;

    public string? Search { get; set; }
    public string? SortBy { get; set; }
    public bool Descending { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = DefaultPageSize;

    public int NormalizedPage => Page < 1 ? 1 : Page;

    public int NormalizedPageSize
    {
        get
        {
            if (PageSize < 1)
            {
                return DefaultPageSize;
            }

            return PageSize > MaxPageSize ? MaxPageSize : PageSize;
        }
    }

    public string? NormalizedSearch => string.IsNullOrWhiteSpace(Search) ? null : Search.Trim();
    public string? NormalizedSortBy => string.IsNullOrWhiteSpace(SortBy) ? null : SortBy.Trim().ToLowerInvariant();
}
