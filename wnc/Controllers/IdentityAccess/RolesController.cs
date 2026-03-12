using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using wnc.Data;
using wnc.Features.IdentityAccess.Common;
using wnc.Features.IdentityAccess.Roles;
using wnc.Models;

namespace wnc.Controllers;

public sealed class RolesController(
    AppDbContext dbContext,
    IListQueryService<RolesListQuery, RoleListItemViewModel> listQueryService) : Controller
{
    public async Task<IActionResult> Index([FromQuery] RolesListQuery query, CancellationToken cancellationToken)
    {
        var result = await listQueryService.ExecuteAsync(query, cancellationToken);
        var model = new RolesIndexViewModel
        {
            Query = query,
            Result = result,
            RoleTypeOptions = IdentityAccessSelectLists.BooleanOptions(query.IsSystemRole, "System roles", "Custom roles")
        };

        return View(model);
    }

    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var role = await dbContext.Roles
            .AsNoTracking()
            .Where(entity => entity.Id == id)
            .Select(entity => new RoleDetailsViewModel
            {
                Id = entity.Id,
                Code = entity.Code,
                Name = entity.Name,
                Description = entity.Description,
                IsSystemRole = entity.IsSystemRole,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
                Assignments = entity.UserRoles
                    .OrderByDescending(userRole => userRole.RevokedAt == null)
                    .ThenBy(userRole => userRole.User.Username)
                    .Select(userRole =>
                        $"{(userRole.User.Username ?? userRole.User.Email ?? userRole.User.PhoneNumber ?? userRole.UserId.ToString())} " +
                        $"({(userRole.RevokedAt == null ? "Active" : $"Revoked {userRole.RevokedAt:u}")})")
                    .ToList()
            })
            .SingleOrDefaultAsync(cancellationToken);

        return role is null ? NotFound() : View(role);
    }

    public IActionResult Create() => View(new RoleFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RoleFormViewModel model, CancellationToken cancellationToken)
    {
        await ValidateRoleAsync(model, null, cancellationToken);
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var role = new Role
        {
            Id = Guid.NewGuid(),
            Code = model.Code.Trim().ToUpperInvariant(),
            Name = model.Name.Trim(),
            Description = NormalizeText(model.Description),
            IsSystemRole = model.IsSystemRole,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        dbContext.Roles.Add(role);
        await dbContext.SaveChangesAsync(cancellationToken);

        return RedirectToAction(nameof(Details), new { id = role.Id });
    }

    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var role = await dbContext.Roles.AsNoTracking().SingleOrDefaultAsync(entity => entity.Id == id, cancellationToken);
        if (role is null)
        {
            return NotFound();
        }

        var model = new RoleFormViewModel
        {
            Code = role.Code,
            Name = role.Name,
            Description = role.Description,
            IsSystemRole = role.IsSystemRole
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, RoleFormViewModel model, CancellationToken cancellationToken)
    {
        var role = await dbContext.Roles.SingleOrDefaultAsync(entity => entity.Id == id, cancellationToken);
        if (role is null)
        {
            return NotFound();
        }

        await ValidateRoleAsync(model, id, cancellationToken);
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        role.Code = model.Code.Trim().ToUpperInvariant();
        role.Name = model.Name.Trim();
        role.Description = NormalizeText(model.Description);
        role.IsSystemRole = model.IsSystemRole;
        role.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return RedirectToAction(nameof(Details), new { id });
    }

    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var role = await dbContext.Roles
            .AsNoTracking()
            .Where(entity => entity.Id == id)
            .Select(entity => new RoleDetailsViewModel
            {
                Id = entity.Id,
                Code = entity.Code,
                Name = entity.Name,
                Description = entity.Description,
                IsSystemRole = entity.IsSystemRole,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
                Assignments = entity.UserRoles
                    .Select(userRole => userRole.User.Username ?? userRole.User.Email ?? userRole.User.PhoneNumber ?? userRole.UserId.ToString())
                    .ToList()
            })
            .SingleOrDefaultAsync(cancellationToken);

        return role is null ? NotFound() : View(role);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id, CancellationToken cancellationToken)
    {
        var role = await dbContext.Roles
            .Include(entity => entity.UserRoles)
            .SingleOrDefaultAsync(entity => entity.Id == id, cancellationToken);

        if (role is null)
        {
            return NotFound();
        }

        if (role.IsSystemRole)
        {
            ModelState.AddModelError(string.Empty, "System roles cannot be deleted.");
            return View(new RoleDetailsViewModel
            {
                Id = role.Id,
                Code = role.Code,
                Name = role.Name,
                Description = role.Description,
                IsSystemRole = role.IsSystemRole,
                CreatedAt = role.CreatedAt,
                UpdatedAt = role.UpdatedAt,
                Assignments = role.UserRoles
                    .Select(userRole => userRole.UserId.ToString())
                    .ToList()
            });
        }

        if (role.UserRoles.Count != 0)
        {
            ModelState.AddModelError(string.Empty, "Delete or revoke related assignments before removing this role.");
            return View(new RoleDetailsViewModel
            {
                Id = role.Id,
                Code = role.Code,
                Name = role.Name,
                Description = role.Description,
                IsSystemRole = role.IsSystemRole,
                CreatedAt = role.CreatedAt,
                UpdatedAt = role.UpdatedAt,
                Assignments = role.UserRoles.Select(userRole => userRole.UserId.ToString()).ToList()
            });
        }

        dbContext.Roles.Remove(role);
        await dbContext.SaveChangesAsync(cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    private async Task ValidateRoleAsync(RoleFormViewModel model, Guid? currentRoleId, CancellationToken cancellationToken)
    {
        model.Code = model.Code.Trim().ToUpperInvariant();
        model.Name = model.Name.Trim();

        if (await dbContext.Roles.AnyAsync(
                entity => entity.Code == model.Code && entity.Id != currentRoleId,
                cancellationToken))
        {
            ModelState.AddModelError(nameof(model.Code), "Role code already exists.");
        }
    }

    private static string? NormalizeText(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
