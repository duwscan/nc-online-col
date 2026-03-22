using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using wnc.Data;
using wnc.Features.Admin.StaffManagement.ViewModels;
using wnc.Models;

namespace wnc.Features.Admin.StaffManagement.Controllers;

[Authorize(Roles = "ADMIN")]
public class StaffManagementController(AppDbContext dbContext) : Controller
{
    private static readonly string[] StaffRoleCodes = ["ADMIN", "ADMISSION_OFFICER", "REPORT_VIEWER"];
    private static readonly string[] StaffStatuses = ["ACTIVE", "INACTIVE"];

    [HttpGet("/admin/staff")]
    public async Task<IActionResult> Index(string? searchTerm = null, string? roleCode = null, string? status = null, int page = 1)
    {
        var normalizedSearchTerm = string.IsNullOrWhiteSpace(searchTerm) ? null : searchTerm.Trim();
        var normalizedRoleCode = NormalizeRoleCode(roleCode);
        var normalizedStatus = NormalizeStatus(status);
        var currentPage = page < 1 ? 1 : page;

        var availableRoles = await GetAvailableRoleOptionsAsync();

        var query = dbContext.Users
            .AsNoTracking()
            .Where(user => user.DeletedAt == null)
            .Where(user => user.UserRoles.Any(userRole =>
                userRole.RevokedAt == null &&
                StaffRoleCodes.Contains(userRole.Role.Code)));

        if (!string.IsNullOrWhiteSpace(normalizedSearchTerm))
        {
            query = query.Where(user =>
                (user.Username != null && user.Username.Contains(normalizedSearchTerm)) ||
                (user.Email != null && user.Email.Contains(normalizedSearchTerm)) ||
                (user.PhoneNumber != null && user.PhoneNumber.Contains(normalizedSearchTerm)));
        }

        if (!string.IsNullOrWhiteSpace(normalizedRoleCode))
        {
            query = query.Where(user => user.UserRoles.Any(userRole =>
                userRole.RevokedAt == null && userRole.Role.Code == normalizedRoleCode));
        }

        if (!string.IsNullOrWhiteSpace(normalizedStatus))
        {
            query = query.Where(user => user.Status == normalizedStatus);
        }

        const int pageSize = 10;
        var totalItems = await query.CountAsync();
        var pagedUsers = await query
            .Include(user => user.UserRoles.Where(userRole =>
                userRole.RevokedAt == null &&
                StaffRoleCodes.Contains(userRole.Role.Code)))
            .ThenInclude(userRole => userRole.Role)
            .OrderBy(user => user.Username ?? user.Email ?? user.PhoneNumber ?? user.Id.ToString())
            .Skip((currentPage - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var staffMembers = pagedUsers
            .Select(user => new StaffListItemViewModel
            {
                Id = user.Id,
                Username = user.Username ?? string.Empty,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber,
                RoleNames = user.UserRoles
                    .Where(userRole => userRole.RevokedAt == null && StaffRoleCodes.Contains(userRole.Role.Code))
                    .OrderBy(userRole => userRole.Role.Name)
                    .Select(userRole => userRole.Role.Name)
                    .ToList(),
                Status = user.Status,
                LastLoginAt = user.LastLoginAt
            })
            .ToList();

        var model = new StaffListViewModel
        {
            StaffMembers = staffMembers,
            AvailableRoles = availableRoles,
            SearchTerm = normalizedSearchTerm,
            RoleCode = normalizedRoleCode,
            Status = normalizedStatus,
            Page = currentPage,
            PageSize = pageSize,
            TotalItems = totalItems
        };

        return View("~/Features/Admin/StaffManagement/Views/Index.cshtml", model);
    }

    [HttpGet("/admin/staff/create")]
    public async Task<IActionResult> Create()
    {
        var model = new CreateStaffViewModel();
        await PopulateCreateOptionsAsync(model);
        return View("~/Features/Admin/StaffManagement/Views/Create.cshtml", model);
    }

    [HttpPost("/admin/staff/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateStaffViewModel model)
    {
        Normalize(model);
        await ValidateStaffInputAsync(model.Email, model.PhoneNumber, model.Status, null, model.SelectedRoleCodes);

        if (!ModelState.IsValid)
        {
            await PopulateCreateOptionsAsync(model);
            model.Password = string.Empty;
            return View("~/Features/Admin/StaffManagement/Views/Create.cshtml", model);
        }

        var roles = await GetSelectedRolesAsync(model.SelectedRoleCodes);
        if (roles.Count != model.SelectedRoleCodes.Distinct(StringComparer.OrdinalIgnoreCase).Count())
        {
            ModelState.AddModelError(nameof(model.SelectedRoleCodes), "One or more selected roles are invalid.");
            await PopulateCreateOptionsAsync(model);
            model.Password = string.Empty;
            return View("~/Features/Admin/StaffManagement/Views/Create.cshtml", model);
        }

        var userId = Guid.NewGuid();
        var currentUserId = GetCurrentUserId();
        var now = DateTime.UtcNow;

        var user = new AppUser
        {
            Id = userId,
            Username = model.Username,
            Email = model.Email,
            PhoneNumber = model.PhoneNumber,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
            Status = model.Status,
            CreatedAt = now,
            UpdatedAt = now
        };

        dbContext.Users.Add(user);

        foreach (var role in roles)
        {
            dbContext.UserRoles.Add(new UserRole
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                RoleId = role.Id,
                AssignedBy = currentUserId,
                AssignedAt = now,
                RevokedAt = null
            });
        }

        await dbContext.SaveChangesAsync();
        TempData["SuccessMessage"] = "Staff account created successfully.";

        return Redirect("/admin/staff");
    }

    [HttpGet("/admin/staff/edit/{id:guid}")]
    public async Task<IActionResult> Edit(Guid id)
    {
        var user = await dbContext.Users
            .AsNoTracking()
            .Include(appUser => appUser.UserRoles.Where(userRole =>
                userRole.RevokedAt == null &&
                StaffRoleCodes.Contains(userRole.Role.Code)))
            .ThenInclude(userRole => userRole.Role)
            .SingleOrDefaultAsync(appUser => appUser.Id == id && appUser.DeletedAt == null);

        if (user is null || !user.UserRoles.Any())
        {
            return NotFound();
        }

        var model = new EditStaffViewModel
        {
            Id = user.Id,
            Username = user.Username ?? string.Empty,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            Status = user.Status,
            SelectedRoleCodes = user.UserRoles
                .Select(userRole => userRole.Role.Code)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList()
        };

        await PopulateEditOptionsAsync(model);
        return View("~/Features/Admin/StaffManagement/Views/Edit.cshtml", model);
    }

    [HttpPost("/admin/staff/edit/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, EditStaffViewModel model)
    {
        if (id != model.Id)
        {
            return NotFound();
        }

        Normalize(model);
        await ValidateStaffInputAsync(model.Email, model.PhoneNumber, model.Status, id, model.SelectedRoleCodes);

        var user = await dbContext.Users
            .Include(appUser => appUser.UserRoles.Where(userRole =>
                StaffRoleCodes.Contains(userRole.Role.Code)))
            .ThenInclude(userRole => userRole.Role)
            .SingleOrDefaultAsync(appUser => appUser.Id == id && appUser.DeletedAt == null);

        if (user is null)
        {
            return NotFound();
        }

        if (!user.UserRoles.Any(userRole => userRole.RevokedAt == null && StaffRoleCodes.Contains(userRole.Role.Code)))
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            await PopulateEditOptionsAsync(model);
            model.Password = string.Empty;
            return View("~/Features/Admin/StaffManagement/Views/Edit.cshtml", model);
        }

        var selectedRoles = await GetSelectedRolesAsync(model.SelectedRoleCodes);
        if (selectedRoles.Count != model.SelectedRoleCodes.Distinct(StringComparer.OrdinalIgnoreCase).Count())
        {
            ModelState.AddModelError(nameof(model.SelectedRoleCodes), "One or more selected roles are invalid.");
            await PopulateEditOptionsAsync(model);
            model.Password = string.Empty;
            return View("~/Features/Admin/StaffManagement/Views/Edit.cshtml", model);
        }

        user.Username = model.Username;
        user.Email = model.Email;
        user.PhoneNumber = model.PhoneNumber;
        user.Status = model.Status;
        user.UpdatedAt = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(model.Password))
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);
        }

        var selectedRoleCodes = selectedRoles
            .Select(role => role.Code)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var activeUserRoles = user.UserRoles
            .Where(userRole => userRole.RevokedAt == null && StaffRoleCodes.Contains(userRole.Role.Code))
            .ToList();

        foreach (var existingUserRole in activeUserRoles.Where(userRole => !selectedRoleCodes.Contains(userRole.Role.Code)))
        {
            existingUserRole.RevokedAt = DateTime.UtcNow;
        }

        var activeRoleCodes = activeUserRoles
            .Where(userRole => userRole.RevokedAt == null)
            .Select(userRole => userRole.Role.Code)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var currentUserId = GetCurrentUserId();
        var assignedAt = DateTime.UtcNow;

        foreach (var role in selectedRoles.Where(role => !activeRoleCodes.Contains(role.Code)))
        {
            dbContext.UserRoles.Add(new UserRole
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                RoleId = role.Id,
                AssignedBy = currentUserId,
                AssignedAt = assignedAt,
                RevokedAt = null
            });
        }

        await dbContext.SaveChangesAsync();
        TempData["SuccessMessage"] = "Staff account updated successfully.";

        return Redirect("/admin/staff");
    }

    [HttpPost("/admin/staff/delete/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var user = await dbContext.Users
            .SingleOrDefaultAsync(appUser =>
                appUser.Id == id &&
                appUser.DeletedAt == null &&
                appUser.UserRoles.Any(userRole =>
                    userRole.RevokedAt == null &&
                    StaffRoleCodes.Contains(userRole.Role.Code)));

        if (user is null)
        {
            return NotFound();
        }

        user.DeletedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();
        TempData["SuccessMessage"] = "Staff account removed successfully.";

        return Redirect("/admin/staff");
    }

    private async Task<List<StaffFilterOptionViewModel>> GetAvailableRoleOptionsAsync()
    {
        return await dbContext.Roles
            .AsNoTracking()
            .Where(role => StaffRoleCodes.Contains(role.Code))
            .OrderBy(role => role.Name)
            .Select(role => new StaffFilterOptionViewModel
            {
                Code = role.Code,
                Name = role.Name
            })
            .ToListAsync();
    }

    private async Task PopulateCreateOptionsAsync(CreateStaffViewModel model)
    {
        model.AvailableStatuses = StaffStatuses;
        model.AvailableRoles = await GetRoleSelectionOptionsAsync(model.SelectedRoleCodes);
    }

    private async Task PopulateEditOptionsAsync(EditStaffViewModel model)
    {
        model.AvailableStatuses = StaffStatuses;
        model.AvailableRoles = await GetRoleSelectionOptionsAsync(model.SelectedRoleCodes);
    }

    private async Task<List<StaffRoleOptionViewModel>> GetRoleSelectionOptionsAsync(IEnumerable<string> selectedRoleCodes)
    {
        var selectedCodes = selectedRoleCodes
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .Select(code => code.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return await dbContext.Roles
            .AsNoTracking()
            .Where(role => StaffRoleCodes.Contains(role.Code))
            .OrderBy(role => role.Name)
            .Select(role => new StaffRoleOptionViewModel
            {
                Id = role.Id,
                Code = role.Code,
                Name = role.Name,
                Description = role.Description,
                IsSelected = selectedCodes.Contains(role.Code)
            })
            .ToListAsync();
    }

    private async Task<List<Role>> GetSelectedRolesAsync(IEnumerable<string> selectedRoleCodes)
    {
        var normalizedRoleCodes = selectedRoleCodes
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .Select(code => code.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return await dbContext.Roles
            .Where(role => normalizedRoleCodes.Contains(role.Code))
            .Where(role => StaffRoleCodes.Contains(role.Code))
            .ToListAsync();
    }

    private async Task ValidateStaffInputAsync(string email, string? phoneNumber, string? status, Guid? currentUserId, IReadOnlyCollection<string> selectedRoleCodes)
    {
        if (!StaffStatuses.Contains(NormalizeStatus(status) ?? string.Empty, StringComparer.OrdinalIgnoreCase))
        {
            ModelState.AddModelError("Status", "Please select a valid status.");
        }

        if (selectedRoleCodes.Count == 0)
        {
            ModelState.AddModelError("SelectedRoleCodes", "At least one role must be assigned.");
        }

        var duplicateEmailExists = await dbContext.Users
            .AnyAsync(user => user.Email == email && user.Id != currentUserId);

        if (duplicateEmailExists)
        {
            ModelState.AddModelError("Email", "This email address is already in use.");
        }

        if (!string.IsNullOrWhiteSpace(phoneNumber))
        {
            var duplicatePhoneExists = await dbContext.Users
                .AnyAsync(user => user.PhoneNumber == phoneNumber && user.Id != currentUserId);

            if (duplicatePhoneExists)
            {
                ModelState.AddModelError("PhoneNumber", "This phone number is already in use.");
            }
        }
    }

    private Guid? GetCurrentUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(userId, out var parsedUserId) ? parsedUserId : null;
    }

    private static string? NormalizeRoleCode(string? roleCode)
    {
        return string.IsNullOrWhiteSpace(roleCode) ? null : roleCode.Trim();
    }

    private static string? NormalizeStatus(string? status)
    {
        return string.IsNullOrWhiteSpace(status) ? null : status.Trim().ToUpperInvariant();
    }

    private static void Normalize(CreateStaffViewModel model)
    {
        model.Username = model.Username.Trim();
        model.Email = model.Email.Trim();
        model.PhoneNumber = string.IsNullOrWhiteSpace(model.PhoneNumber) ? null : model.PhoneNumber.Trim();
        model.Status = NormalizeStatus(model.Status) ?? "ACTIVE";
        model.SelectedRoleCodes = model.SelectedRoleCodes
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .Select(code => code.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static void Normalize(EditStaffViewModel model)
    {
        model.Username = model.Username.Trim();
        model.Email = model.Email.Trim();
        model.PhoneNumber = string.IsNullOrWhiteSpace(model.PhoneNumber) ? null : model.PhoneNumber.Trim();
        model.Password = string.IsNullOrWhiteSpace(model.Password) ? null : model.Password.Trim();
        model.Status = NormalizeStatus(model.Status) ?? "ACTIVE";
        model.SelectedRoleCodes = model.SelectedRoleCodes
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .Select(code => code.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
