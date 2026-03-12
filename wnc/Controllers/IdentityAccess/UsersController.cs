using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using wnc.Data;
using wnc.Features.IdentityAccess.Common;
using wnc.Features.IdentityAccess.Users;
using wnc.Models;

namespace wnc.Controllers;

public sealed class UsersController(
    AppDbContext dbContext,
    IListQueryService<UsersListQuery, UserListItemViewModel> listQueryService) : Controller
{
    public async Task<IActionResult> Index([FromQuery] UsersListQuery query, CancellationToken cancellationToken)
    {
        var result = await listQueryService.ExecuteAsync(query, cancellationToken);
        var model = new UsersIndexViewModel
        {
            Query = query,
            Result = result,
            StatusOptions = IdentityAccessSelectLists.UserStatusOptions(query.Status)
        };

        return View(model);
    }

    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users
            .AsNoTracking()
            .Where(entity => entity.Id == id)
            .Select(entity => new UserDetailsViewModel
            {
                Id = entity.Id,
                Username = entity.Username,
                Email = entity.Email,
                PhoneNumber = entity.PhoneNumber,
                Status = entity.Status,
                PasswordHash = entity.PasswordHash,
                EmailVerifiedAt = entity.EmailVerifiedAt,
                PhoneVerifiedAt = entity.PhoneVerifiedAt,
                LastLoginAt = entity.LastLoginAt,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
                DeletedAt = entity.DeletedAt,
                Roles = entity.UserRoles
                    .Where(userRole => userRole.RevokedAt == null)
                    .OrderBy(userRole => userRole.Role.Name)
                    .Select(userRole => $"{userRole.Role.Code} - {userRole.Role.Name}")
                    .ToList(),
                RecentAuthLogs = entity.AuthLogs
                    .OrderByDescending(log => log.LoggedAt)
                    .Take(5)
                    .Select(log => $"{log.LoggedAt:u} | {log.Status} | {log.LoginIdentifier}")
                    .ToList()
            })
            .SingleOrDefaultAsync(cancellationToken);

        return user is null ? NotFound() : View(user);
    }

    public IActionResult Create()
    {
        var model = BuildForm();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UserFormViewModel model, CancellationToken cancellationToken)
    {
        await ValidateUserAsync(model, null, cancellationToken);

        if (!ModelState.IsValid)
        {
            return View(BuildForm(model));
        }

        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            Username = NormalizeText(model.Username),
            Email = NormalizeText(model.Email),
            PhoneNumber = NormalizeText(model.PhoneNumber),
            PasswordHash = model.PasswordHash.Trim(),
            Status = model.Status.Trim().ToUpperInvariant(),
            EmailVerifiedAt = model.EmailVerifiedAt,
            PhoneVerifiedAt = model.PhoneVerifiedAt,
            LastLoginAt = model.LastLoginAt,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(cancellationToken);

        return RedirectToAction(nameof(Details), new { id = user.Id });
    }

    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users.AsNoTracking().SingleOrDefaultAsync(entity => entity.Id == id, cancellationToken);
        if (user is null)
        {
            return NotFound();
        }

        var model = BuildForm(new UserFormViewModel
        {
            Username = user.Username,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            PasswordHash = user.PasswordHash,
            Status = user.Status,
            EmailVerifiedAt = user.EmailVerifiedAt,
            PhoneVerifiedAt = user.PhoneVerifiedAt,
            LastLoginAt = user.LastLoginAt
        });

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, UserFormViewModel model, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users.SingleOrDefaultAsync(entity => entity.Id == id, cancellationToken);
        if (user is null)
        {
            return NotFound();
        }

        await ValidateUserAsync(model, id, cancellationToken);

        if (!ModelState.IsValid)
        {
            return View(BuildForm(model));
        }

        user.Username = NormalizeText(model.Username);
        user.Email = NormalizeText(model.Email);
        user.PhoneNumber = NormalizeText(model.PhoneNumber);
        user.PasswordHash = model.PasswordHash.Trim();
        user.Status = model.Status.Trim().ToUpperInvariant();
        user.EmailVerifiedAt = model.EmailVerifiedAt;
        user.PhoneVerifiedAt = model.PhoneVerifiedAt;
        user.LastLoginAt = model.LastLoginAt;
        user.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return RedirectToAction(nameof(Details), new { id });
    }

    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users
            .AsNoTracking()
            .Where(entity => entity.Id == id)
            .Select(entity => new UserDetailsViewModel
            {
                Id = entity.Id,
                Username = entity.Username,
                Email = entity.Email,
                PhoneNumber = entity.PhoneNumber,
                Status = entity.Status,
                PasswordHash = entity.PasswordHash,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
                DeletedAt = entity.DeletedAt,
                Roles = entity.UserRoles
                    .Where(userRole => userRole.RevokedAt == null)
                    .Select(userRole => $"{userRole.Role.Code} - {userRole.Role.Name}")
                    .ToList()
            })
            .SingleOrDefaultAsync(cancellationToken);

        return user is null ? NotFound() : View(user);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users.SingleOrDefaultAsync(entity => entity.Id == id, cancellationToken);
        if (user is null)
        {
            return NotFound();
        }

        user.Status = "DELETED";
        user.DeletedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    private UserFormViewModel BuildForm(UserFormViewModel? model = null)
    {
        model ??= new UserFormViewModel();
        model.StatusOptions = IdentityAccessSelectLists.UserStatusOptions(model.Status);
        return model;
    }

    private async Task ValidateUserAsync(UserFormViewModel model, Guid? currentUserId, CancellationToken cancellationToken)
    {
        model.Email = NormalizeText(model.Email);
        model.PhoneNumber = NormalizeText(model.PhoneNumber);

        if (string.IsNullOrWhiteSpace(model.Email))
        {
            ModelState.AddModelError(nameof(model.Email), "Email is required.");
        }

        if (string.IsNullOrWhiteSpace(model.PhoneNumber))
        {
            ModelState.AddModelError(nameof(model.PhoneNumber), "Phone number is required.");
        }

        if (model.EmailVerifiedAt.HasValue && string.IsNullOrWhiteSpace(model.Email))
        {
            ModelState.AddModelError(nameof(model.EmailVerifiedAt), "Email verified time requires an email value.");
        }

        if (model.PhoneVerifiedAt.HasValue && string.IsNullOrWhiteSpace(model.PhoneNumber))
        {
            ModelState.AddModelError(nameof(model.PhoneVerifiedAt), "Phone verified time requires a phone number.");
        }

        if (!string.IsNullOrWhiteSpace(model.Email) &&
            await dbContext.Users.AnyAsync(
                entity => entity.Email == model.Email && entity.Id != currentUserId,
                cancellationToken))
        {
            ModelState.AddModelError(nameof(model.Email), "Email is already in use.");
        }

        if (!string.IsNullOrWhiteSpace(model.PhoneNumber) &&
            await dbContext.Users.AnyAsync(
                entity => entity.PhoneNumber == model.PhoneNumber && entity.Id != currentUserId,
                cancellationToken))
        {
            ModelState.AddModelError(nameof(model.PhoneNumber), "Phone number is already in use.");
        }
    }

    private static string? NormalizeText(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
