using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using wnc.Data;
using wnc.Models;

namespace wnc.Infrastructure.Identity;

public class AppRoleStore(AppDbContext dbContext) : IRoleStore<Role>
{
    public void Dispose()
    {
    }

    public async Task<IdentityResult> CreateAsync(Role role, CancellationToken cancellationToken)
    {
        dbContext.Roles.Add(role);
        await dbContext.SaveChangesAsync(cancellationToken);
        return IdentityResult.Success;
    }

    public async Task<IdentityResult> UpdateAsync(Role role, CancellationToken cancellationToken)
    {
        role.UpdatedAt = DateTime.UtcNow;
        dbContext.Roles.Update(role);
        await dbContext.SaveChangesAsync(cancellationToken);
        return IdentityResult.Success;
    }

    public async Task<IdentityResult> DeleteAsync(Role role, CancellationToken cancellationToken)
    {
        dbContext.Roles.Remove(role);
        await dbContext.SaveChangesAsync(cancellationToken);
        return IdentityResult.Success;
    }

    public Task<string> GetRoleIdAsync(Role role, CancellationToken cancellationToken)
    {
        return Task.FromResult(role.Id.ToString());
    }

    public Task<string?> GetRoleNameAsync(Role role, CancellationToken cancellationToken)
    {
        return Task.FromResult<string?>(role.Code);
    }

    public Task SetRoleNameAsync(Role role, string? roleName, CancellationToken cancellationToken)
    {
        role.Code = roleName ?? string.Empty;
        return Task.CompletedTask;
    }

    public Task<string?> GetNormalizedRoleNameAsync(Role role, CancellationToken cancellationToken)
    {
        return Task.FromResult<string?>(role.Code.ToUpperInvariant());
    }

    public Task SetNormalizedRoleNameAsync(Role role, string? normalizedName, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task<Role?> FindByIdAsync(string roleId, CancellationToken cancellationToken)
    {
        return Guid.TryParse(roleId, out var parsedId)
            ? dbContext.Roles.SingleOrDefaultAsync(x => x.Id == parsedId, cancellationToken)
            : Task.FromResult<Role?>(null);
    }

    public Task<Role?> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
    {
        return dbContext.Roles.SingleOrDefaultAsync(
            x => x.Code.ToUpper() == normalizedRoleName.ToUpper(),
            cancellationToken);
    }
}
