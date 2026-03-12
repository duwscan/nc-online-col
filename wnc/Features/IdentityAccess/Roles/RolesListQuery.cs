using wnc.Infrastructure.Queries;

namespace wnc.Features.IdentityAccess.Roles;

public sealed class RolesListQuery : ListQueryParameters
{
    public bool? IsSystemRole { get; set; }
}
