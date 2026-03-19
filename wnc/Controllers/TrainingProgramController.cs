using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using wnc.Data;
using wnc.Models;

namespace wnc.Controllers;

public class TrainingProgramController : Controller
{
    private readonly AppDbContext _context;

    public TrainingProgramController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public IActionResult GetTrainingPrograms([FromQuery] TrainingProgramSearchViewModel search)
    {
        var query = _context.TrainingPrograms
            .Include(tp => tp.Majors)
            .AsQueryable();

        if (!string.IsNullOrEmpty(search.SearchTerm))
        {
            var term = search.SearchTerm.ToLower();
            query = query.Where(tp =>
                tp.ProgramCode.ToLower().Contains(term) ||
                tp.ProgramName.ToLower().Contains(term) ||
                (tp.Description != null && tp.Description.ToLower().Contains(term)));
        }

        if (!string.IsNullOrEmpty(search.EducationType))
        {
            query = query.Where(tp => tp.EducationType == search.EducationType);
        }

        if (!string.IsNullOrEmpty(search.Status))
        {
            query = query.Where(tp => tp.Status == search.Status);
        }

        var totalCount = query.Count();

        var programs = query
            .OrderBy(tp => tp.DisplayOrder)
            .ThenBy(tp => tp.ProgramName)
            .Skip((search.Page - 1) * search.PageSize)
            .Take(search.PageSize)
            .Select(tp => new TrainingProgramViewModel
            {
                Id = tp.Id,
                ProgramCode = tp.ProgramCode,
                ProgramName = tp.ProgramName,
                EducationType = tp.EducationType,
                Description = tp.Description,
                TuitionFee = tp.TuitionFee,
                DurationText = tp.DurationText,
                Quota = tp.Quota,
                ManagingUnit = tp.ManagingUnit,
                Status = tp.Status,
                DisplayOrder = tp.DisplayOrder,
                CreatedAt = tp.CreatedAt,
                UpdatedAt = tp.UpdatedAt,
                MajorCount = tp.Majors.Count(m => m.Status == "ACTIVE")
            })
            .ToList();

        var result = new TrainingProgramPagedResult
        {
            Items = programs,
            TotalCount = totalCount,
            PageNumber = search.Page,
            PageSize = search.PageSize
        };

        return Json(result);
    }

    [HttpGet]
    public IActionResult GetById(Guid id)
    {
        var program = _context.TrainingPrograms
            .Include(tp => tp.Majors)
            .FirstOrDefault(tp => tp.Id == id);

        if (program == null)
        {
            return Json(new { success = false, message = "Training program not found" });
        }

        var vm = new EditTrainingProgramViewModel
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
            DisplayOrder = program.DisplayOrder
        };

        return Json(new { success = true, data = vm });
    }

    [HttpPost]
    public IActionResult Create([FromBody] CreateTrainingProgramViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return Json(new { success = false, message = "Invalid data", errors = ModelState });
        }

        if (_context.TrainingPrograms.Any(tp => tp.ProgramCode == model.ProgramCode))
        {
            return Json(new { success = false, message = "Program code already exists" });
        }

        var program = new TrainingProgram
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
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.TrainingPrograms.Add(program);
        _context.SaveChanges();

        return Json(new { success = true, message = "Training program created successfully", id = program.Id });
    }

    [HttpPost]
    public IActionResult Edit([FromBody] EditTrainingProgramViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return Json(new { success = false, message = "Invalid data", errors = ModelState });
        }

        var program = _context.TrainingPrograms.FirstOrDefault(tp => tp.Id == model.Id);

        if (program == null)
        {
            return Json(new { success = false, message = "Training program not found" });
        }

        if (_context.TrainingPrograms.Any(tp => tp.ProgramCode == model.ProgramCode && tp.Id != model.Id))
        {
            return Json(new { success = false, message = "Program code already exists" });
        }

        program.ProgramCode = model.ProgramCode;
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

        _context.SaveChanges();

        return Json(new { success = true, message = "Training program updated successfully" });
    }

    [HttpPost]
    public IActionResult Delete(Guid id)
    {
        var program = _context.TrainingPrograms
            .Include(tp => tp.RoundPrograms)
            .FirstOrDefault(tp => tp.Id == id);

        if (program == null)
        {
            return Json(new { success = false, message = "Training program not found" });
        }

        if (program.RoundPrograms.Any())
        {
            return Json(new { success = false, message = "Cannot delete program with existing admission rounds" });
        }

        _context.TrainingPrograms.Remove(program);
        _context.SaveChanges();

        return Json(new { success = true, message = "Training program deleted successfully" });
    }

    [HttpGet]
    public IActionResult GetEducationTypes()
    {
        var types = new[]
        {
            new { Value = "CAO_DANG", Text = "Cao đẳng" },
            new { Value = "TRUNG_CAP", Text = "Trung cấp" },
            new { Value = "LIEN_THONG", Text = "Liên thông" },
            new { Value = "VAN_BANG_2", Text = "Văn bằng 2" },
            new { Value = "TU_XA", Text = "Từ xa" }
        };

        return Json(types);
    }

    [HttpGet]
    public IActionResult GetStatuses()
    {
        var statuses = new[]
        {
            new { Value = "ACTIVE", Text = "Active" },
            new { Value = "INACTIVE", Text = "Inactive" },
            new { Value = "DRAFT", Text = "Draft" }
        };

        return Json(statuses);
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        var programs = _context.TrainingPrograms
            .Where(tp => tp.Status == "ACTIVE")
            .OrderBy(tp => tp.DisplayOrder)
            .ThenBy(tp => tp.ProgramName)
            .Select(tp => new { tp.Id, tp.ProgramCode, tp.ProgramName })
            .ToList();

        return Json(programs);
    }
}
