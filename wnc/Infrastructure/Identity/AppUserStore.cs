using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using wnc.Data;
using wnc.Models;

namespace wnc.Infrastructure.Identity;

public class AppUserStore(AppDbContext dbContext) :
    IUserStore<AppUser>,
    IUserPasswordStore<AppUser>,
    IUserEmailStore<AppUser>,
    IUserPhoneNumberStore<AppUser>,
    IUserRoleStore<AppUser>
{
    public void Dispose()
    {
    }

    public async Task<IdentityResult> CreateAsync(AppUser user, CancellationToken cancellationToken)
    {
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(cancellationToken);
        return IdentityResult.Success;
    }

    public async Task<IdentityResult> UpdateAsync(AppUser user, CancellationToken cancellationToken)
    {
        user.UpdatedAt = DateTime.UtcNow;
        dbContext.Users.Update(user);
        await dbContext.SaveChangesAsync(cancellationToken);
        return IdentityResult.Success;
    }

    public async Task<IdentityResult> DeleteAsync(AppUser user, CancellationToken cancellationToken)
    {
        user.DeletedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;
        dbContext.Users.Update(user);
        await dbContext.SaveChangesAsync(cancellationToken);
        return IdentityResult.Success;
    }

    public Task<string> GetUserIdAsync(AppUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.Id.ToString());
    }

    public Task<string?> GetUserNameAsync(AppUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult<string?>(user.Username);
    }

    public Task SetUserNameAsync(AppUser user, string? userName, CancellationToken cancellationToken)
    {
        user.Username = userName;
        return Task.CompletedTask;
    }

    public Task<string?> GetNormalizedUserNameAsync(AppUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.Username?.ToUpperInvariant());
    }

    public Task SetNormalizedUserNameAsync(AppUser user, string? normalizedName, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task<AppUser?> FindByIdAsync(string userId, CancellationToken cancellationToken)
    {
        return Guid.TryParse(userId, out var parsedId)
            ? dbContext.Users.SingleOrDefaultAsync(x => x.Id == parsedId && x.DeletedAt == null, cancellationToken)
            : Task.FromResult<AppUser?>(null);
    }

    public Task<AppUser?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
    {
        return dbContext.Users.SingleOrDefaultAsync(
            x => x.DeletedAt == null && x.Username != null && x.Username.ToUpper() == normalizedUserName.ToUpper(),
            cancellationToken);
    }

    public Task SetPasswordHashAsync(AppUser user, string? passwordHash, CancellationToken cancellationToken)
    {
        user.PasswordHash = passwordHash ?? string.Empty;
        return Task.CompletedTask;
    }

    public Task<string?> GetPasswordHashAsync(AppUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult<string?>(user.PasswordHash);
    }

    public Task<bool> HasPasswordAsync(AppUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(!string.IsNullOrWhiteSpace(user.PasswordHash));
    }

    public Task SetEmailAsync(AppUser user, string? email, CancellationToken cancellationToken)
    {
        user.Email = email;
        return Task.CompletedTask;
    }

    public Task<string?> GetEmailAsync(AppUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.Email);
    }

    public Task<bool> GetEmailConfirmedAsync(AppUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.EmailVerifiedAt.HasValue);
    }

    public Task SetEmailConfirmedAsync(AppUser user, bool confirmed, CancellationToken cancellationToken)
    {
        user.EmailVerifiedAt = confirmed ? DateTime.UtcNow : null;
        return Task.CompletedTask;
    }

    public Task<AppUser?> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
    {
        return dbContext.Users.SingleOrDefaultAsync(
            x => x.DeletedAt == null && x.Email != null && x.Email.ToUpper() == normalizedEmail.ToUpper(),
            cancellationToken);
    }

    public Task<string?> GetNormalizedEmailAsync(AppUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.Email?.ToUpperInvariant());
    }

    public Task SetNormalizedEmailAsync(AppUser user, string? normalizedEmail, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task SetPhoneNumberAsync(AppUser user, string? phoneNumber, CancellationToken cancellationToken)
    {
        user.PhoneNumber = phoneNumber;
        return Task.CompletedTask;
    }

    public Task<string?> GetPhoneNumberAsync(AppUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.PhoneNumber);
    }

    public Task<bool> GetPhoneNumberConfirmedAsync(AppUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.PhoneVerifiedAt.HasValue);
    }

    public Task SetPhoneNumberConfirmedAsync(AppUser user, bool confirmed, CancellationToken cancellationToken)
    {
        user.PhoneVerifiedAt = confirmed ? DateTime.UtcNow : null;
        return Task.CompletedTask;
    }

    public async Task AddToRoleAsync(AppUser user, string roleName, CancellationToken cancellationToken)
    {
        var role = await dbContext.Roles.SingleAsync(
            x => x.Code.ToUpper() == roleName.ToUpper(),
            cancellationToken);

        var existing = await dbContext.UserRoles.SingleOrDefaultAsync(
            x => x.UserId == user.Id && x.RoleId == role.Id && x.RevokedAt == null,
            cancellationToken);

        if (existing is not null)
        {
            return;
        }

        dbContext.UserRoles.Add(new UserRole
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            RoleId = role.Id,
            AssignedAt = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveFromRoleAsync(AppUser user, string roleName, CancellationToken cancellationToken)
    {
        var role = await dbContext.Roles.SingleAsync(
            x => x.Code.ToUpper() == roleName.ToUpper(),
            cancellationToken);

        var activeUserRoles = await dbContext.UserRoles
            .Where(x => x.UserId == user.Id && x.RoleId == role.Id && x.RevokedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var userRole in activeUserRoles)
        {
            userRole.RevokedAt = DateTime.UtcNow;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IList<string>> GetRolesAsync(AppUser user, CancellationToken cancellationToken)
    {
        return await dbContext.UserRoles
            .Where(x => x.UserId == user.Id && x.RevokedAt == null)
            .Join(dbContext.Roles, userRole => userRole.RoleId, role => role.Id, (userRole, role) => role.Code)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    public Task<bool> IsInRoleAsync(AppUser user, string roleName, CancellationToken cancellationToken)
    {
        return dbContext.UserRoles
            .Where(x => x.UserId == user.Id && x.RevokedAt == null)
            .Join(dbContext.Roles, userRole => userRole.RoleId, role => role.Id, (userRole, role) => role.Code)
            .AnyAsync(x => x.ToUpper() == roleName.ToUpper(), cancellationToken);
    }

    public async Task<IList<AppUser>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
    {
        return await dbContext.UserRoles
            .Where(x => x.RevokedAt == null)
            .Join(dbContext.Roles.Where(x => x.Code.ToUpper() == roleName.ToUpper()),
                userRole => userRole.RoleId,
                role => role.Id,
                (userRole, role) => userRole.UserId)
            .Join(dbContext.Users.Where(x => x.DeletedAt == null),
                userId => userId,
                user => user.Id,
                (userId, user) => user)
            .Distinct()
            .ToListAsync(cancellationToken);
    }
}
