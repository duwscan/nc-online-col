using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace wnc.Features.Admin.Authentication.Controllers.Logout;

[Authorize]
public class AdminLogoutController : Controller
{
    [HttpGet("/auth/admin/logout")]
    public async Task<IActionResult> Index()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Redirect("/auth/admin/login");
    }
}
