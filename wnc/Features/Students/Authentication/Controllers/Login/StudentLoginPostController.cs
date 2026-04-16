using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using wnc.Data;
using wnc.Features.Students.Authentication.ViewModels;
using wnc.Infrastructure.Security;
using wnc.Models;

namespace wnc.Features.Students.Authentication.Controllers.Login;

[AllowAnonymous]
public class StudentLoginPostController(
    AppDbContext dbContext,
    SignInManager<AppUser> signInManager,
    PortalSessionService portalSessionService) : Controller
{
    [HttpPost("/auth/student/login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(StudentLoginViewModel model)
    {
        model.LoginIdentifier = model.LoginIdentifier.Trim();

        if (!ModelState.IsValid)
        {
            model.Password = string.Empty;
            return View("~/Views/Auth/Student/Login.cshtml", model);
        }

        var configs = await dbContext.SystemConfigs
            .AsNoTracking()
            .Where(x => x.ConfigKey == "AUTH.LOGIN_BY_EMAIL" || x.ConfigKey == "AUTH.LOGIN_BY_PHONE")
            .ToDictionaryAsync(x => x.ConfigKey, x => x.ConfigValue);

        var allowEmailLogin = configs.TryGetValue("AUTH.LOGIN_BY_EMAIL", out var loginByEmail) &&
                              bool.TryParse(loginByEmail, out var allowEmail) &&
                              allowEmail;
        var allowPhoneLogin = configs.TryGetValue("AUTH.LOGIN_BY_PHONE", out var loginByPhone) &&
                              bool.TryParse(loginByPhone, out var allowPhone) &&
                              allowPhone;

        var loginIdentifier = model.LoginIdentifier;
        var looksLikeEmail = loginIdentifier.Contains('@');

        if ((looksLikeEmail && !allowEmailLogin) || (!looksLikeEmail && !allowPhoneLogin))
        {
            await LogAttemptAsync(null, loginIdentifier, "FAILED", "LOGIN_METHOD_DISABLED");
            model.Password = string.Empty;
            model.ErrorMessage = "This sign-in method is not enabled.";
            return View("~/Views/Auth/Student/Login.cshtml", model);
        }

        var user = await dbContext.Users
            .Include(x => x.UserRoles.Where(y => y.RevokedAt == null))
            .ThenInclude(x => x.Role)
            .SingleOrDefaultAsync(x =>
                x.DeletedAt == null &&
                (looksLikeEmail ? x.Email == loginIdentifier : x.PhoneNumber == loginIdentifier));

        if (user is null)
        {
            await LogAttemptAsync(user?.Id, loginIdentifier, "FAILED", "INVALID_CREDENTIALS");
            model.Password = string.Empty;
            model.ErrorMessage = "The provided credentials are invalid.";
            return View("~/Views/Auth/Student/Login.cshtml", model);
        }

        var passwordCheck = await signInManager.CheckPasswordSignInAsync(user, model.Password, lockoutOnFailure: false);
        if (!passwordCheck.Succeeded)
        {
            await LogAttemptAsync(user.Id, loginIdentifier, "FAILED", "INVALID_CREDENTIALS");
            model.Password = string.Empty;
            model.ErrorMessage = "The provided credentials are invalid.";
            return View("~/Views/Auth/Student/Login.cshtml", model);
        }

        if (!string.Equals(user.Status, "ACTIVE", StringComparison.OrdinalIgnoreCase))
        {
            await LogAttemptAsync(user.Id, loginIdentifier, "FAILED", "USER_INACTIVE");
            model.Password = string.Empty;
            model.ErrorMessage = "This account is not active.";
            return View("~/Views/Auth/Student/Login.cshtml", model);
        }

        var activeRoleCodes = user.UserRoles
            .Where(x => x.RevokedAt == null)
            .Select(x => x.Role.Code)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (!activeRoleCodes.Contains("CANDIDATE", StringComparer.OrdinalIgnoreCase))
        {
            await LogAttemptAsync(user.Id, loginIdentifier, "FAILED", "STUDENT_ROLE_REQUIRED");
            model.Password = string.Empty;
            model.ErrorMessage = "This account is not authorized for the student portal.";
            return View("~/Views/Auth/Student/Login.cshtml", model);
        }

        user.LastLoginAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;

        await signInManager.SignInAsync(user, new Microsoft.AspNetCore.Authentication.AuthenticationProperties
        {
            IsPersistent = model.RememberMe,
            AllowRefresh = true
        });
        portalSessionService.StoreSignIn(HttpContext, user, activeRoleCodes, "STUDENT");

        dbContext.AuthLogs.Add(CreateAuthLog(user.Id, loginIdentifier, "SUCCESS", null));
        await dbContext.SaveChangesAsync();

        return Url.IsLocalUrl(model.ReturnUrl) ? LocalRedirect(model.ReturnUrl!) : Redirect("/student/profile");
    }

    private async Task LogAttemptAsync(Guid? userId, string loginIdentifier, string status, string? failureReason)
    {
        dbContext.AuthLogs.Add(CreateAuthLog(userId, loginIdentifier, status, failureReason));
        await dbContext.SaveChangesAsync();
    }

    private AuthLog CreateAuthLog(Guid? userId, string loginIdentifier, string status, string? failureReason)
    {
        var userAgent = Request.Headers.UserAgent.ToString();
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

        return new AuthLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            LoginIdentifier = loginIdentifier,
            Status = status,
            FailureReason = failureReason,
            IpAddress = ipAddress?[..Math.Min(ipAddress.Length, 64)],
            UserAgent = userAgent[..Math.Min(userAgent.Length, 500)],
            LoggedAt = DateTime.UtcNow
        };
    }
}
