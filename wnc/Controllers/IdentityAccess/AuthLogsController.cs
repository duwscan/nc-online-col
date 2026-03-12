using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using wnc.Data;
using wnc.Features.IdentityAccess.AuthLogs;
using wnc.Features.IdentityAccess.Common;
using wnc.Models;

namespace wnc.Controllers;

public sealed class AuthLogsController(
    AppDbContext dbContext,
    IListQueryService<AuthLogsListQuery, AuthLogListItemViewModel> listQueryService) : Controller
{
    public async Task<IActionResult> Index([FromQuery] AuthLogsListQuery query, CancellationToken cancellationToken)
    {
        var result = await listQueryService.ExecuteAsync(query, cancellationToken);
        var model = new AuthLogsIndexViewModel
        {
            Query = query,
            Result = result,
            UserOptions = await BuildUserOptionsAsync(query.UserId, "All users", cancellationToken),
            StatusOptions = IdentityAccessSelectLists.AuthLogStatusOptions(query.Status)
        };

        return View(model);
    }

    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var authLog = await dbContext.AuthLogs
            .AsNoTracking()
            .Include(entity => entity.User)
            .Where(entity => entity.Id == id)
            .Select(entity => new AuthLogDetailsViewModel
            {
                Id = entity.Id,
                UserId = entity.UserId,
                UserDisplay = entity.User != null
                    ? entity.User.Username ?? entity.User.Email ?? entity.User.PhoneNumber
                    : null,
                LoginIdentifier = entity.LoginIdentifier,
                Status = entity.Status,
                FailureReason = entity.FailureReason,
                IpAddress = entity.IpAddress,
                UserAgent = entity.UserAgent,
                LoggedAt = entity.LoggedAt
            })
            .SingleOrDefaultAsync(cancellationToken);

        return authLog is null ? NotFound() : View(authLog);
    }

    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var model = await BuildFormAsync(new AuthLogFormViewModel(), cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AuthLogFormViewModel model, CancellationToken cancellationToken)
    {
        await ValidateAuthLogAsync(model, cancellationToken);
        if (!ModelState.IsValid)
        {
            return View(await BuildFormAsync(model, cancellationToken));
        }

        var authLog = new AuthLog
        {
            Id = Guid.NewGuid(),
            UserId = model.UserId,
            LoginIdentifier = model.LoginIdentifier.Trim(),
            Status = model.Status.Trim().ToUpperInvariant(),
            FailureReason = NormalizeText(model.FailureReason),
            IpAddress = NormalizeText(model.IpAddress),
            UserAgent = NormalizeText(model.UserAgent),
            LoggedAt = model.LoggedAt
        };

        dbContext.AuthLogs.Add(authLog);
        await dbContext.SaveChangesAsync(cancellationToken);
        return RedirectToAction(nameof(Details), new { id = authLog.Id });
    }

    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var authLog = await dbContext.AuthLogs.AsNoTracking().SingleOrDefaultAsync(entity => entity.Id == id, cancellationToken);
        if (authLog is null)
        {
            return NotFound();
        }

        var model = new AuthLogFormViewModel
        {
            UserId = authLog.UserId,
            LoginIdentifier = authLog.LoginIdentifier,
            Status = authLog.Status,
            FailureReason = authLog.FailureReason,
            IpAddress = authLog.IpAddress,
            UserAgent = authLog.UserAgent,
            LoggedAt = authLog.LoggedAt
        };

        return View(await BuildFormAsync(model, cancellationToken));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, AuthLogFormViewModel model, CancellationToken cancellationToken)
    {
        var authLog = await dbContext.AuthLogs.SingleOrDefaultAsync(entity => entity.Id == id, cancellationToken);
        if (authLog is null)
        {
            return NotFound();
        }

        await ValidateAuthLogAsync(model, cancellationToken);
        if (!ModelState.IsValid)
        {
            return View(await BuildFormAsync(model, cancellationToken));
        }

        authLog.UserId = model.UserId;
        authLog.LoginIdentifier = model.LoginIdentifier.Trim();
        authLog.Status = model.Status.Trim().ToUpperInvariant();
        authLog.FailureReason = NormalizeText(model.FailureReason);
        authLog.IpAddress = NormalizeText(model.IpAddress);
        authLog.UserAgent = NormalizeText(model.UserAgent);
        authLog.LoggedAt = model.LoggedAt;

        await dbContext.SaveChangesAsync(cancellationToken);
        return RedirectToAction(nameof(Details), new { id });
    }

    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var authLog = await dbContext.AuthLogs
            .AsNoTracking()
            .Include(entity => entity.User)
            .Where(entity => entity.Id == id)
            .Select(entity => new AuthLogDetailsViewModel
            {
                Id = entity.Id,
                UserId = entity.UserId,
                UserDisplay = entity.User != null
                    ? entity.User.Username ?? entity.User.Email ?? entity.User.PhoneNumber
                    : null,
                LoginIdentifier = entity.LoginIdentifier,
                Status = entity.Status,
                FailureReason = entity.FailureReason,
                IpAddress = entity.IpAddress,
                UserAgent = entity.UserAgent,
                LoggedAt = entity.LoggedAt
            })
            .SingleOrDefaultAsync(cancellationToken);

        return authLog is null ? NotFound() : View(authLog);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id, CancellationToken cancellationToken)
    {
        var authLog = await dbContext.AuthLogs.SingleOrDefaultAsync(entity => entity.Id == id, cancellationToken);
        if (authLog is null)
        {
            return NotFound();
        }

        dbContext.AuthLogs.Remove(authLog);
        await dbContext.SaveChangesAsync(cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    private async Task<AuthLogFormViewModel> BuildFormAsync(AuthLogFormViewModel model, CancellationToken cancellationToken)
    {
        model.UserOptions = await BuildUserOptionsAsync(model.UserId, "Anonymous / unknown", cancellationToken);
        model.StatusOptions = IdentityAccessSelectLists.AuthLogStatusOptions(model.Status);
        return model;
    }

    private async Task<IReadOnlyList<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>> BuildUserOptionsAsync(
        Guid? selectedUserId,
        string emptyText,
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

        return IdentityAccessSelectLists.BuildEntityOptions(users.Select(entity => (entity.Id, entity.Label)), selectedUserId, emptyText);
    }

    private async Task ValidateAuthLogAsync(AuthLogFormViewModel model, CancellationToken cancellationToken)
    {
        if (model.UserId.HasValue && !await dbContext.Users.AnyAsync(entity => entity.Id == model.UserId, cancellationToken))
        {
            ModelState.AddModelError(nameof(model.UserId), "Selected user does not exist.");
        }

        if (string.Equals(model.Status, "FAILED", StringComparison.OrdinalIgnoreCase) && string.IsNullOrWhiteSpace(model.FailureReason))
        {
            ModelState.AddModelError(nameof(model.FailureReason), "Failure reason is required when status is FAILED.");
        }
    }

    private static string? NormalizeText(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
