using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using wnc.Data;
using wnc.Features.Students.Authentication.ViewModels;
using wnc.Models;
using BCryptNet = BCrypt.Net.BCrypt;

namespace wnc.Features.Students.Authentication.Controllers.Signup;

[AllowAnonymous]
public class StudentSignupController(AppDbContext dbContext) : Controller
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
        var email = looksLikeEmail ? identifier : string.Empty;
        var phoneNumber = looksLikeEmail ? string.Empty : identifier;

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
            Email = looksLikeEmail ? identifier : null,
            PhoneNumber = looksLikeEmail ? null : identifier,
            PasswordHash = BCryptNet.HashPassword(model.Password),
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
            Email = email,
            PhoneNumber = phoneNumber,
            CreatedAt = now,
            UpdatedAt = now
        };

        var userRole = new UserRole
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            RoleId = SeedIds.CandidateRoleId,
            AssignedAt = now,
            AssignedBy = null
        };

        dbContext.Users.Add(user);
        dbContext.Candidates.Add(candidate);
        dbContext.UserRoles.Add(userRole);

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

        // Sign in the new user
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Name, model.FullName),
            new(ClaimTypes.Role, "CANDIDATE"),
            new("role", "CANDIDATE")
        };

        if (!string.IsNullOrWhiteSpace(user.Email))
        {
            claims.Add(new Claim(ClaimTypes.Email, user.Email));
        }

        var principal = new ClaimsPrincipal(
            new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = false,
                AllowRefresh = true
            });

        return Redirect("/");
    }
}
