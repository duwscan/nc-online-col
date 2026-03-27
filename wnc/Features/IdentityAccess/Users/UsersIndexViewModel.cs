using Microsoft.AspNetCore.Mvc.Rendering;
using wnc.Infrastructure.Queries;

namespace wnc.Features.IdentityAccess.Users;

public sealed class UsersIndexViewModel
{
    public required UsersListQuery Query { get; init; }
    public required PagedResult<UserListItemViewModel> Result { get; init; }
    public IReadOnlyList<SelectListItem> StatusOptions { get; init; } = [];
}
