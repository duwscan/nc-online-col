using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace wnc.Infrastructure.Queries;

public sealed class QueryPipeline<TEntity>(IQueryable<TEntity> query)
{
    private IQueryable<TEntity> _query = query;

    public QueryPipeline<TEntity> Search(
        string? searchTerm,
        Func<IQueryable<TEntity>, string, IQueryable<TEntity>> applySearch)
    {
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            _query = applySearch(_query, searchTerm.Trim());
        }

        return this;
    }

    public QueryPipeline<TEntity> Filter(Func<IQueryable<TEntity>, IQueryable<TEntity>> applyFilter)
    {
        _query = applyFilter(_query);
        return this;
    }

    public QueryPipeline<TEntity> Sort(
        QueryPipelineRequest request,
        string defaultSortBy,
        IReadOnlyDictionary<string, SortOption<TEntity>> sortOptions)
    {
        var requestedSort = string.IsNullOrWhiteSpace(request.SortBy)
            ? defaultSortBy
            : request.SortBy!;

        var effectiveSort = sortOptions.TryGetValue(requestedSort, out var option)
            ? option
            : sortOptions[defaultSortBy];

        _query = effectiveSort.Apply(_query, request.Descending);
        return this;
    }

    public async Task<PagedResult<TProjection>> SelectPageAsync<TProjection>(
        QueryPipelineRequest request,
        Expression<Func<TEntity, TProjection>> selector,
        CancellationToken cancellationToken = default)
    {
        var page = request.NormalizedPage;
        var pageSize = request.NormalizedPageSize;
        var totalCount = await _query.CountAsync(cancellationToken);

        var items = await _query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(selector)
            .ToListAsync(cancellationToken);

        return new PagedResult<TProjection>(items, totalCount, page, pageSize);
    }
}
