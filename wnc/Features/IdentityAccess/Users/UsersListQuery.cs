using wnc.Infrastructure.Queries;

namespace wnc.Features.IdentityAccess.Users;

public sealed class UsersListQuery : ListQueryParameters
{
    public string? Status { get; set; }
    public bool IncludeDeleted { get; set; }
}
