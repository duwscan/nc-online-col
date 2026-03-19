using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using wnc.Data;
using wnc.Models;

namespace wnc.Controllers;

public class AdmissionRoundController : Controller
{
    private readonly AppDbContext _context;

    public AdmissionRoundController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public IActionResult GetAdmissionRounds([FromQuery] AdmissionRoundSearchViewModel search)
    {
        var query = _context.AdmissionRounds
            .Include(ar => ar.CreatedByUser)
            .Include(ar => ar.RoundPrograms)
            .ThenInclude(rp => rp.Program)
            .Include(ar => ar.RoundPrograms)
            .ThenInclude(rp => rp.AdmissionApplications)
            .AsQueryable();

        if (!string.IsNullOrEmpty(search.SearchTerm))
        {
            var term = search.SearchTerm.ToLower();
            query = query.Where(ar =>
                ar.RoundCode.ToLower().Contains(term) ||
                ar.RoundName.ToLower().Contains(term) ||
                (ar.Notes != null && ar.Notes.ToLower().Contains(term)));
        }

        if (!string.IsNullOrEmpty(search.Status))
        {
            query = query.Where(ar => ar.Status == search.Status);
        }

        if (search.AdmissionYear.HasValue)
        {
            query = query.Where(ar => ar.AdmissionYear == search.AdmissionYear.Value);
        }

        var totalCount = query.Count();

        var rounds = query
            .OrderByDescending(ar => ar.AdmissionYear)
            .ThenByDescending(ar => ar.StartAt)
            .Skip((search.Page - 1) * search.PageSize)
            .Take(search.PageSize)
            .Select(ar => new AdmissionRoundViewModel
            {
                Id = ar.Id,
                RoundCode = ar.RoundCode,
                RoundName = ar.RoundName,
                AdmissionYear = ar.AdmissionYear,
                StartAt = ar.StartAt,
                EndAt = ar.EndAt,
                Status = ar.Status,
                Notes = ar.Notes,
                AllowEnrollmentConfirmation = ar.AllowEnrollmentConfirmation,
                CreatedAt = ar.CreatedAt,
                UpdatedAt = ar.UpdatedAt,
                CreatedByUserName = ar.CreatedByUser != null ? ar.CreatedByUser.Username : null,
                ProgramCount = ar.RoundPrograms.Count(),
                ApplicationCount = ar.RoundPrograms.SelectMany(rp => rp.AdmissionApplications).Count(),
                StatusText = GetStatusText(ar.Status, ar.StartAt, ar.EndAt),
                DateRangeText = ar.StartAt.ToString("dd/MM/yyyy") + " - " + ar.EndAt.ToString("dd/MM/yyyy")
            })
            .ToList();

        var result = new AdmissionRoundPagedResult
        {
            Items = rounds,
            TotalCount = totalCount,
            PageNumber = search.Page,
            PageSize = search.PageSize
        };

        return Json(result);
    }

    [HttpGet]
    public IActionResult GetById(Guid id)
    {
        var round = _context.AdmissionRounds
            .Include(ar => ar.CreatedByUser)
            .Include(ar => ar.RoundPrograms)
            .ThenInclude(rp => rp.Program)
            .FirstOrDefault(ar => ar.Id == id);

        if (round == null)
        {
            return Json(new { success = false, message = "Admission round not found" });
        }

        var vm = new EditAdmissionRoundViewModel
        {
            Id = round.Id,
            RoundCode = round.RoundCode,
            RoundName = round.RoundName,
            AdmissionYear = round.AdmissionYear,
            StartAt = round.StartAt,
            EndAt = round.EndAt,
            Status = round.Status,
            Notes = round.Notes,
            AllowEnrollmentConfirmation = round.AllowEnrollmentConfirmation
        };

        return Json(new { success = true, data = vm });
    }

    [HttpPost]
    public IActionResult Create([FromBody] CreateAdmissionRoundViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return Json(new { success = false, message = "Invalid data", errors = ModelState });
        }

        if (model.EndAt <= model.StartAt)
        {
            return Json(new { success = false, message = "End date must be after start date" });
        }

        if (_context.AdmissionRounds.Any(ar => ar.RoundCode == model.RoundCode))
        {
            return Json(new { success = false, message = "Round code already exists" });
        }

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
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.AdmissionRounds.Add(round);
        _context.SaveChanges();

        return Json(new { success = true, message = "Admission round created successfully", id = round.Id });
    }

    [HttpPost]
    public IActionResult Edit([FromBody] EditAdmissionRoundViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return Json(new { success = false, message = "Invalid data", errors = ModelState });
        }

        if (model.EndAt <= model.StartAt)
        {
            return Json(new { success = false, message = "End date must be after start date" });
        }

        var round = _context.AdmissionRounds.FirstOrDefault(ar => ar.Id == model.Id);

        if (round == null)
        {
            return Json(new { success = false, message = "Admission round not found" });
        }

        if (_context.AdmissionRounds.Any(ar => ar.RoundCode == model.RoundCode && ar.Id != model.Id))
        {
            return Json(new { success = false, message = "Round code already exists" });
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

        _context.SaveChanges();

        return Json(new { success = true, message = "Admission round updated successfully" });
    }

    [HttpPost]
    public IActionResult Delete(Guid id)
    {
        var round = _context.AdmissionRounds
            .Include(ar => ar.RoundPrograms)
            .ThenInclude(rp => rp.AdmissionApplications)
            .FirstOrDefault(ar => ar.Id == id);

        if (round == null)
        {
            return Json(new { success = false, message = "Admission round not found" });
        }

        if (round.RoundPrograms.Any(rp => rp.AdmissionApplications.Any()))
        {
            return Json(new { success = false, message = "Cannot delete round with existing applications" });
        }

        if (round.Status == "PUBLISHED")
        {
            return Json(new { success = false, message = "Cannot delete a published admission round" });
        }

        _context.AdmissionRounds.Remove(round);
        _context.SaveChanges();

        return Json(new { success = true, message = "Admission round deleted successfully" });
    }

    [HttpGet]
    public IActionResult GetStatuses()
    {
        var statuses = new[]
        {
            new { Value = "DRAFT", Text = "Nháp", Color = "secondary" },
            new { Value = "PUBLISHED", Text = "Đã đăng tải", Color = "success" },
            new { Value = "CLOSED", Text = "Đã đóng", Color = "danger" },
            new { Value = "ARCHIVED", Text = "Đã lưu trữ", Color = "dark" }
        };

        return Json(statuses);
    }

    [HttpGet]
    public IActionResult GetAdmissionYears()
    {
        var currentYear = DateTime.Now.Year;
        var years = Enumerable.Range(currentYear - 2, 5)
            .Select(y => new { Value = y, Text = y.ToString() })
            .ToList();

        return Json(years);
    }

    private static string GetStatusText(string status, DateTime startAt, DateTime endAt)
    {
        var now = DateTime.UtcNow;

        return status switch
        {
            "DRAFT" => "Nháp",
            "PUBLISHED" when now < startAt => "Sắp mở",
            "PUBLISHED" when now >= startAt && now <= endAt => "Đang mở",
            "PUBLISHED" when now > endAt => "Đã kết thúc",
            "CLOSED" => "Đã đóng",
            "ARCHIVED" => "Đã lưu trữ",
            _ => status
        };
    }
}
