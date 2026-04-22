using System.Collections;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using wnc.Data;
using wnc.Models;

namespace wnc.Features.Admin.Rounds.Controllers;

[Authorize(Roles = "ADMIN,ADMISSION_OFFICER")]
public class AdminDocumentTypesControllerThi(AppDbContext dbContext) : Controller
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
    
    [HttpGet("/admin/document-types/search")]
    public async Task<IActionResult> Search(string giayTo, string moTa)
    {
        if (!string.IsNullOrEmpty(moTa))
        {
            if (moTa.Any(char.IsDigit) || moTa.Replace(" ", "").Length <= 3)
            {
                TempData["ErrorMessage"] = "cái 2 phải > 3 kí tu và k đc chứa số";
                return View("~/Features/Admin/Rounds/Views/DocumentTypesIndexThi.cshtml", new List<DocumentType>());
            }
        }
        
        var query = dbContext.DocumentTypes.AsQueryable();

        if (!string.IsNullOrWhiteSpace(giayTo))
        {
            query = query.Where(dt => dt.DocumentName.ToLower().Contains(giayTo.ToLower().Trim()));
        }

        if (!string.IsNullOrWhiteSpace(moTa))
        {
            query = query.Where(dt => dt.Description.ToLower().Contains(moTa.ToLower().Trim()));
        }

        var values = await query.ToListAsync();
        
        return View("~/Features/Admin/Rounds/Views/DocumentTypesIndexThi.cshtml", values);
    }
}
