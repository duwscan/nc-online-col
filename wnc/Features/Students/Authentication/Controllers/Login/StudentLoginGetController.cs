using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using wnc.Features.Students.Authentication.ViewModels;

namespace wnc.Features.Students.Authentication.Controllers.Login;

[AllowAnonymous]
public class StudentLoginGetController : Controller
{
    [HttpGet("/auth/student/login")]
    public IActionResult Index(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true && User.IsInRole("CANDIDATE"))
        {
            return Url.IsLocalUrl(returnUrl) ? LocalRedirect(returnUrl!) : Redirect("/");
        }

        return View("~/Views/Auth/Student/Login.cshtml", new StudentLoginViewModel
        {
            ReturnUrl = returnUrl
        });
    }
}
