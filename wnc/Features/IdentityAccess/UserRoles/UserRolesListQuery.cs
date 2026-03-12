using wnc.Infrastructure.Queries;

namespace wnc.Features.IdentityAccess.UserRoles;

public sealed class UserRolesListQuery : ListQueryParameters
{
    public Guid? UserId { get; set; }
    public Guid? RoleId { get; set; }
    public string? State { get; set; }
}
