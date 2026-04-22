using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using wnc.Data;
using wnc.Models;

namespace wnc.Controllers;

[Authorize]
public abstract class BaseController(AppDbContext dbContext) : Controller
{
    protected AppDbContext DbContext => dbContext;

    protected async Task<AppUser?> GetCurrentUserAsync()
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdValue) || !Guid.TryParse(userIdValue, out var userId))
        {
            return null;
        }

        return await dbContext.Users
            .AsNoTracking()
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    protected bool IsInRole(string role)
    {
        return User.IsInRole(role);
    }

    protected bool IsInAnyRole(params string[] roles)
    {
        return roles.Any(role => User.IsInRole(role));
    }

    protected void SetSuccessMessage(string message)
    {
        TempData["SuccessMessage"] = message;
    }

    protected void SetErrorMessage(string message)
    {
        TempData["ErrorMessage"] = message;
    }
}
