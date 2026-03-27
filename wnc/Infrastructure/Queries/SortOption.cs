namespace wnc.Infrastructure.Queries;

public sealed class SortOption<TEntity>(
    Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy,
    Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderByDescending)
{
    public IQueryable<TEntity> Apply(IQueryable<TEntity> query, bool descending)
        => descending ? orderByDescending(query) : orderBy(query);
}
