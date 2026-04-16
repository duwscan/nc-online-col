using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using wnc.Infrastructure.Security;
using wnc.Models;

namespace wnc.Features.Admin.Authentication.Controllers.Logout;

[Authorize]
public class AdminLogoutController(
    SignInManager<AppUser> signInManager,
    PortalSessionService portalSessionService) : Controller
{
    [HttpGet("/auth/admin/logout")]
    public async Task<IActionResult> Index()
    {
        portalSessionService.Clear(HttpContext);
        await signInManager.SignOutAsync();
        return Redirect("/auth/admin/login");
    }
}
