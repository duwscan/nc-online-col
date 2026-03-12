using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using wnc.Data;
using wnc.Features.IdentityAccess.Common;
using wnc.Features.IdentityAccess.UserRoles;
using wnc.Models;

namespace wnc.Controllers;

public sealed class UserRolesController(
    AppDbContext dbContext,
    IListQueryService<UserRolesListQuery, UserRoleListItemViewModel> listQueryService) : Controller
{
    public async Task<IActionResult> Index([FromQuery] UserRolesListQuery query, CancellationToken cancellationToken)
    {
        var result = await listQueryService.ExecuteAsync(query, cancellationToken);
        var (userOptions, roleOptions) = await LoadUserRoleFiltersAsync(query.UserId, query.RoleId, cancellationToken);

        var model = new UserRolesIndexViewModel
        {
            Query = query,
            Result = result,
            UserOptions = userOptions,
            RoleOptions = roleOptions,
            StateOptions = IdentityAccessSelectLists.RevocationStateOptions(query.State)
        };

        return View(model);
    }

    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var assignment = await dbContext.UserRoles
            .AsNoTracking()
            .Include(entity => entity.User)
            .Include(entity => entity.Role)
            .Include(entity => entity.AssignedByUser)
            .Where(entity => entity.Id == id)
            .Select(entity => new UserRoleDetailsViewModel
            {
                Id = entity.Id,
                UserId = entity.UserId,
                UserDisplay = entity.User.Username ?? entity.User.Email ?? entity.User.PhoneNumber ?? entity.UserId.ToString(),
                RoleId = entity.RoleId,
                RoleDisplay = $"{entity.Role.Code} - {entity.Role.Name}",
                AssignedBy = entity.AssignedBy,
                AssignedByDisplay = entity.AssignedByUser != null
                    ? entity.AssignedByUser.Username ?? entity.AssignedByUser.Email ?? entity.AssignedByUser.PhoneNumber
                    : null,
                AssignedAt = entity.AssignedAt,
                RevokedAt = entity.RevokedAt
            })
            .SingleOrDefaultAsync(cancellationToken);

        return assignment is null ? NotFound() : View(assignment);
    }

    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var model = await BuildFormAsync(new UserRoleFormViewModel(), cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UserRoleFormViewModel model, CancellationToken cancellationToken)
    {
        await ValidateUserRoleAsync(model, null, cancellationToken);
        if (!ModelState.IsValid)
        {
            return View(await BuildFormAsync(model, cancellationToken));
        }

        var assignment = new UserRole
        {
            Id = Guid.NewGuid(),
            UserId = model.UserId,
            RoleId = model.RoleId,
            AssignedBy = model.AssignedBy,
            AssignedAt = model.AssignedAt,
            RevokedAt = model.RevokedAt
        };

        dbContext.UserRoles.Add(assignment);
        await dbContext.SaveChangesAsync(cancellationToken);
        return RedirectToAction(nameof(Details), new { id = assignment.Id });
    }

    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var assignment = await dbContext.UserRoles.AsNoTracking().SingleOrDefaultAsync(entity => entity.Id == id, cancellationToken);
        if (assignment is null)
        {
            return NotFound();
        }

        var model = new UserRoleFormViewModel
        {
            UserId = assignment.UserId,
            RoleId = assignment.RoleId,
            AssignedBy = assignment.AssignedBy,
            AssignedAt = assignment.AssignedAt,
            RevokedAt = assignment.RevokedAt
        };

        return View(await BuildFormAsync(model, cancellationToken));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, UserRoleFormViewModel model, CancellationToken cancellationToken)
    {
        var assignment = await dbContext.UserRoles.SingleOrDefaultAsync(entity => entity.Id == id, cancellationToken);
        if (assignment is null)
        {
            return NotFound();
        }

        await ValidateUserRoleAsync(model, id, cancellationToken);
        if (!ModelState.IsValid)
        {
            return View(await BuildFormAsync(model, cancellationToken));
        }

        assignment.UserId = model.UserId;
        assignment.RoleId = model.RoleId;
        assignment.AssignedBy = model.AssignedBy;
        assignment.AssignedAt = model.AssignedAt;
        assignment.RevokedAt = model.RevokedAt;

        await dbContext.SaveChangesAsync(cancellationToken);
        return RedirectToAction(nameof(Details), new { id });
    }

    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var assignment = await dbContext.UserRoles
            .AsNoTracking()
            .Include(entity => entity.User)
            .Include(entity => entity.Role)
            .Include(entity => entity.AssignedByUser)
            .Where(entity => entity.Id == id)
            .Select(entity => new UserRoleDetailsViewModel
            {
                Id = entity.Id,
                UserId = entity.UserId,
                UserDisplay = entity.User.Username ?? entity.User.Email ?? entity.User.PhoneNumber ?? entity.UserId.ToString(),
                RoleId = entity.RoleId,
                RoleDisplay = $"{entity.Role.Code} - {entity.Role.Name}",
                AssignedBy = entity.AssignedBy,
                AssignedByDisplay = entity.AssignedByUser != null
                    ? entity.AssignedByUser.Username ?? entity.AssignedByUser.Email ?? entity.AssignedByUser.PhoneNumber
                    : null,
                AssignedAt = entity.AssignedAt,
                RevokedAt = entity.RevokedAt
            })
            .SingleOrDefaultAsync(cancellationToken);

        return assignment is null ? NotFound() : View(assignment);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id, CancellationToken cancellationToken)
    {
        var assignment = await dbContext.UserRoles.SingleOrDefaultAsync(entity => entity.Id == id, cancellationToken);
        if (assignment is null)
        {
            return NotFound();
        }

        assignment.RevokedAt ??= DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    private async Task<UserRoleFormViewModel> BuildFormAsync(UserRoleFormViewModel model, CancellationToken cancellationToken)
    {
        var users = await dbContext.Users
            .AsNoTracking()
            .OrderBy(entity => entity.Username)
            .Select(entity => new
            {
                entity.Id,
                Label = entity.Username ?? entity.Email ?? entity.PhoneNumber ?? entity.Id.ToString()
            })
            .ToListAsync(cancellationToken);

        var roles = await dbContext.Roles
            .AsNoTracking()
            .OrderBy(entity => entity.Name)
            .Select(entity => new
            {
                entity.Id,
                Label = $"{entity.Code} - {entity.Name}"
            })
            .ToListAsync(cancellationToken);

        model.UserOptions = IdentityAccessSelectLists.BuildEntityOptions(users.Select(entity => (entity.Id, entity.Label)), model.UserId, "Select a user");
        model.RoleOptions = IdentityAccessSelectLists.BuildEntityOptions(roles.Select(entity => (entity.Id, entity.Label)), model.RoleId, "Select a role");
        model.AssignedByOptions = IdentityAccessSelectLists.BuildEntityOptions(users.Select(entity => (entity.Id, entity.Label)), model.AssignedBy, "Not specified");

        return model;
    }

    private async Task<(IReadOnlyList<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> Users, IReadOnlyList<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> Roles)> LoadUserRoleFiltersAsync(
        Guid? selectedUserId,
        Guid? selectedRoleId,
        CancellationToken cancellationToken)
    {
        var users = await dbContext.Users
            .AsNoTracking()
            .OrderBy(entity => entity.Username)
            .Select(entity => new
            {
                entity.Id,
                Label = entity.Username ?? entity.Email ?? entity.PhoneNumber ?? entity.Id.ToString()
            })
            .ToListAsync(cancellationToken);

        var roles = await dbContext.Roles
            .AsNoTracking()
            .OrderBy(entity => entity.Name)
            .Select(entity => new
            {
                entity.Id,
                Label = $"{entity.Code} - {entity.Name}"
            })
            .ToListAsync(cancellationToken);

        return (
            IdentityAccessSelectLists.BuildEntityOptions(users.Select(entity => (entity.Id, entity.Label)), selectedUserId, "All users"),
            IdentityAccessSelectLists.BuildEntityOptions(roles.Select(entity => (entity.Id, entity.Label)), selectedRoleId, "All roles"));
    }

    private async Task ValidateUserRoleAsync(UserRoleFormViewModel model, Guid? currentAssignmentId, CancellationToken cancellationToken)
    {
        if (model.RevokedAt.HasValue && model.RevokedAt < model.AssignedAt)
        {
            ModelState.AddModelError(nameof(model.RevokedAt), "Revoked time must be after assigned time.");
        }

        if (!await dbContext.Users.AnyAsync(entity => entity.Id == model.UserId, cancellationToken))
        {
            ModelState.AddModelError(nameof(model.UserId), "Selected user does not exist.");
        }

        if (!await dbContext.Roles.AnyAsync(entity => entity.Id == model.RoleId, cancellationToken))
        {
            ModelState.AddModelError(nameof(model.RoleId), "Selected role does not exist.");
        }

        if (model.AssignedBy.HasValue && !await dbContext.Users.AnyAsync(entity => entity.Id == model.AssignedBy, cancellationToken))
        {
            ModelState.AddModelError(nameof(model.AssignedBy), "Assigned by user does not exist.");
        }

        if (!model.RevokedAt.HasValue &&
            await dbContext.UserRoles.AnyAsync(
                entity => entity.Id != currentAssignmentId &&
                          entity.UserId == model.UserId &&
                          entity.RoleId == model.RoleId &&
                          entity.RevokedAt == null,
                cancellationToken))
        {
            ModelState.AddModelError(string.Empty, "An active assignment for this user and role already exists.");
        }
    }
}
