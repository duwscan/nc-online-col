using wnc.Infrastructure.Queries;

namespace wnc.ViewModels.Shared;

public class IndexPageViewModel<TItem>
{
    public string Title { get; init; } = string.Empty;

    public string SearchPlaceholder { get; init; } = "Tìm kiếm...";

    public QueryPipelineRequest Query { get; init; } = new();

    public PagedResult<TItem> Result { get; init; } = PagedResult<TItem>.Empty;
}
