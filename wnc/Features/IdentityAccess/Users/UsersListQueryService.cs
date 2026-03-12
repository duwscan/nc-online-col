using Microsoft.EntityFrameworkCore;
using wnc.Data;
using wnc.Features.IdentityAccess.Common;
using wnc.Infrastructure.Queries;

namespace wnc.Features.IdentityAccess.Users;

public sealed class UsersListQueryService(AppDbContext dbContext) : IListQueryService<UsersListQuery, UserListItemViewModel>
{
    private static readonly SortMap<Models.AppUser> SortMap = new SortMap<Models.AppUser>()
        .Add("username", user => user.Username ?? string.Empty)
        .Add("email", user => user.Email ?? string.Empty)
        .Add("phoneNumber", user => user.PhoneNumber ?? string.Empty)
        .Add("status", user => user.Status)
        .Add("createdAt", user => user.CreatedAt)
        .Add("lastLoginAt", user => user.LastLoginAt ?? DateTime.MinValue)
        .Add("activeRoles", user => user.UserRoles.Count(role => role.RevokedAt == null));

    public async Task<PagedResult<UserListItemViewModel>> ExecuteAsync(UsersListQuery query, CancellationToken cancellationToken = default)
    {
        var users = dbContext.Users
            .AsNoTracking()
            .ApplyFilterIf(!query.IncludeDeleted, source => source.Where(user => user.DeletedAt == null))
            .ApplyFilterIf(!string.IsNullOrWhiteSpace(query.Status), source => source.Where(user => user.Status == query.Status))
            .ApplySearch(query.SearchTerm, (source, term) =>
            {
                var pattern = $"%{term}%";
                return source.Where(user =>
                    EF.Functions.Like(user.Username ?? string.Empty, pattern) ||
                    EF.Functions.Like(user.Email ?? string.Empty, pattern) ||
                    EF.Functions.Like(user.PhoneNumber ?? string.Empty, pattern));
            })
            .ApplySorting(query, SortMap, "createdAt");

        var projection = users.Select(user => new UserListItemViewModel
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Status = user.Status,
            ActiveRoleCount = user.UserRoles.Count(role => role.RevokedAt == null),
            LastLoginAt = user.LastLoginAt,
            CreatedAt = user.CreatedAt,
            DeletedAt = user.DeletedAt
        });

        return await projection.ToPagedResultAsync(query, cancellationToken);
    }
}
