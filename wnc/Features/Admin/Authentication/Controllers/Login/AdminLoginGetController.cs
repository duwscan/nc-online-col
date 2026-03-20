using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using wnc.Features.Admin.Authentication.ViewModels;

namespace wnc.Features.Admin.Authentication.Controllers.Login;

[AllowAnonymous]
public class AdminLoginGetController : Controller
{
    [HttpGet("/auth/admin/login")]
    public IActionResult Index(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true &&
            (User.IsInRole("ADMIN") || User.IsInRole("ADMISSION_OFFICER") || User.IsInRole("REPORT_VIEWER")))
        {
            return Url.IsLocalUrl(returnUrl) ? LocalRedirect(returnUrl!) : Redirect("/");
        }

        return View("~/Views/Auth/Admin/Login.cshtml", new AdminLoginViewModel
        {
            ReturnUrl = returnUrl
        });
    }
}
