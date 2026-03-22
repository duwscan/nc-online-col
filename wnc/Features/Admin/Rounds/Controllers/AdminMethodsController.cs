using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using wnc.Data;
using wnc.Models;

namespace wnc.Features.Admin.Rounds.Controllers;

[Authorize(Roles = "ADMIN,ADMISSION_OFFICER")]
public class AdminMethodsController(AppDbContext dbContext) : Controller
{
    private static string GenerateSlug(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return string.Empty;
        var slug = name.ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("_", "-");
        slug = Regex.Replace(slug, @"[^a-z0-9\-]", "");
        slug = Regex.Replace(slug, @"-+", "-");
        return slug.Trim('-');
    }

    [HttpGet("/admin/methods")]
    public async Task<IActionResult> Index()
    {
        var methods = await dbContext.AdmissionMethods
            .AsNoTracking()
            .OrderBy(m => m.MethodCode)
            .ToListAsync();

        return View("~/Features/Admin/Rounds/Views/MethodsIndex.cshtml", methods);
    }

    [HttpGet("/admin/methods/create")]
    public IActionResult Create()
    {
        return View("~/Features/Admin/Rounds/Views/MethodCreate.cshtml", new AdmissionMethod());
    }

    [HttpPost("/admin/methods/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AdmissionMethod model)
    {
        if (string.IsNullOrWhiteSpace(model.MethodName))
            ModelState.AddModelError("MethodName", "Tên phương thức là bắt buộc.");

        var methodCode = GenerateSlug(model.MethodName);
        var exists = await dbContext.AdmissionMethods
            .AnyAsync(m => m.MethodCode == methodCode);
        if (exists)
            ModelState.AddModelError("MethodName", "Mã phương thức đã tồn tại.");

        if (!ModelState.IsValid)
            return View("~/Features/Admin/Rounds/Views/MethodCreate.cshtml", model);

        var method = new AdmissionMethod
        {
            Id = Guid.NewGuid(),
            MethodCode = methodCode,
            MethodName = model.MethodName.Trim(),
            Description = model.Description?.Trim(),
            Status = "ACTIVE",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        dbContext.AdmissionMethods.Add(method);
        await dbContext.SaveChangesAsync();

        TempData["SuccessMessage"] = "Tạo phương thức xét tuyển thành công.";
        return Redirect("/admin/methods");
    }

    [HttpGet("/admin/methods/edit/{id:guid}")]
    public async Task<IActionResult> Edit(Guid id)
    {
        var method = await dbContext.AdmissionMethods.FindAsync(id);
        if (method == null)
            return NotFound();

        return View("~/Features/Admin/Rounds/Views/MethodEdit.cshtml", method);
    }

    [HttpPost("/admin/methods/edit/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, AdmissionMethod model)
    {
        if (id != model.Id)
            return NotFound();

        if (string.IsNullOrWhiteSpace(model.MethodName))
            ModelState.AddModelError("MethodName", "Tên phương thức là bắt buộc.");

        var methodCode = GenerateSlug(model.MethodName);
        var exists = await dbContext.AdmissionMethods
            .AnyAsync(m => m.MethodCode == methodCode && m.Id != id);
        if (exists)
            ModelState.AddModelError("MethodName", "Mã phương thức đã tồn tại.");

        if (!ModelState.IsValid)
            return View("~/Features/Admin/Rounds/Views/MethodEdit.cshtml", model);

        var method = await dbContext.AdmissionMethods.FindAsync(id);
        if (method == null)
            return NotFound();

        method.MethodCode = methodCode;
        method.MethodName = model.MethodName.Trim();
        method.Description = model.Description?.Trim();
        method.Status = model.Status;
        method.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();

        TempData["SuccessMessage"] = "Cập nhật phương thức xét tuyển thành công.";
        return Redirect("/admin/methods");
    }

    [HttpPost("/admin/methods/delete/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var method = await dbContext.AdmissionMethods.FindAsync(id);
        if (method == null)
            return NotFound();

        var isInUse = await dbContext.RoundAdmissionMethods
            .AnyAsync(ram => ram.MethodId == id);
        if (isInUse)
        {
            TempData["ErrorMessage"] = "Phương thức đang được sử dụng, không thể xóa.";
            return Redirect("/admin/methods");
        }

        dbContext.AdmissionMethods.Remove(method);
        await dbContext.SaveChangesAsync();

        TempData["SuccessMessage"] = "Xóa phương thức xét tuyển thành công.";
        return Redirect("/admin/methods");
    }
}
