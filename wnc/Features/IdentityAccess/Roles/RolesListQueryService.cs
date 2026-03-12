using Microsoft.EntityFrameworkCore;
using wnc.Data;
using wnc.Features.IdentityAccess.Common;
using wnc.Infrastructure.Queries;

namespace wnc.Features.IdentityAccess.Roles;

public sealed class RolesListQueryService(AppDbContext dbContext) : IListQueryService<RolesListQuery, RoleListItemViewModel>
{
    private static readonly SortMap<Models.Role> SortMap = new SortMap<Models.Role>()
        .Add("code", role => role.Code)
        .Add("name", role => role.Name)
        .Add("isSystemRole", role => role.IsSystemRole)
        .Add("updatedAt", role => role.UpdatedAt)
        .Add("assignments", role => role.UserRoles.Count(userRole => userRole.RevokedAt == null));

    public async Task<PagedResult<RoleListItemViewModel>> ExecuteAsync(RolesListQuery query, CancellationToken cancellationToken = default)
    {
        var roles = dbContext.Roles
            .AsNoTracking()
            .ApplyFilterIf(query.IsSystemRole.HasValue, source => source.Where(role => role.IsSystemRole == query.IsSystemRole!.Value))
            .ApplySearch(query.SearchTerm, (source, term) =>
            {
                var pattern = $"%{term}%";
                return source.Where(role =>
                    EF.Functions.Like(role.Code, pattern) ||
                    EF.Functions.Like(role.Name, pattern) ||
                    EF.Functions.Like(role.Description ?? string.Empty, pattern));
            })
            .ApplySorting(query, SortMap, "name");

        var projection = roles.Select(role => new RoleListItemViewModel
        {
            Id = role.Id,
            Code = role.Code,
            Name = role.Name,
            Description = role.Description,
            IsSystemRole = role.IsSystemRole,
            ActiveAssignmentCount = role.UserRoles.Count(userRole => userRole.RevokedAt == null),
            UpdatedAt = role.UpdatedAt
        });

        return await projection.ToPagedResultAsync(query, cancellationToken);
    }
}
