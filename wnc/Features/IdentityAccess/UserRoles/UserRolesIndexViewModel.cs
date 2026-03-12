using Microsoft.AspNetCore.Mvc.Rendering;
using wnc.Infrastructure.Queries;

namespace wnc.Features.IdentityAccess.UserRoles;

public sealed class UserRolesIndexViewModel
{
    public required UserRolesListQuery Query { get; init; }
    public required PagedResult<UserRoleListItemViewModel> Result { get; init; }
    public IReadOnlyList<SelectListItem> UserOptions { get; init; } = [];
    public IReadOnlyList<SelectListItem> RoleOptions { get; init; } = [];
    public IReadOnlyList<SelectListItem> StateOptions { get; init; } = [];
}
