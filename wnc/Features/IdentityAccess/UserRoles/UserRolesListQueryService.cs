using Microsoft.EntityFrameworkCore;
using wnc.Data;
using wnc.Features.IdentityAccess.Common;
using wnc.Infrastructure.Queries;

namespace wnc.Features.IdentityAccess.UserRoles;

public sealed class UserRolesListQueryService(AppDbContext dbContext) : IListQueryService<UserRolesListQuery, UserRoleListItemViewModel>
{
    private static readonly SortMap<Models.UserRole> SortMap = new SortMap<Models.UserRole>()
        .Add("user", userRole => userRole.User.Username ?? userRole.User.Email ?? string.Empty)
        .Add("role", userRole => userRole.Role.Name)
        .Add("assignedBy", userRole => userRole.AssignedByUser != null
            ? userRole.AssignedByUser.Username ?? userRole.AssignedByUser.Email ?? string.Empty
            : string.Empty)
        .Add("assignedAt", userRole => userRole.AssignedAt)
        .Add("revokedAt", userRole => userRole.RevokedAt ?? DateTime.MaxValue);

    public async Task<PagedResult<UserRoleListItemViewModel>> ExecuteAsync(UserRolesListQuery query, CancellationToken cancellationToken = default)
    {
        var userRoles = dbContext.UserRoles
            .AsNoTracking()
            .Include(userRole => userRole.User)
            .Include(userRole => userRole.Role)
            .Include(userRole => userRole.AssignedByUser)
            .ApplyFilterIf(query.UserId.HasValue, source => source.Where(userRole => userRole.UserId == query.UserId))
            .ApplyFilterIf(query.RoleId.HasValue, source => source.Where(userRole => userRole.RoleId == query.RoleId))
            .ApplyFilterIf(string.Equals(query.State, "ACTIVE", StringComparison.OrdinalIgnoreCase), source => source.Where(userRole => userRole.RevokedAt == null))
            .ApplyFilterIf(string.Equals(query.State, "REVOKED", StringComparison.OrdinalIgnoreCase), source => source.Where(userRole => userRole.RevokedAt != null))
            .ApplySearch(query.SearchTerm, (source, term) =>
            {
                var pattern = $"%{term}%";
                return source.Where(userRole =>
                    EF.Functions.Like(userRole.User.Username ?? string.Empty, pattern) ||
                    EF.Functions.Like(userRole.User.Email ?? string.Empty, pattern) ||
                    EF.Functions.Like(userRole.Role.Code, pattern) ||
                    EF.Functions.Like(userRole.Role.Name, pattern) ||
                    EF.Functions.Like(userRole.AssignedByUser != null ? userRole.AssignedByUser.Username ?? string.Empty : string.Empty, pattern) ||
                    EF.Functions.Like(userRole.AssignedByUser != null ? userRole.AssignedByUser.Email ?? string.Empty : string.Empty, pattern));
            })
            .ApplySorting(query, SortMap, "assignedAt");

        var projection = userRoles.Select(userRole => new UserRoleListItemViewModel
        {
            Id = userRole.Id,
            UserId = userRole.UserId,
            UserDisplay = userRole.User.Username ?? userRole.User.Email ?? userRole.User.PhoneNumber ?? userRole.UserId.ToString(),
            RoleId = userRole.RoleId,
            RoleDisplay = $"{userRole.Role.Code} - {userRole.Role.Name}",
            AssignedByDisplay = userRole.AssignedByUser != null
                ? userRole.AssignedByUser.Username ?? userRole.AssignedByUser.Email ?? userRole.AssignedByUser.PhoneNumber
                : null,
            AssignedAt = userRole.AssignedAt,
            RevokedAt = userRole.RevokedAt
        });

        return await projection.ToPagedResultAsync(query, cancellationToken);
    }
}
