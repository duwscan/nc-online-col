using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using wnc.Data;
using wnc.Models;

namespace wnc.Features.Admin.Rounds.Controllers;

public class AdminDocumentTypesControllerSearch(AppDbContext dbContext) : Controller
{
    [HttpGet("/admin/document-types-search")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> Index(string ?q, string? description)
    {
        var types = await dbContext.DocumentTypes
            .AsNoTracking()
            .Where(dt =>
                (string.IsNullOrWhiteSpace(q) || dt.DocumentName.Contains(q)) &&
                (string.IsNullOrWhiteSpace(description) || (!string.IsNullOrEmpty(dt.Description) && dt.Description.Contains(description)))
            )
            .OrderBy(dt => dt.DocumentCode)
            .ToListAsync();

        return View("~/Features/Admin/Rounds/Views/DocumentTypesIndex.cshtml", types);
    }

}
