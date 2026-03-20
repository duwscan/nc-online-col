using Microsoft.EntityFrameworkCore;

namespace wnc.Features.Common.Queries;

public class QueryPipelineService
{
    public async Task<PagedResult<TItem>> ExecuteAsync<TEntity, TItem, TQuery>(
        IQueryable<TEntity> source,
        TQuery request,
        IQueryDefinition<TEntity, TItem, TQuery> definition,
        CancellationToken cancellationToken = default)
        where TEntity : class
        where TQuery : ListQueryParameters
    {
        var page = request.NormalizedPage;
        var pageSize = request.NormalizedPageSize;

        var query = definition.ApplyIncludes(source.AsNoTracking());
        query = definition.ApplyFilters(query, request);
        query = definition.ApplySearch(query, request);
        query = query.ApplySort(
            definition.SortExpressions,
            request.NormalizedSortBy,
            request.Descending,
            definition.DefaultSortBy,
            definition.DefaultSortDescending);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .ApplyPaging(page, pageSize)
            .Select(definition.Selector)
            .ToListAsync(cancellationToken);

        return new PagedResult<TItem>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }
}
