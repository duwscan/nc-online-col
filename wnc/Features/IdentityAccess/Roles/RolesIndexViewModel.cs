using Microsoft.AspNetCore.Mvc.Rendering;
using wnc.Infrastructure.Queries;

namespace wnc.Features.IdentityAccess.Roles;

public sealed class RolesIndexViewModel
{
    public required RolesListQuery Query { get; init; }
    public required PagedResult<RoleListItemViewModel> Result { get; init; }
    public IReadOnlyList<SelectListItem> RoleTypeOptions { get; init; } = [];
}
