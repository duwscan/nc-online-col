using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using wnc.Data;
using wnc.Features.Admin.TrainingProgramManagement.ViewModels;
using wnc.Models;

namespace wnc.Features.Admin.TrainingProgramManagement.Controllers;

[Authorize(Roles = "ADMIN")]
public class TrainingProgramController(AppDbContext dbContext) : Controller
{
    private static readonly TrainingProgramFilterOptionViewModel[] EducationTypes =
    [
        new() { Value = "CAO_DANG", Label = "Cao đẳng" },
        new() { Value = "LIEN_THONG", Label = "Liên thông" },
        new() { Value = "VAN_BANG_2", Label = "Văn bằng 2" },
        new() { Value = "TU_XA", Label = "Từ xa" }
    ];

    private static readonly string[] Statuses = ["ACTIVE", "INACTIVE"];

    [HttpGet("/admin/programs")]
    public async Task<IActionResult> Index(string? searchTerm = null, string? educationType = null, string? status = null, int page = 1)
    {
        var normalizedSearchTerm = string.IsNullOrWhiteSpace(searchTerm) ? null : searchTerm.Trim();
        var normalizedEducationType = NormalizeEducationType(educationType);
        var normalizedStatus = NormalizeStatus(status);
        var currentPage = page < 1 ? 1 : page;

        var query = dbContext.TrainingPrograms.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(normalizedSearchTerm))
        {
            query = query.Where(program =>
                program.ProgramCode.Contains(normalizedSearchTerm) ||
                program.ProgramName.Contains(normalizedSearchTerm));
        }

        if (!string.IsNullOrWhiteSpace(normalizedEducationType))
        {
            query = query.Where(program => program.EducationType == normalizedEducationType);
        }

        if (!string.IsNullOrWhiteSpace(normalizedStatus))
        {
            query = query.Where(program => program.Status == normalizedStatus);
        }

        const int pageSize = 10;
        var totalItems = await query.CountAsync();
        var programs = await query
            .OrderBy(program => program.DisplayOrder)
            .ThenBy(program => program.ProgramCode)
            .Skip((currentPage - 1) * pageSize)
            .Take(pageSize)
            .Select(program => new TrainingProgramListItemViewModel
            {
                Id = program.Id,
                ProgramCode = program.ProgramCode,
                ProgramName = program.ProgramName,
                EducationType = program.EducationType,
                EducationTypeLabel = GetEducationTypeLabel(program.EducationType),
                TuitionFee = program.TuitionFee,
                Quota = program.Quota,
                Status = program.Status,
                DisplayOrder = program.DisplayOrder
            })
            .ToListAsync();

        var model = new TrainingProgramListViewModel
        {
            Programs = programs,
            AvailableEducationTypes = EducationTypes,
            AvailableStatuses = Statuses,
            SearchTerm = normalizedSearchTerm,
            EducationType = normalizedEducationType,
            Status = normalizedStatus,
            Page = currentPage,
            PageSize = pageSize,
            TotalItems = totalItems
        };

        return View("~/Features/Admin/TrainingProgramManagement/Views/Index.cshtml", model);
    }

    [HttpGet("/admin/programs/create")]
    public IActionResult Create()
    {
        var model = new TrainingProgramFormViewModel();
        PopulateOptions(model);
        return View("~/Features/Admin/TrainingProgramManagement/Views/Create.cshtml", model);
    }

    [HttpPost("/admin/programs/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TrainingProgramFormViewModel model)
    {
        Normalize(model);
        await ValidateFormAsync(model, null);

        if (!ModelState.IsValid)
        {
            PopulateOptions(model);
            return View("~/Features/Admin/TrainingProgramManagement/Views/Create.cshtml", model);
        }

        var now = DateTime.UtcNow;
        dbContext.TrainingPrograms.Add(new TrainingProgram
        {
            Id = Guid.NewGuid(),
            ProgramCode = model.ProgramCode,
            ProgramName = model.ProgramName,
            EducationType = model.EducationType,
            Description = model.Description,
            TuitionFee = model.TuitionFee,
            DurationText = model.DurationText,
            Quota = model.Quota,
            ManagingUnit = model.ManagingUnit,
            Status = model.Status,
            DisplayOrder = model.DisplayOrder,
            CreatedAt = now,
            UpdatedAt = now
        });

        await SaveChangesAsync(nameof(model.ProgramCode), "Mã chương trình đã tồn tại trong hệ thống.");
        if (!ModelState.IsValid)
        {
            PopulateOptions(model);
            return View("~/Features/Admin/TrainingProgramManagement/Views/Create.cshtml", model);
        }

        TempData["SuccessMessage"] = "Đã tạo chương trình đào tạo thành công.";
        return Redirect("/admin/programs");
    }

    [HttpGet("/admin/programs/edit/{id:guid}")]
    public async Task<IActionResult> Edit(Guid id)
    {
        var program = await dbContext.TrainingPrograms
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == id);

        if (program is null)
        {
            return NotFound();
        }

        var model = new TrainingProgramFormViewModel
        {
            Id = program.Id,
            ProgramCode = program.ProgramCode,
            ProgramName = program.ProgramName,
            EducationType = program.EducationType,
            Description = program.Description,
            TuitionFee = program.TuitionFee,
            DurationText = program.DurationText,
            Quota = program.Quota,
            ManagingUnit = program.ManagingUnit,
            Status = program.Status,
            DisplayOrder = program.DisplayOrder,
            IsEditMode = true
        };

        PopulateOptions(model);
        return View("~/Features/Admin/TrainingProgramManagement/Views/Edit.cshtml", model);
    }

    [HttpPost("/admin/programs/edit/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, TrainingProgramFormViewModel model)
    {
        if (id != model.Id)
        {
            return NotFound();
        }

        Normalize(model);
        await ValidateFormAsync(model, id);

        var program = await dbContext.TrainingPrograms.SingleOrDefaultAsync(item => item.Id == id);
        if (program is null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            model.ProgramCode = program.ProgramCode;
            model.IsEditMode = true;
            PopulateOptions(model);
            return View("~/Features/Admin/TrainingProgramManagement/Views/Edit.cshtml", model);
        }

        program.ProgramName = model.ProgramName;
        program.EducationType = model.EducationType;
        program.Description = model.Description;
        program.TuitionFee = model.TuitionFee;
        program.DurationText = model.DurationText;
        program.Quota = model.Quota;
        program.ManagingUnit = model.ManagingUnit;
        program.Status = model.Status;
        program.DisplayOrder = model.DisplayOrder;
        program.UpdatedAt = DateTime.UtcNow;

        await SaveChangesAsync(nameof(model.ProgramCode), "Mã chương trình đã tồn tại trong hệ thống.");
        if (!ModelState.IsValid)
        {
            model.ProgramCode = program.ProgramCode;
            model.IsEditMode = true;
            PopulateOptions(model);
            return View("~/Features/Admin/TrainingProgramManagement/Views/Edit.cshtml", model);
        }

        TempData["SuccessMessage"] = "Đã cập nhật chương trình đào tạo thành công.";
        return Redirect("/admin/programs");
    }

    [HttpPost("/admin/programs/delete/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var program = await dbContext.TrainingPrograms
            .Include(p => p.Majors)
            .Include(p => p.RoundPrograms)
            .Include(p => p.ApplicationPreferences)
            .SingleOrDefaultAsync(p => p.Id == id);

        if (program is null)
        {
            return NotFound();
        }

        if (program.Majors.Count > 0 || program.RoundPrograms.Count > 0 || program.ApplicationPreferences.Count > 0)
        {
            TempData["ErrorMessage"] = "Không thể xóa chương trình đào tạo đã có dữ liệu liên quan.";
            return Redirect("/admin/programs");
        }

        dbContext.TrainingPrograms.Remove(program);
        await dbContext.SaveChangesAsync();

        TempData["SuccessMessage"] = "Đã xóa chương trình đào tạo thành công.";
        return Redirect("/admin/programs");
    }

    [HttpPost("/admin/programs/toggle-status/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(Guid id, string? returnUrl = null)
    {
        var program = await dbContext.TrainingPrograms.SingleOrDefaultAsync(item => item.Id == id);
        if (program is null)
        {
            return NotFound();
        }

        program.Status = string.Equals(program.Status, "ACTIVE", StringComparison.OrdinalIgnoreCase)
            ? "INACTIVE"
            : "ACTIVE";
        program.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();
        TempData["SuccessMessage"] = "Đã cập nhật trạng thái chương trình đào tạo.";

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        return Redirect("/admin/programs");
    }

    private async Task ValidateFormAsync(TrainingProgramFormViewModel model, Guid? currentId)
    {
        if (!EducationTypes.Any(item => item.Value == model.EducationType))
        {
            ModelState.AddModelError(nameof(model.EducationType), "Hệ đào tạo không hợp lệ.");
        }

        if (!Statuses.Contains(model.Status, StringComparer.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(nameof(model.Status), "Trạng thái không hợp lệ.");
        }

        var duplicateCodeExists = await dbContext.TrainingPrograms
            .AnyAsync(program => program.ProgramCode == model.ProgramCode && program.Id != currentId);

        if (duplicateCodeExists)
        {
            ModelState.AddModelError(nameof(model.ProgramCode), "Mã chương trình đã tồn tại trong hệ thống.");
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

    private static void PopulateOptions(TrainingProgramFormViewModel model)
    {
        model.AvailableEducationTypes = EducationTypes;
        model.AvailableStatuses = Statuses;
    }

    private static void Normalize(TrainingProgramFormViewModel model)
    {
        model.ProgramCode = model.ProgramCode.Trim();
        model.ProgramName = model.ProgramName.Trim();
        model.EducationType = NormalizeEducationType(model.EducationType) ?? string.Empty;
        model.Description = NormalizeOptionalText(model.Description);
        model.DurationText = NormalizeOptionalText(model.DurationText);
        model.ManagingUnit = NormalizeOptionalText(model.ManagingUnit);
        model.Status = NormalizeStatus(model.Status) ?? "ACTIVE";
    }

    private static string? NormalizeEducationType(string? educationType)
    {
        return string.IsNullOrWhiteSpace(educationType) ? null : educationType.Trim().ToUpperInvariant();
    }

    private static string? NormalizeStatus(string? status)
    {
        return string.IsNullOrWhiteSpace(status) ? null : status.Trim().ToUpperInvariant();
    }

    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static string GetEducationTypeLabel(string educationType)
    {
        return EducationTypes.FirstOrDefault(item => item.Value == educationType)?.Label ?? educationType;
    }
}
