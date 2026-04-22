using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using wnc.Data;
using wnc.Models;

namespace wnc.Features.Admin.Rounds.Controllers;

[Authorize(Roles = "ADMIN,ADMISSION_OFFICER")]
public class AdminDocumentTypesThiController(AppDbContext dbContext) : Controller
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

    [HttpGet("/admin/document-types-grid")]
    public async Task<IActionResult> Index(string tenGiayTo, string moTa)
    {
        int ktRequire = 3;
        if (!string.IsNullOrWhiteSpace(moTa))
        {
            if (moTa.Any(char.IsDigit) || moTa.Replace(" ", "").Length < ktRequire)
            {
                TempData["ErrorMessage"] = "search thất bại: " + moTa + " phải có " + ktRequire + " kí tự và k có số";
                return View("~/Features/Admin/Rounds/Views/DocumentTypeGridCreate.cshtml", new List<DocumentType>());
            }
        }

        var query = dbContext.DocumentTypes.AsQueryable();

        if (!string.IsNullOrWhiteSpace(tenGiayTo))
        {
            query = query.Where(documentType => documentType.DocumentName.ToLower().Contains(tenGiayTo.ToLower().Trim()));
        }
        
        if (!string.IsNullOrWhiteSpace(moTa))
        {
            query = query.Where(documentType => documentType.Description.ToLower().Contains(moTa.ToLower().Trim()));
        }

        var values = await query.ToListAsync();
        return View("~/Features/Admin/Rounds/Views/DocumentTypesGrid.cshtml", values);
    }

    [HttpGet("/admin/document-types-grid/create")]
    public IActionResult Create()
    {
        return View("~/Features/Admin/Rounds/Views/DocumentTypeGridCreate.cshtml", new DocumentType());
    }

    [HttpPost("/admin/document-types-grid/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DocumentType model)
    {
        if (string.IsNullOrWhiteSpace(model.DocumentName))
        {
            TempData["ErrorMessage"] = "moẹ m nó rỗng";
            return View("~/Features/Admin/Rounds/Views/DocumentTypeGridCreate.cshtml", model);
        }

        var documentTypeCode = GenerateSlug(model.DocumentName);

        var query = dbContext.DocumentTypes.AsQueryable();
        var existed = query.Any(documentType => documentTypeCode == documentType.DocumentCode);

        if (existed)
        {
            TempData["ErrorMessage"] = "moẹ m nó đã tồn tại";
            return View("~/Features/Admin/Rounds/Views/DocumentTypeGridCreate.cshtml", model);
        }

        var documentType = new DocumentType
        {
            Id = Guid.NewGuid(),
            DocumentCode = model.DocumentCode,
            DocumentName = model.DocumentName,
            Description = model.Description,
            Status = "ACTIVE",
            CreatedAt = DateTime.UtcNow
        };

        dbContext.DocumentTypes.Add(documentType);
        await dbContext.SaveChangesAsync();
        
        TempData["SuccessMessage"] = "oke da luu";
        return View("~/Features/Admin/Rounds/Views/DocumentTypeGridCreate.cshtml", documentType);
    }

    [HttpGet("/admin/document-types-grid/edit/{id:guid}")]
    public async Task<IActionResult> Edit(Guid id)
    {
        var query = dbContext.DocumentTypes.AsQueryable();
        
        var documentType = query.FirstOrDefault(documentType => documentType.Id == id);

        if (documentType == null)
        {
            TempData["ErrorMessage"] = "moẹ m nó chưa có";
            return View("~/Features/Admin/Rounds/Views/DocumentTypeGridEdit.cshtml", new List<DocumentType>());
        }

        var model = new DocumentType
        {
            Id = documentType.Id,
            DocumentCode = documentType.DocumentCode,
            DocumentName = documentType.DocumentName,
            Description = documentType.Description,
            Status = documentType.Status,
            CreatedAt = documentType.CreatedAt
        };
        
        return View("~/Features/Admin/Rounds/Views/DocumentTypeGridEdit.cshtml", model);
    }

    [HttpPost("/admin/document-types-grid/edit/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, DocumentType model)
    {
        var query = dbContext.DocumentTypes.AsQueryable();
        
        var documentType = query.FirstOrDefault(documentType => documentType.Id == id);

        if (documentType == null)
        {
            TempData["ErrorMessage"] = "moẹ m nó chưa có";
            return View("~/Features/Admin/Rounds/Views/DocumentTypeGridEdit.cshtml", new List<DocumentType>());
        }
        
        if (string.IsNullOrWhiteSpace(model.DocumentName))
        {
            TempData["ErrorMessage"] = "moẹ m nó rỗng";
            return View("~/Features/Admin/Rounds/Views/DocumentTypeGridEdit.cshtml", model);
        }
        
        var newDTCode = GenerateSlug(model.DocumentName);

        var existed = dbContext.DocumentTypes.Any(documentType => documentType.DocumentCode == newDTCode);

        if (existed)
        {
            TempData["ErrorMessage"] = "code này nó đã tồn tại";
            return View("~/Features/Admin/Rounds/Views/DocumentTypeGridCreate.cshtml", model);
        }

        documentType.DocumentCode = newDTCode;
        documentType.DocumentName = model.DocumentName;
        documentType.Description = model.Description;
        documentType.Status = "ACTIVE";
        
        await dbContext.SaveChangesAsync();
        
        TempData["SuccessMessage"] = "oke đã chỉnh sửa";
        return Redirect("/admin/document-types-grid");
    }

    [HttpPost("/admin/document-types-grid/delete/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var query = dbContext.DocumentTypes.AsQueryable();
        
        var documentType = query.FirstOrDefault(documentType => documentType.Id == id);

        if (documentType == null)
        {
            TempData["ErrorMessage"] = "moẹ m nó chưa có dt id nay";
            return Redirect("/admin/document-types-grid");
        }
        
        dbContext.DocumentTypes.Remove(documentType);
        await dbContext.SaveChangesAsync();
        
        TempData["SuccessMessage"] = "oke đã xoa";
        return Redirect("/admin/document-types-grid");
    }
}
