using System.Linq.Expressions;

namespace wnc.Features.Common.Queries;

public interface IQueryDefinition<TEntity, TItem, in TQuery>
    where TEntity : class
    where TQuery : ListQueryParameters
{
    string DefaultSortBy { get; }
    bool DefaultSortDescending { get; }
    IReadOnlyDictionary<string, LambdaExpression> SortExpressions { get; }
    Expression<Func<TEntity, TItem>> Selector { get; }

    IQueryable<TEntity> ApplyIncludes(IQueryable<TEntity> query);
    IQueryable<TEntity> ApplyFilters(IQueryable<TEntity> query, TQuery request);
    IQueryable<TEntity> ApplySearch(IQueryable<TEntity> query, TQuery request);
}
