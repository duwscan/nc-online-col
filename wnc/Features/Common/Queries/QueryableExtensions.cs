using System.Linq.Expressions;
using System.Reflection;

namespace wnc.Features.Common.Queries;

public static class QueryableExtensions
{
    private static readonly MethodInfo OrderByMethod = GetOrderingMethod(nameof(Queryable.OrderBy));
    private static readonly MethodInfo OrderByDescendingMethod = GetOrderingMethod(nameof(Queryable.OrderByDescending));

    public static IQueryable<TEntity> ApplySort<TEntity>(
        this IQueryable<TEntity> query,
        IReadOnlyDictionary<string, LambdaExpression> sortExpressions,
        string? sortBy,
        bool descending,
        string defaultSortBy,
        bool defaultDescending)
    {
        var effectiveSortBy = sortBy ?? defaultSortBy;
        var effectiveDescending = sortBy is null ? defaultDescending : descending;

        if (!sortExpressions.TryGetValue(effectiveSortBy, out var expression))
        {
            expression = sortExpressions[defaultSortBy];
            effectiveDescending = defaultDescending;
        }

        var method = effectiveDescending ? OrderByDescendingMethod : OrderByMethod;
        var genericMethod = method.MakeGenericMethod(typeof(TEntity), expression.ReturnType);

        return (IQueryable<TEntity>)genericMethod.Invoke(null, new object[] { query, expression })!;
    }

    public static IQueryable<TEntity> ApplyPaging<TEntity>(this IQueryable<TEntity> query, int page, int pageSize)
    {
        return query
            .Skip((page - 1) * pageSize)
            .Take(pageSize);
    }

    private static MethodInfo GetOrderingMethod(string name)
    {
        return typeof(Queryable)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(method => method.Name == name && method.GetParameters().Length == 2);
    }
}
