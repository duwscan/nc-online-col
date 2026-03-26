using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using wnc.Data;
using wnc.Features.Students.Authentication.ViewModels;
using wnc.Infrastructure.Security;
using wnc.Models;

namespace wnc.Features.Students.Authentication.Controllers.Signup;

[AllowAnonymous]
public class StudentSignupController(
    AppDbContext dbContext,
    UserManager<AppUser> userManager,
    SignInManager<AppUser> signInManager,
    PortalSessionService portalSessionService) : Controller
{
    [HttpGet("/auth/student/signup")]
    public IActionResult Index(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true && User.IsInRole("CANDIDATE"))
        {
            return Url.IsLocalUrl(returnUrl) ? LocalRedirect(returnUrl!) : Redirect("/");
        }

        return View("~/Views/Auth/Student/Signup.cshtml", new StudentSignupViewModel());
    }

    [HttpPost("/auth/student/signup")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(StudentSignupViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("~/Views/Auth/Student/Signup.cshtml", model);
        }

        var identifier = model.LoginIdentifier.Trim();
        var looksLikeEmail = identifier.Contains('@');

        // Check if user already exists
        var existingUser = await dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.DeletedAt == null &&
                (looksLikeEmail ? x.Email == identifier : x.PhoneNumber == identifier));

        if (existingUser != null)
        {
            model.ErrorMessage = "An account with this email or phone number already exists.";
            return View("~/Views/Auth/Student/Signup.cshtml", model);
        }

        // Validate age (must be at least 16)
        var minAgeDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-16));
        if (model.DateOfBirth > minAgeDate)
        {
            ModelState.AddModelError(nameof(model.DateOfBirth), "You must be at least 16 years old to register.");
            return View("~/Views/Auth/Student/Signup.cshtml", model);
        }

        var userId = Guid.NewGuid();
        var candidateId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        var user = new AppUser
        {
            Id = userId,
            Username = looksLikeEmail ? identifier : $"student.{userId:N}",
            Email = looksLikeEmail ? identifier : null,
            PhoneNumber = looksLikeEmail ? null : identifier,
            Status = "ACTIVE",
            CreatedAt = now,
            UpdatedAt = now
        };

        var candidate = new Candidate
        {
            Id = candidateId,
            UserId = userId,
            FullName = model.FullName.Trim(),
            DateOfBirth = model.DateOfBirth,
            Gender = model.Gender,
            Email = looksLikeEmail ? identifier : string.Empty,
            PhoneNumber = looksLikeEmail ? string.Empty : identifier,
            CreatedAt = now,
            UpdatedAt = now
        };

        var createUserResult = await userManager.CreateAsync(user, model.Password);
        if (!createUserResult.Succeeded)
        {
            model.ErrorMessage = string.Join(" ", createUserResult.Errors.Select(x => x.Description));
            return View("~/Views/Auth/Student/Signup.cshtml", model);
        }

        var addRoleResult = await userManager.AddToRoleAsync(user, "CANDIDATE");
        if (!addRoleResult.Succeeded)
        {
            model.ErrorMessage = string.Join(" ", addRoleResult.Errors.Select(x => x.Description));
            return View("~/Views/Auth/Student/Signup.cshtml", model);
        }

        dbContext.Candidates.Add(candidate);

        // Log successful registration
        var userAgent = Request.Headers.UserAgent.ToString();
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        dbContext.AuthLogs.Add(new AuthLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            LoginIdentifier = identifier,
            Status = "SUCCESS",
            FailureReason = "REGISTER",
            IpAddress = ipAddress?[..Math.Min(ipAddress.Length, 64)],
            UserAgent = userAgent[..Math.Min(userAgent.Length, 500)],
            LoggedAt = now
        });

        await dbContext.SaveChangesAsync();

        await signInManager.SignInAsync(user, isPersistent: false);
        portalSessionService.StoreSignIn(HttpContext, user, ["CANDIDATE"], "STUDENT");

        return Redirect("/");
    }
}
