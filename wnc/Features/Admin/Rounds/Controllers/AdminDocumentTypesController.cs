using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using wnc.Data;
using wnc.Models;

namespace wnc.Features.Admin.Rounds.Controllers;

[Authorize(Roles = "ADMIN,ADMISSION_OFFICER")]
public class AdminDocumentTypesController(AppDbContext dbContext) : Controller
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

    [HttpGet("/admin/document-types")]
    public async Task<IActionResult> Index()
    {
        var types = await dbContext.DocumentTypes
            .AsNoTracking()
            .OrderBy(dt => dt.DocumentCode)
            .ToListAsync();

        return View("~/Features/Admin/Rounds/Views/DocumentTypesIndex.cshtml", types);
    }

    [HttpGet("/admin/document-types/create")]
    public IActionResult Create()
    {
        return View("~/Features/Admin/Rounds/Views/DocumentTypeCreate.cshtml", new DocumentType());
    }

    [HttpPost("/admin/document-types/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DocumentType model)
    {
        if (string.IsNullOrWhiteSpace(model.DocumentName))
            ModelState.AddModelError("DocumentName", "Tên giấy tờ là bắt buộc.");

        var documentCode = GenerateSlug(model.DocumentName);
        var exists = await dbContext.DocumentTypes
            .AnyAsync(dt => dt.DocumentCode == documentCode);
        if (exists)
            ModelState.AddModelError("DocumentName", "Mã giấy tờ đã tồn tại.");

        if (!ModelState.IsValid)
            return View("~/Features/Admin/Rounds/Views/DocumentTypeCreate.cshtml", model);

        var docType = new DocumentType
        {
            Id = Guid.NewGuid(),
            DocumentCode = documentCode,
            DocumentName = model.DocumentName.Trim(),
            Description = model.Description?.Trim(),
            Status = "ACTIVE",
            CreatedAt = DateTime.UtcNow
        };

        dbContext.DocumentTypes.Add(docType);
        await dbContext.SaveChangesAsync();

        TempData["SuccessMessage"] = "Tạo loại giấy tờ thành công.";
        return Redirect("/admin/document-types");
    }

    [HttpGet("/admin/document-types/edit/{id:guid}")]
    public async Task<IActionResult> Edit(Guid id)
    {
        var docType = await dbContext.DocumentTypes.FindAsync(id);
        if (docType == null)
            return NotFound();

        return View("~/Features/Admin/Rounds/Views/DocumentTypeEdit.cshtml", docType);
    }

    [HttpPost("/admin/document-types/edit/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, DocumentType model)
    {
        if (id != model.Id)
            return NotFound();

        if (string.IsNullOrWhiteSpace(model.DocumentName))
            ModelState.AddModelError("DocumentName", "Tên giấy tờ là bắt buộc.");

        var documentCode = GenerateSlug(model.DocumentName);
        var exists = await dbContext.DocumentTypes
            .AnyAsync(dt => dt.DocumentCode == documentCode && dt.Id != id);
        if (exists)
            ModelState.AddModelError("DocumentName", "Mã giấy tờ đã tồn tại.");

        if (!ModelState.IsValid)
            return View("~/Features/Admin/Rounds/Views/DocumentTypeEdit.cshtml", model);

        var docType = await dbContext.DocumentTypes.FindAsync(id);
        if (docType == null)
            return NotFound();

        docType.DocumentCode = documentCode;
        docType.DocumentName = model.DocumentName.Trim();
        docType.Description = model.Description?.Trim();
        docType.Status = model.Status;

        await dbContext.SaveChangesAsync();

        TempData["SuccessMessage"] = "Cập nhật loại giấy tờ thành công.";
        return Redirect("/admin/document-types");
    }

    [HttpPost("/admin/document-types/delete/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var docType = await dbContext.DocumentTypes.FindAsync(id);
        if (docType == null)
            return NotFound();

        var isInUse = await dbContext.RoundDocumentRequirements
            .AnyAsync(rdr => rdr.DocumentTypeId == id);
        if (isInUse)
        {
            TempData["ErrorMessage"] = "Giấy tờ đang được sử dụng, không thể xóa.";
            return Redirect("/admin/document-types");
        }

        dbContext.DocumentTypes.Remove(docType);
        await dbContext.SaveChangesAsync();

        TempData["SuccessMessage"] = "Xóa loại giấy tờ thành công.";
        return Redirect("/admin/document-types");
    }
}
