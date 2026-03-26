using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using wnc.Infrastructure.Security;
using wnc.Models;

namespace wnc.Features.Students.Authentication.Controllers.Logout;

[Authorize]
public class StudentLogoutController(
    SignInManager<AppUser> signInManager,
    PortalSessionService portalSessionService) : Controller
{
    [HttpGet("/auth/student/logout")]
    public async Task<IActionResult> Index()
    {
        portalSessionService.Clear(HttpContext);
        await signInManager.SignOutAsync();
        return Redirect("/auth/student/login");
    }
}
