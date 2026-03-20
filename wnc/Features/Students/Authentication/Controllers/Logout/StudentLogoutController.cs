using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace wnc.Features.Students.Authentication.Controllers.Logout;

[Authorize]
public class StudentLogoutController : Controller
{
    [HttpGet("/auth/student/logout")]
    public async Task<IActionResult> Index()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Redirect("/auth/student/login");
    }
}
