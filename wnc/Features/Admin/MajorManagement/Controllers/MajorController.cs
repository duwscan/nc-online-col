using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using wnc.Data;
using wnc.Features.Admin.MajorManagement.ViewModels;
using wnc.Models;

namespace wnc.Features.Admin.MajorManagement.Controllers;

[Authorize(Roles = "ADMIN")]
public class MajorController(AppDbContext dbContext) : Controller
{
    private static readonly string[] Statuses = ["ACTIVE", "INACTIVE"];

    [HttpGet("/admin/majors")]
    public async Task<IActionResult> Index(string? searchTerm = null, Guid? programId = null, string? status = null, int page = 1)
    {
        var normalizedSearchTerm = string.IsNullOrWhiteSpace(searchTerm) ? null : searchTerm.Trim();
        var normalizedProgramId = programId == Guid.Empty ? null : programId;
        var normalizedStatus = NormalizeStatus(status);
        var currentPage = page < 1 ? 1 : page;

        var availablePrograms = await GetAvailableProgramsAsync();
        if (normalizedProgramId.HasValue && availablePrograms.All(program => program.Id != normalizedProgramId.Value))
        {
            normalizedProgramId = null;
        }

        var query = dbContext.Majors
            .AsNoTracking()
            .Include(major => major.Program)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(normalizedSearchTerm))
        {
            query = query.Where(major =>
                major.MajorCode.Contains(normalizedSearchTerm) ||
                major.MajorName.Contains(normalizedSearchTerm));
        }

        if (normalizedProgramId.HasValue)
        {
            query = query.Where(major => major.ProgramId == normalizedProgramId.Value);
        }

        if (!string.IsNullOrWhiteSpace(normalizedStatus))
        {
            query = query.Where(major => major.Status == normalizedStatus);
        }

        const int pageSize = 10;
        var totalItems = await query.CountAsync();
        var majors = await query
            .OrderBy(major => major.DisplayOrder)
            .ThenBy(major => major.MajorCode)
            .Skip((currentPage - 1) * pageSize)
            .Take(pageSize)
            .Select(major => new MajorListItemViewModel
            {
                Id = major.Id,
                MajorCode = major.MajorCode,
                MajorName = major.MajorName,
                ProgramName = major.Program.ProgramName,
                Quota = major.Quota,
                Status = major.Status
            })
            .ToListAsync();

        var model = new MajorListViewModel
        {
            Majors = majors,
            AvailablePrograms = availablePrograms,
            AvailableStatuses = Statuses,
            SearchTerm = normalizedSearchTerm,
            ProgramId = normalizedProgramId,
            Status = normalizedStatus,
            Page = currentPage,
            PageSize = pageSize,
            TotalItems = totalItems
        };

        return View("~/Features/Admin/MajorManagement/Views/Index.cshtml", model);
    }

    [HttpGet("/admin/majors/create")]
    public async Task<IActionResult> Create()
    {
        var model = new MajorFormViewModel();
        await PopulateOptionsAsync(model);
        return View("~/Features/Admin/MajorManagement/Views/Create.cshtml", model);
    }

    [HttpPost("/admin/majors/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MajorFormViewModel model)
    {
        Normalize(model);
        await ValidateFormAsync(model, null);

        if (!ModelState.IsValid)
        {
            await PopulateOptionsAsync(model);
            return View("~/Features/Admin/MajorManagement/Views/Create.cshtml", model);
        }

        var now = DateTime.UtcNow;
        dbContext.Majors.Add(new Major
        {
            Id = Guid.NewGuid(),
            ProgramId = model.ProgramId!.Value,
            MajorCode = model.MajorCode,
            MajorName = model.MajorName,
            Description = model.Description,
            Quota = model.Quota,
            DisplayOrder = model.DisplayOrder,
            Status = model.Status,
            CreatedAt = now,
            UpdatedAt = now
        });

        await SaveChangesAsync(nameof(model.MajorCode), "Mã ngành đã tồn tại trong hệ thống.");
        if (!ModelState.IsValid)
        {
            await PopulateOptionsAsync(model);
            return View("~/Features/Admin/MajorManagement/Views/Create.cshtml", model);
        }

        TempData["SuccessMessage"] = "Đã tạo ngành đào tạo thành công.";
        return Redirect("/admin/majors");
    }

    [HttpGet("/admin/majors/edit/{id:guid}")]
    public async Task<IActionResult> Edit(Guid id)
    {
        var major = await dbContext.Majors
            .AsNoTracking()
            .Include(item => item.Program)
            .SingleOrDefaultAsync(item => item.Id == id);

        if (major is null)
        {
            return NotFound();
        }

        var model = new MajorFormViewModel
        {
            Id = major.Id,
            ProgramId = major.ProgramId,
            ProgramName = major.Program.ProgramName,
            MajorCode = major.MajorCode,
            MajorName = major.MajorName,
            Description = major.Description,
            Quota = major.Quota,
            DisplayOrder = major.DisplayOrder,
            Status = major.Status,
            IsEditMode = true
        };

        await PopulateOptionsAsync(model);
        return View("~/Features/Admin/MajorManagement/Views/Edit.cshtml", model);
    }

    [HttpPost("/admin/majors/edit/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, MajorFormViewModel model)
    {
        if (id != model.Id)
        {
            return NotFound();
        }

        Normalize(model);

        var major = await dbContext.Majors
            .Include(item => item.Program)
            .SingleOrDefaultAsync(item => item.Id == id);

        if (major is null)
        {
            return NotFound();
        }

        model.ProgramId = major.ProgramId;
        model.ProgramName = major.Program.ProgramName;

        await ValidateFormAsync(model, id);

        if (!ModelState.IsValid)
        {
            model.MajorCode = major.MajorCode;
            model.IsEditMode = true;
            await PopulateOptionsAsync(model);
            return View("~/Features/Admin/MajorManagement/Views/Edit.cshtml", model);
        }

        major.MajorName = model.MajorName;
        major.Description = model.Description;
        major.Quota = model.Quota;
        major.DisplayOrder = model.DisplayOrder;
        major.Status = model.Status;
        major.UpdatedAt = DateTime.UtcNow;

        await SaveChangesAsync(nameof(model.MajorCode), "Mã ngành đã tồn tại trong hệ thống.");
        if (!ModelState.IsValid)
        {
            model.MajorCode = major.MajorCode;
            model.IsEditMode = true;
            await PopulateOptionsAsync(model);
            return View("~/Features/Admin/MajorManagement/Views/Edit.cshtml", model);
        }

        TempData["SuccessMessage"] = "Đã cập nhật ngành đào tạo thành công.";
        return Redirect("/admin/majors");
    }

    [HttpPost("/admin/majors/delete/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var major = await dbContext.Majors
            .Include(m => m.RoundPrograms)
            .Include(m => m.ApplicationPreferences)
            .SingleOrDefaultAsync(m => m.Id == id);

        if (major is null)
        {
            return NotFound();
        }

        if (major.RoundPrograms.Count > 0 || major.ApplicationPreferences.Count > 0)
        {
            TempData["ErrorMessage"] = "Không thể xóa ngành đào tạo đã có dữ liệu liên quan.";
            return Redirect("/admin/majors");
        }

        dbContext.Majors.Remove(major);
        await dbContext.SaveChangesAsync();

        TempData["SuccessMessage"] = "Đã xóa ngành đào tạo thành công.";
        return Redirect("/admin/majors");
    }

    private async Task ValidateFormAsync(MajorFormViewModel model, Guid? currentId)
    {
        if (!Statuses.Contains(model.Status, StringComparer.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(nameof(model.Status), "Trạng thái không hợp lệ.");
        }

        if (!model.ProgramId.HasValue || model.ProgramId == Guid.Empty)
        {
            ModelState.AddModelError(nameof(model.ProgramId), "Vui lòng chọn chương trình đào tạo.");
        }
        else
        {
            var programExists = currentId.HasValue
                ? await dbContext.TrainingPrograms.AnyAsync(program => program.Id == model.ProgramId.Value)
                : await dbContext.TrainingPrograms.AnyAsync(program => program.Id == model.ProgramId.Value && program.Status == "ACTIVE");

            if (!programExists)
            {
                ModelState.AddModelError(nameof(model.ProgramId), "Chương trình đào tạo không hợp lệ hoặc đã ngừng hoạt động.");
            }
        }

        var duplicateCodeExists = await dbContext.Majors
            .AnyAsync(major => major.MajorCode == model.MajorCode && major.Id != currentId);

        if (duplicateCodeExists)
        {
            ModelState.AddModelError(nameof(model.MajorCode), "Mã ngành đã tồn tại trong hệ thống.");
        }
    }

    private async Task<IReadOnlyList<MajorProgramOptionViewModel>> GetAvailableProgramsAsync()
    {
        return await dbContext.TrainingPrograms
            .AsNoTracking()
            .Where(program => program.Status == "ACTIVE")
            .OrderBy(program => program.DisplayOrder)
            .ThenBy(program => program.ProgramName)
            .Select(program => new MajorProgramOptionViewModel
            {
                Id = program.Id,
                Name = program.ProgramName
            })
            .ToListAsync();
    }

    private async Task PopulateOptionsAsync(MajorFormViewModel model)
    {
        var availablePrograms = await GetAvailableProgramsAsync();
        if (model.ProgramId.HasValue && availablePrograms.All(program => program.Id != model.ProgramId.Value))
        {
            var selectedProgram = await dbContext.TrainingPrograms
                .AsNoTracking()
                .Where(program => program.Id == model.ProgramId.Value)
                .Select(program => new MajorProgramOptionViewModel
                {
                    Id = program.Id,
                    Name = program.ProgramName
                })
                .SingleOrDefaultAsync();

            if (selectedProgram is not null)
            {
                availablePrograms = availablePrograms.Concat([selectedProgram]).ToList();
            }
        }

        model.AvailablePrograms = availablePrograms
            .OrderBy(program => program.Name)
            .ToList();
        model.AvailableStatuses = Statuses;

        if (model.ProgramId.HasValue)
        {
            model.ProgramName = model.AvailablePrograms
                .FirstOrDefault(program => program.Id == model.ProgramId.Value)?.Name ?? model.ProgramName;
        }
    }

    private async Task SaveChangesAsync(string fieldName, string duplicateMessage)
    {
        try
        {
            await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(fieldName, duplicateMessage);
        }
    }

    private static void Normalize(MajorFormViewModel model)
    {
        model.MajorCode = model.MajorCode.Trim();
        model.MajorName = model.MajorName.Trim();
        model.Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim();
        model.Status = NormalizeStatus(model.Status) ?? "ACTIVE";
        if (model.ProgramId == Guid.Empty)
        {
            model.ProgramId = null;
        }
    }

    private static string? NormalizeStatus(string? status)
    {
        return string.IsNullOrWhiteSpace(status) ? null : status.Trim().ToUpperInvariant();
    }
}
