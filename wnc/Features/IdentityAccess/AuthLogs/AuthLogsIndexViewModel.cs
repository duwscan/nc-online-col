using Microsoft.AspNetCore.Mvc.Rendering;
using wnc.Infrastructure.Queries;

namespace wnc.Features.IdentityAccess.AuthLogs;

public sealed class AuthLogsIndexViewModel
{
    public required AuthLogsListQuery Query { get; init; }
    public required PagedResult<AuthLogListItemViewModel> Result { get; init; }
    public IReadOnlyList<SelectListItem> UserOptions { get; init; } = [];
    public IReadOnlyList<SelectListItem> StatusOptions { get; init; } = [];
}
