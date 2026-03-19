using Microsoft.AspNetCore.Mvc;
using wnc.Models;

namespace wnc.Controllers;

public class AccountController : Controller
{
    [HttpGet]
    public IActionResult Login()
    {
        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        TempData["LoginMessage"] = $"Dang nhap thanh cong cho {model.UserName}.";
        return RedirectToAction(nameof(Login));
    }
}
