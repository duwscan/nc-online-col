using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace wnc.Infrastructure.Queries;

public static class QueryPipelineExtensions
{
    public static IQueryable<TEntity> ApplyFilterIf<TEntity>(
        this IQueryable<TEntity> query,
        bool condition,
        Func<IQueryable<TEntity>, IQueryable<TEntity>> filter)
        => condition ? filter(query) : query;

    public static IQueryable<TEntity> ApplySearch<TEntity>(
        this IQueryable<TEntity> query,
        string? searchTerm,
        Func<IQueryable<TEntity>, string, IQueryable<TEntity>> applySearch)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return query;
        }

        return applySearch(query, searchTerm.Trim());
    }

    public static IQueryable<TEntity> ApplySorting<TEntity>(
        this IQueryable<TEntity> query,
        ListQueryParameters request,
        SortMap<TEntity> sortMap,
        string defaultSortKey)
    {
        var sortKey = string.IsNullOrWhiteSpace(request.SortBy) ? defaultSortKey : request.SortBy;
        if (!sortMap.TryGet(sortKey, out var selector) && !sortMap.TryGet(defaultSortKey, out selector))
        {
            return query;
        }

        return query.ApplyOrdering(selector, request.IsDescending());
    }

    public static async Task<PagedResult<TProjection>> ToPagedResultAsync<TProjection>(
        this IQueryable<TProjection> query,
        ListQueryParameters request,
        CancellationToken cancellationToken = default)
    {
        var page = request.GetPage();
        var pageSize = request.GetPageSize();
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<TProjection>(items, totalCount, page, pageSize);
    }

    private static IQueryable<TEntity> ApplyOrdering<TEntity>(
        this IQueryable<TEntity> source,
        LambdaExpression keySelector,
        bool descending)
    {
        var methodName = descending ? nameof(Queryable.OrderByDescending) : nameof(Queryable.OrderBy);
        var method = typeof(Queryable)
            .GetMethods()
            .Single(x => x.Name == methodName && x.GetParameters().Length == 2);

        var genericMethod = method.MakeGenericMethod(typeof(TEntity), keySelector.ReturnType);
        return (IQueryable<TEntity>)genericMethod.Invoke(null, [source, keySelector])!;
    }
}
