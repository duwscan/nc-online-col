using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using wnc.Data;
using wnc.Features.Students.Profile.ViewModels;
using wnc.Models;

namespace wnc.Features.Students.Profile.Controllers;

[Authorize(Roles = "CANDIDATE")]
public class StudentProfileController(AppDbContext dbContext) : Controller
{
    private AppDbContext _dbContext = dbContext;

    [HttpGet("/student/profile")]
    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return RedirectToAction("Login", "StudentLogin");

        var candidate = await _dbContext.Candidates
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.UserId == Guid.Parse(userId));

        if (candidate == null) return NotFound();

        var model = new StudentProfileViewModel
        {
            FullName = candidate.FullName,
            DateOfBirth = candidate.DateOfBirth,
            Gender = candidate.Gender,
            NationalId = candidate.NationalId,
            Email = candidate.Email,
            PhoneNumber = candidate.PhoneNumber,
            AddressLine = candidate.AddressLine,
            Ward = candidate.Ward,
            District = candidate.District,
            ProvinceCode = candidate.ProvinceCode
        };

        return View("~/Views/Student/Profile.cshtml", model);
    }

    [HttpPost("/student/profile")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(StudentProfileViewModel model)
    {
        if (!ModelState.IsValid)
            return View("~/Views/Student/Profile.cshtml", model);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return RedirectToAction("Login", "StudentLogin");

        var candidate = await _dbContext.Candidates
            .FirstOrDefaultAsync(c => c.UserId == Guid.Parse(userId));

        if (candidate == null) return NotFound();

        candidate.FullName = model.FullName;
        candidate.DateOfBirth = model.DateOfBirth;
        candidate.Gender = model.Gender;
        candidate.NationalId = model.NationalId;
        candidate.Email = model.Email;
        candidate.PhoneNumber = model.PhoneNumber;
        candidate.AddressLine = model.AddressLine;
        candidate.Ward = model.Ward;
        candidate.District = model.District;
        candidate.ProvinceCode = model.ProvinceCode;
        candidate.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        model.SuccessMessage = "Cập nhật hồ sơ thành công.";
        return View("~/Views/Student/Profile.cshtml", model);
    }
}
