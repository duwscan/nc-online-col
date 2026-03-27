using wnc.Infrastructure.Queries;

namespace wnc.Features.IdentityAccess.Common;

public interface IListQueryService<in TQuery, TResult> where TQuery : ListQueryParameters
{
    Task<PagedResult<TResult>> ExecuteAsync(TQuery query, CancellationToken cancellationToken = default);
}
