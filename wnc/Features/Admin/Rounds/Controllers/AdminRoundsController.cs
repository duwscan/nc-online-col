using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using wnc.Data;
using wnc.Features.Admin.Rounds.ViewModels;
using wnc.Models;

namespace wnc.Features.Admin.Rounds.Controllers;

[Authorize(Roles = "ADMIN,ADMISSION_OFFICER")]
public class AdminRoundsController(AppDbContext dbContext) : Controller
{
    private static readonly string[] RoundStatuses = ["DRAFT", "PUBLISHED", "CLOSED"];

    [HttpGet("/admin/rounds")]
    public async Task<IActionResult> Index(string? searchTerm = null, string? status = null, int page = 1)
    {
        var normalizedSearchTerm = string.IsNullOrWhiteSpace(searchTerm) ? null : searchTerm.Trim();
        var normalizedStatus = NormalizeStatus(status);
        var currentPage = page < 1 ? 1 : page;

        var query = dbContext.AdmissionRounds
            .AsNoTracking()
            .Where(round => round.DeletedAt == null);

        if (!string.IsNullOrWhiteSpace(normalizedSearchTerm))
        {
            query = query.Where(round =>
                round.RoundCode.Contains(normalizedSearchTerm) ||
                round.RoundName.Contains(normalizedSearchTerm));
        }

        if (!string.IsNullOrWhiteSpace(normalizedStatus))
        {
            query = query.Where(round => round.Status == normalizedStatus);
        }

        const int pageSize = 10;
        var totalItems = await query.CountAsync();
        var rounds = await query
            .OrderByDescending(round => round.AdmissionYear)
            .ThenByDescending(round => round.StartAt)
            .Skip((currentPage - 1) * pageSize)
            .Take(pageSize)
            .Select(round => new RoundListItemViewModel
            {
                Id = round.Id,
                RoundCode = round.RoundCode,
                RoundName = round.RoundName,
                AdmissionYear = round.AdmissionYear,
                StartAt = round.StartAt,
                EndAt = round.EndAt,
                Status = round.Status
            })
            .ToListAsync();

        var model = new RoundsListViewModel
        {
            Rounds = rounds,
            AvailableStatuses = RoundStatuses,
            SearchTerm = normalizedSearchTerm,
            Status = normalizedStatus,
            Page = currentPage,
            PageSize = pageSize,
            TotalItems = totalItems,
            SuccessMessage = TempData["SuccessMessage"] as string
        };

        return View("~/Features/Admin/Rounds/Views/Index.cshtml", model);
    }

    [HttpGet("/admin/rounds/create")]
    public async Task<IActionResult> Create()
    {
        var availableMethods = await dbContext.AdmissionMethods
            .AsNoTracking()
            .Where(m => m.Status == "ACTIVE")
            .OrderBy(m => m.MethodName)
            .Select(m => new AdmissionMethodOption
            {
                Id = m.Id,
                MethodCode = m.MethodCode,
                MethodName = m.MethodName
            })
            .ToListAsync();

        var model = new CreateRoundViewModel
        {
            AvailableStatuses = RoundStatuses,
            AvailableMethods = availableMethods
        };

        return View("~/Features/Admin/Rounds/Views/Create.cshtml", model);
    }

    [HttpPost("/admin/rounds/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateRoundViewModel model)
    {
        Normalize(model);
        await ValidateRoundInputAsync(model.RoundCode, model.Status, null);

        if (!ModelState.IsValid)
        {
            model.AvailableStatuses = RoundStatuses;
            model.AvailableMethods = await dbContext.AdmissionMethods
                .AsNoTracking()
                .Where(m => m.Status == "ACTIVE")
                .OrderBy(m => m.MethodName)
                .Select(m => new AdmissionMethodOption
                {
                    Id = m.Id,
                    MethodCode = m.MethodCode,
                    MethodName = m.MethodName
                })
                .ToListAsync();
            return View("~/Features/Admin/Rounds/Views/Create.cshtml", model);
        }

        var now = DateTime.UtcNow;
        var round = new AdmissionRound
        {
            Id = Guid.NewGuid(),
            RoundCode = model.RoundCode,
            RoundName = model.RoundName,
            AdmissionYear = model.AdmissionYear,
            StartAt = model.StartAt,
            EndAt = model.EndAt,
            Status = model.Status,
            Notes = model.Notes,
            AllowEnrollmentConfirmation = model.AllowEnrollmentConfirmation,
            CreatedBy = GetCurrentUserId(),
            CreatedAt = now,
            UpdatedAt = now,
            DeletedAt = null
        };

        dbContext.AdmissionRounds.Add(round);
        await dbContext.SaveChangesAsync();

        TempData["SuccessMessage"] = "Tạo đợt xét tuyển thành công.";
        return Redirect("/admin/rounds");
    }

    [HttpGet("/admin/rounds/edit/{id:guid}")]
    public async Task<IActionResult> Edit(Guid id)
    {
        var round = await dbContext.AdmissionRounds
            .AsNoTracking()
            .Include(r => r.RoundPrograms)
                .ThenInclude(rp => rp.Program)
            .Include(r => r.RoundPrograms)
                .ThenInclude(rp => rp.RoundAdmissionMethods)
            .SingleOrDefaultAsync(item => item.Id == id && item.DeletedAt == null);

        if (round is null)
        {
            return NotFound();
        }

        var availablePrograms = await dbContext.TrainingPrograms
            .AsNoTracking()
            .Where(p => p.Status == "ACTIVE")
            .OrderBy(p => p.DisplayOrder)
            .ThenBy(p => p.ProgramName)
            .Select(p => new ProgramOptionViewModel
            {
                Id = p.Id,
                ProgramCode = p.ProgramCode,
                ProgramName = p.ProgramName,
                EducationType = p.EducationType
            })
            .ToListAsync();

        var availableMethods = await dbContext.AdmissionMethods
            .AsNoTracking()
            .Where(m => m.Status == "ACTIVE")
            .OrderBy(m => m.MethodName)
            .Select(m => new AdmissionMethodOption
            {
                Id = m.Id,
                MethodCode = m.MethodCode,
                MethodName = m.MethodName
            })
            .ToListAsync();

        var model = new EditRoundViewModel
        {
            Id = round.Id,
            RoundCode = round.RoundCode,
            RoundName = round.RoundName,
            AdmissionYear = round.AdmissionYear,
            StartAt = round.StartAt,
            EndAt = round.EndAt,
            Status = round.Status,
            Notes = round.Notes,
            AllowEnrollmentConfirmation = round.AllowEnrollmentConfirmation,
            AvailableStatuses = RoundStatuses,
            AssignedPrograms = round.RoundPrograms.Select(rp => new RoundProgramItemViewModel
            {
                Id = rp.Id,
                ProgramId = rp.ProgramId,
                ProgramCode = rp.Program.ProgramCode,
                ProgramName = rp.Program.ProgramName,
                EducationType = rp.Program.EducationType,
                Quota = rp.Quota,
                PublishedQuota = rp.PublishedQuota,
                Status = rp.Status,
                AssignedMethodIds = rp.RoundAdmissionMethods.Select(rm => rm.MethodId).ToList()
            }).ToList(),
            AvailablePrograms = availablePrograms,
            AvailableMethods = availableMethods,
            SelectedMethodIds = round.RoundPrograms
                .SelectMany(rp => rp.RoundAdmissionMethods.Select(rm => rm.MethodId))
                .Distinct()
                .ToList()
        };

        return View("~/Features/Admin/Rounds/Views/Edit.cshtml", model);
    }

    [HttpPost("/admin/rounds/edit/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, EditRoundViewModel model)
    {
        if (id != model.Id)
        {
            return NotFound();
        }

        Normalize(model);
        await ValidateRoundInputAsync(model.RoundCode, model.Status, id);

        var round = await dbContext.AdmissionRounds
            .Include(r => r.RoundPrograms)
                .ThenInclude(rp => rp.RoundAdmissionMethods)
            .SingleOrDefaultAsync(item => item.Id == id && item.DeletedAt == null);

        if (round is null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            model.AvailableStatuses = RoundStatuses;
            model.AssignedPrograms = round.RoundPrograms.Select(rp => new RoundProgramItemViewModel
            {
                Id = rp.Id,
                ProgramId = rp.ProgramId,
                ProgramCode = rp.Program.ProgramCode,
                ProgramName = rp.Program.ProgramName,
                EducationType = rp.Program.EducationType,
                Quota = rp.Quota,
                PublishedQuota = rp.PublishedQuota,
                Status = rp.Status,
                AssignedMethodIds = rp.RoundAdmissionMethods.Select(rm => rm.MethodId).ToList()
            }).ToList();
            model.AvailableMethods = await dbContext.AdmissionMethods
                .AsNoTracking()
                .Where(m => m.Status == "ACTIVE")
                .OrderBy(m => m.MethodName)
                .Select(m => new AdmissionMethodOption
                {
                    Id = m.Id,
                    MethodCode = m.MethodCode,
                    MethodName = m.MethodName
                })
                .ToListAsync();
            return View("~/Features/Admin/Rounds/Views/Edit.cshtml", model);
        }

        round.RoundCode = model.RoundCode;
        round.RoundName = model.RoundName;
        round.AdmissionYear = model.AdmissionYear;
        round.StartAt = model.StartAt;
        round.EndAt = model.EndAt;
        round.Status = model.Status;
        round.Notes = model.Notes;
        round.AllowEnrollmentConfirmation = model.AllowEnrollmentConfirmation;
        round.UpdatedAt = DateTime.UtcNow;

        var form = HttpContext.Request.Form;
        var addedProgramIds = form["addProgramIds"]
            .Where(v => !string.IsNullOrEmpty(v) && Guid.TryParse(v, out _))
            .Select(v => Guid.Parse(v!))
            .ToHashSet();
        var removedRoundProgramIds = form["removeRoundProgramIds"]
            .Where(v => !string.IsNullOrEmpty(v) && Guid.TryParse(v, out _))
            .Select(v => Guid.Parse(v!))
            .ToHashSet();

        if (removedRoundProgramIds.Count > 0)
        {
            await dbContext.RoundPrograms
                .Where(rp => removedRoundProgramIds.Contains(rp.Id))
                .ExecuteDeleteAsync();

            var removedEntities = round.RoundPrograms
                .Where(rp => removedRoundProgramIds.Contains(rp.Id))
                .ToList();
            foreach (var entity in removedEntities)
            {
                dbContext.Entry(entity).State = EntityState.Detached;
            }
            round.RoundPrograms.Clear();
        }

        if (addedProgramIds.Count > 0)
        {
            var existingProgramIds = round.RoundPrograms.Select(rp => rp.ProgramId).ToHashSet();
            var toAdd = addedProgramIds.Where(pid => !existingProgramIds.Contains(pid)).ToList();

            foreach (var programId in toAdd)
            {
                var program = await dbContext.TrainingPrograms.FindAsync(programId);
                if (program != null)
                {
                    round.RoundPrograms.Add(new RoundProgram
                    {
                        Id = Guid.NewGuid(),
                        RoundId = round.Id,
                        ProgramId = programId,
                        Quota = program.Quota,
                        PublishedQuota = program.Quota,
                        Status = "ACTIVE",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                }
            }
        }

        // Handle admission methods
        var selectedMethodIds = form["SelectedMethodIds"]
            .Where(v => !string.IsNullOrEmpty(v) && Guid.TryParse(v, out _))
            .Select(v => Guid.Parse(v!))
            .ToHashSet();

        foreach (var rp in round.RoundPrograms)
        {
            var existingMethodIds = rp.RoundAdmissionMethods.Select(rm => rm.MethodId).ToHashSet();

            // Add new methods
            foreach (var methodId in selectedMethodIds.Where(mid => !existingMethodIds.Contains(mid)))
            {
                rp.RoundAdmissionMethods.Add(new RoundAdmissionMethod
                {
                    Id = Guid.NewGuid(),
                    RoundProgramId = rp.Id,
                    MethodId = methodId,
                    Status = "ACTIVE",
                    CreatedAt = DateTime.UtcNow
                });
            }

            // Remove deselected methods
            var toRemove = rp.RoundAdmissionMethods.Where(rm => !selectedMethodIds.Contains(rm.MethodId)).ToList();
            foreach (var rm in toRemove)
            {
                dbContext.RoundAdmissionMethods.Remove(rm);
            }
        }

        await dbContext.SaveChangesAsync();

        TempData["SuccessMessage"] = "Cập nhật đợt xét tuyển thành công.";
        return Redirect("/admin/rounds");
    }

    [HttpPost("/admin/rounds/delete/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var round = await dbContext.AdmissionRounds
            .SingleOrDefaultAsync(item => item.Id == id && item.DeletedAt == null);

        if (round is null)
        {
            return NotFound();
        }

        round.DeletedAt = DateTime.UtcNow;
        round.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();

        TempData["SuccessMessage"] = "Xóa đợt xét tuyển thành công.";
        return Redirect("/admin/rounds");
    }

    [HttpGet("/admin/rounds/{roundId:guid}/documents")]
    public async Task<IActionResult> EditDocuments(Guid roundId)
    {
        var round = await dbContext.AdmissionRounds
            .AsNoTracking()
            .Include(r => r.RoundPrograms)
                .ThenInclude(rp => rp.Program)
            .Include(r => r.RoundPrograms)
                .ThenInclude(rp => rp.Major)
            .Include(r => r.RoundPrograms)
                .ThenInclude(rp => rp.RoundDocumentRequirements)
                    .ThenInclude(rdr => rdr.DocumentType)
            .FirstOrDefaultAsync(r => r.Id == roundId && r.DeletedAt == null);

        if (round is null)
            return NotFound();

        var documentTypes = await dbContext.DocumentTypes
            .Where(dt => dt.Status == "ACTIVE")
            .AsNoTracking()
            .ToListAsync();

        var model = new EditRoundDocumentsViewModel
        {
            RoundId = round.Id,
            RoundCode = round.RoundCode,
            RoundName = round.RoundName,
            Programs = round.RoundPrograms.Select(rp => new RoundProgramDocumentsViewModel
            {
                Id = rp.Id,
                ProgramName = rp.Program.ProgramName,
                MajorName = rp.Major?.MajorName,
                Requirements = rp.RoundDocumentRequirements.Select(rdr => new RoundDocumentRequirementViewModel
                {
                    Id = rdr.Id,
                    RoundProgramId = rdr.RoundProgramId,
                    DocumentTypeId = rdr.DocumentTypeId,
                    DocumentName = rdr.DocumentType.DocumentName,
                    IsRequired = rdr.IsRequired,
                    RequiresNotarization = rdr.RequiresNotarization,
                    RequiresOriginalCopy = rdr.RequiresOriginalCopy,
                    MaxFiles = rdr.MaxFiles,
                    Notes = rdr.Notes
                }).ToList(),
                AvailableDocumentTypes = documentTypes.Select(dt => new DocumentTypeOption
                {
                    Id = dt.Id,
                    DocumentName = dt.DocumentName,
                    DocumentCode = dt.DocumentCode
                }).ToList()
            }).ToList()
        };

        return View("~/Features/Admin/Rounds/Views/EditDocuments.cshtml", model);
    }

    [HttpPost("/admin/rounds/{roundId:guid}/documents")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditDocumentsSave(Guid roundId, EditRoundDocumentsViewModel model)
    {
        var round = await dbContext.AdmissionRounds
            .Include(r => r.RoundPrograms)
                .ThenInclude(rp => rp.RoundDocumentRequirements)
            .FirstOrDefaultAsync(r => r.Id == roundId && r.DeletedAt == null);

        if (round is null)
            return NotFound();

        var form = HttpContext.Request.Form;
        var submittedDocTypeIds = form.Keys
            .Where(k => k.StartsWith("doc_"))
            .Select(k => Guid.Parse(k.Replace("doc_", "")))
            .Distinct()
            .ToList();

        foreach (var rp in round.RoundPrograms)
        {
            var existingReqs = rp.RoundDocumentRequirements.ToList();
            var submittedForRp = submittedDocTypeIds
                .SelectMany(docTypeId =>
                    form.Keys
                        .Where(k => k == $"doc_{docTypeId}")
                        .Select(_ => new { DocTypeId = docTypeId, RpId = rp.Id }))
                .Where(x => x.RpId == rp.Id)
                .Select(x => x.DocTypeId)
                .ToHashSet();

            var toRemove = existingReqs.Where(x => !submittedForRp.Contains(x.DocumentTypeId)).ToList();
            foreach (var req in toRemove)
                dbContext.RoundDocumentRequirements.Remove(req);

            foreach (var docTypeId in submittedForRp)
            {
                var isRequired = form[$"required_{docTypeId}"].FirstOrDefault() == "true";
                var requiresNotarization = form[$"notarization_{docTypeId}"].FirstOrDefault() == "true";
                var requiresOriginalCopy = form[$"original_{docTypeId}"].FirstOrDefault() == "true";
                var maxFilesStr = form[$"maxfiles_{docTypeId}"].FirstOrDefault();
                int.TryParse(maxFilesStr, out var maxFiles);

                var existing = rp.RoundDocumentRequirements.FirstOrDefault(x => x.DocumentTypeId == docTypeId);
                if (existing != null)
                {
                    existing.IsRequired = isRequired;
                    existing.RequiresNotarization = requiresNotarization;
                    existing.RequiresOriginalCopy = requiresOriginalCopy;
                    existing.MaxFiles = maxFiles > 0 ? maxFiles : 1;
                }
                else
                {
                    rp.RoundDocumentRequirements.Add(new RoundDocumentRequirement
                    {
                        Id = Guid.NewGuid(),
                        RoundProgramId = rp.Id,
                        DocumentTypeId = docTypeId,
                        IsRequired = isRequired,
                        RequiresNotarization = requiresNotarization,
                        RequiresOriginalCopy = requiresOriginalCopy,
                        MaxFiles = maxFiles > 0 ? maxFiles : 1,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
        }

        await dbContext.SaveChangesAsync();
        TempData["SuccessMessage"] = "Cập nhật yêu cầu hồ sơ thành công.";
        return Redirect($"/admin/rounds/{roundId}/documents");
    }

    [HttpGet("/admin/rounds/{roundId:guid}/methods")]
    public async Task<IActionResult> EditMethods(Guid roundId)
    {
        var round = await dbContext.AdmissionRounds
            .AsNoTracking()
            .Include(r => r.RoundPrograms)
                .ThenInclude(rp => rp.Program)
            .Include(r => r.RoundPrograms)
                .ThenInclude(rp => rp.Major)
            .Include(r => r.RoundPrograms)
                .ThenInclude(rp => rp.RoundAdmissionMethods)
                    .ThenInclude(ram => ram.Method)
            .FirstOrDefaultAsync(r => r.Id == roundId && r.DeletedAt == null);

        if (round is null)
            return NotFound();

        var methods = await dbContext.AdmissionMethods
            .Where(m => m.Status == "ACTIVE")
            .AsNoTracking()
            .ToListAsync();

        var model = new EditRoundMethodsViewModel
        {
            RoundId = round.Id,
            RoundCode = round.RoundCode,
            RoundName = round.RoundName,
            Programs = round.RoundPrograms.Select(rp => new RoundProgramMethodsViewModel
            {
                Id = rp.Id,
                ProgramName = rp.Program.ProgramName,
                MajorName = rp.Major?.MajorName,
                Methods = rp.RoundAdmissionMethods.Select(ram => new RoundMethodViewModel
                {
                    Id = ram.Id,
                    RoundProgramId = ram.RoundProgramId,
                    MethodId = ram.MethodId,
                    MethodName = ram.Method.MethodName,
                    MethodCode = ram.Method.MethodCode,
                    MinimumScore = ram.MinimumScore,
                    CalculationRule = ram.CalculationRule,
                    CombinationCode = ram.CombinationCode,
                    PriorityPolicy = ram.PriorityPolicy,
                    Status = ram.Status
                }).ToList(),
                AvailableMethods = methods.Select(m => new AdmissionMethodOption
                {
                    Id = m.Id,
                    MethodName = m.MethodName,
                    MethodCode = m.MethodCode
                }).ToList()
            }).ToList()
        };

        return View("~/Features/Admin/Rounds/Views/EditMethods.cshtml", model);
    }

    [HttpPost("/admin/rounds/{roundId:guid}/methods")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditMethodsSave(Guid roundId, EditRoundMethodsViewModel model)
    {
        var round = await dbContext.AdmissionRounds
            .Include(r => r.RoundPrograms)
                .ThenInclude(rp => rp.RoundAdmissionMethods)
            .FirstOrDefaultAsync(r => r.Id == roundId && r.DeletedAt == null);

        if (round is null)
            return NotFound();

        var form = HttpContext.Request.Form;
        var submittedMethodIds = form.Keys
            .Where(k => k.StartsWith("method_"))
            .Select(k => Guid.Parse(k.Replace("method_", "")))
            .Distinct()
            .ToList();

        foreach (var rp in round.RoundPrograms)
        {
            var existingMethods = rp.RoundAdmissionMethods.ToList();
            var submittedForRp = submittedMethodIds
                .SelectMany(methodId =>
                    form.Keys
                        .Where(k => k == $"method_{methodId}")
                        .Select(_ => new { MethodId = methodId, RpId = rp.Id }))
                .Where(x => x.RpId == rp.Id)
                .Select(x => x.MethodId)
                .ToHashSet();

            var toRemove = existingMethods.Where(x => !submittedForRp.Contains(x.MethodId)).ToList();
            foreach (var method in toRemove)
                dbContext.RoundAdmissionMethods.Remove(method);

            foreach (var methodId in submittedForRp)
            {
                var minScoreStr = form[$"minscore_{methodId}"].FirstOrDefault();
                decimal.TryParse(minScoreStr, out var minScore);

                var existing = rp.RoundAdmissionMethods.FirstOrDefault(x => x.MethodId == methodId);
                if (existing != null)
                {
                    existing.MinimumScore = minScore;
                }
                else
                {
                    rp.RoundAdmissionMethods.Add(new RoundAdmissionMethod
                    {
                        Id = Guid.NewGuid(),
                        RoundProgramId = rp.Id,
                        MethodId = methodId,
                        MinimumScore = minScore,
                        Status = "ACTIVE",
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
        }

        await dbContext.SaveChangesAsync();
        TempData["SuccessMessage"] = "Cập nhật phương thức xét tuyển thành công.";
        return Redirect($"/admin/rounds/{roundId}/methods");
    }

    private async Task ValidateRoundInputAsync(string roundCode, string? status, Guid? currentRoundId)
    {
        if (!RoundStatuses.Contains(NormalizeStatus(status) ?? string.Empty, StringComparer.OrdinalIgnoreCase))
        {
            ModelState.AddModelError("Status", "Trạng thái không hợp lệ.");
        }

        var duplicateRoundCodeExists = await dbContext.AdmissionRounds
            .AnyAsync(round =>
                round.RoundCode == roundCode &&
                round.Id != currentRoundId &&
                round.DeletedAt == null);

        if (duplicateRoundCodeExists)
        {
            ModelState.AddModelError("RoundCode", "Mã đợt xét tuyển đã tồn tại.");
        }
    }

    private Guid? GetCurrentUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(userId, out var parsedUserId) ? parsedUserId : null;
    }

    private static string? NormalizeStatus(string? status)
    {
        return string.IsNullOrWhiteSpace(status) ? null : status.Trim().ToUpperInvariant();
    }

    private static void Normalize(CreateRoundViewModel model)
    {
        model.RoundCode = model.RoundCode.Trim().ToUpperInvariant();
        model.RoundName = model.RoundName.Trim();
        model.Status = NormalizeStatus(model.Status) ?? "DRAFT";
        model.Notes = string.IsNullOrWhiteSpace(model.Notes) ? null : model.Notes.Trim();
    }

    private static void Normalize(EditRoundViewModel model)
    {
        model.RoundCode = model.RoundCode.Trim().ToUpperInvariant();
        model.RoundName = model.RoundName.Trim();
        model.Status = NormalizeStatus(model.Status) ?? "DRAFT";
        model.Notes = string.IsNullOrWhiteSpace(model.Notes) ? null : model.Notes.Trim();
    }
}
