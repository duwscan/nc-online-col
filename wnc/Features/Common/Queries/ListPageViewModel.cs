namespace wnc.Features.Common.Queries;

public class ListPageViewModel<TQuery, TItem>
    where TQuery : ListQueryParameters
{
    public required TQuery Query { get; init; }
    public required PagedResult<TItem> Results { get; init; }
}
