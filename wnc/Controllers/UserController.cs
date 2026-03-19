using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using wnc.Data;
using wnc.Models;

namespace wnc.Controllers;

public class UserController : Controller
{
    private readonly AppDbContext _context;

    public UserController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public IActionResult GetUsers([FromQuery] UserSearchViewModel search)
    {
        var query = _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .Where(u => u.DeletedAt == null)
            .AsQueryable();

        if (!string.IsNullOrEmpty(search.SearchTerm))
        {
            var term = search.SearchTerm.ToLower();
            query = query.Where(u =>
                (u.Username != null && u.Username.ToLower().Contains(term)) ||
                (u.Email != null && u.Email.ToLower().Contains(term)) ||
                (u.PhoneNumber != null && u.PhoneNumber.Contains(term)));
        }

        if (!string.IsNullOrEmpty(search.Status))
        {
            query = query.Where(u => u.Status == search.Status);
        }

        if (!string.IsNullOrEmpty(search.Role))
        {
            query = query.Where(u => u.UserRoles.Any(ur =>
                ur.Role != null && ur.Role.Code == search.Role && ur.RevokedAt == null));
        }

        var totalCount = query.Count();

        var users = query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((search.Page - 1) * search.PageSize)
            .Take(search.PageSize)
            .Select(u => new UserViewModel
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
                Status = u.Status,
                EmailVerifiedAt = u.EmailVerifiedAt,
                PhoneVerifiedAt = u.PhoneVerifiedAt,
                LastLoginAt = u.LastLoginAt,
                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt,
                Roles = u.UserRoles
                    .Where(ur => ur.RevokedAt == null && ur.Role != null)
                    .Select(ur => ur.Role!.Name)
                    .ToList()
            })
            .ToList();

        var result = new UserPagedResult
        {
            Items = users,
            TotalCount = totalCount,
            PageNumber = search.Page,
            PageSize = search.PageSize
        };

        return Json(result);
    }

    [HttpGet]
    public IActionResult GetById(Guid id)
    {
        var user = _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefault(u => u.Id == id && u.DeletedAt == null);

        if (user == null)
        {
            return Json(new { success = false, message = "User not found" });
        }

        var vm = new EditUserViewModel
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Status = user.Status,
            SelectedRoles = user.UserRoles
                .Where(ur => ur.RevokedAt == null && ur.Role != null)
                .Select(ur => ur.Role!.Code)
                .ToList()
        };

        return Json(new { success = true, data = vm });
    }

    [HttpPost]
    public IActionResult Create([FromBody] CreateUserViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return Json(new { success = false, message = "Invalid data", errors = ModelState });
        }

        if (_context.Users.Any(u => u.Email == model.Email))
        {
            return Json(new { success = false, message = "Email already exists" });
        }

        if (!string.IsNullOrEmpty(model.PhoneNumber) &&
            _context.Users.Any(u => u.PhoneNumber == model.PhoneNumber))
        {
            return Json(new { success = false, message = "Phone number already exists" });
        }

        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            Username = model.Username,
            Email = model.Email,
            PhoneNumber = model.PhoneNumber,
            PasswordHash = HashPassword(model.Password!),
            Status = model.Status,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);

        foreach (var roleCode in model.SelectedRoles)
        {
            var role = _context.Roles.FirstOrDefault(r => r.Code == roleCode);
            if (role != null)
            {
                _context.UserRoles.Add(new UserRole
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    RoleId = role.Id,
                    AssignedAt = DateTime.UtcNow
                });
            }
        }

        _context.SaveChanges();

        return Json(new { success = true, message = "User created successfully", id = user.Id });
    }

    [HttpPost]
    public IActionResult Edit([FromBody] EditUserViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return Json(new { success = false, message = "Invalid data", errors = ModelState });
        }

        var user = _context.Users
            .Include(u => u.UserRoles)
            .FirstOrDefault(u => u.Id == model.Id && u.DeletedAt == null);

        if (user == null)
        {
            return Json(new { success = false, message = "User not found" });
        }

        if (_context.Users.Any(u => u.Email == model.Email && u.Id != model.Id))
        {
            return Json(new { success = false, message = "Email already exists" });
        }

        user.Username = model.Username;
        user.Email = model.Email;
        user.PhoneNumber = model.PhoneNumber;
        user.Status = model.Status;
        user.UpdatedAt = DateTime.UtcNow;

        var currentRoles = user.UserRoles.Where(ur => ur.RevokedAt == null).ToList();
        foreach (var ur in currentRoles)
        {
            ur.RevokedAt = DateTime.UtcNow;
        }

        foreach (var roleCode in model.SelectedRoles)
        {
            var role = _context.Roles.FirstOrDefault(r => r.Code == roleCode);
            if (role != null)
            {
                _context.UserRoles.Add(new UserRole
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    RoleId = role.Id,
                    AssignedAt = DateTime.UtcNow
                });
            }
        }

        _context.SaveChanges();

        return Json(new { success = true, message = "User updated successfully" });
    }

    [HttpPost]
    public IActionResult Delete(Guid id)
    {
        var user = _context.Users.FirstOrDefault(u => u.Id == id && u.DeletedAt == null);

        if (user == null)
        {
            return Json(new { success = false, message = "User not found" });
        }

        user.DeletedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;
        _context.SaveChanges();

        return Json(new { success = true, message = "User deleted successfully" });
    }

    [HttpGet]
    public IActionResult GetRoles()
    {
        var roles = _context.Roles
            .Where(r => true)
            .Select(r => new { r.Code, r.Name })
            .ToList();

        return Json(roles);
    }

    [HttpGet]
    public IActionResult GetStatuses()
    {
        var statuses = new[]
        {
            new { Value = "ACTIVE", Text = "Active" },
            new { Value = "INACTIVE", Text = "Inactive" },
            new { Value = "SUSPENDED", Text = "Suspended" }
        };

        return Json(statuses);
    }

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }
}
