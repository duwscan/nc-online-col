using System.Linq.Expressions;

namespace wnc.Infrastructure.Queries;

public sealed class SortMap<TEntity>
{
    private readonly Dictionary<string, LambdaExpression> selectors = new(StringComparer.OrdinalIgnoreCase);

    public SortMap<TEntity> Add<TKey>(string key, Expression<Func<TEntity, TKey>> selector)
    {
        selectors[key] = selector;
        return this;
    }

    public bool TryGet(string key, out LambdaExpression selector) => selectors.TryGetValue(key, out selector!);
}
