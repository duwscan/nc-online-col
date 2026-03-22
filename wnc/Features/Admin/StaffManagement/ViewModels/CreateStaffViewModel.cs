using System.ComponentModel.DataAnnotations;

namespace wnc.Features.Admin.StaffManagement.ViewModels;

public class CreateStaffViewModel
{
    [Required(ErrorMessage = "Username is required.")]
    [StringLength(100, ErrorMessage = "Username cannot exceed 100 characters.")]
    [Display(Name = "Username")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    [StringLength(255, ErrorMessage = "Email cannot exceed 255 characters.")]
    [Display(Name = "Email address")]
    public string Email { get; set; } = string.Empty;

    [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters.")]
    [Display(Name = "Phone number")]
    public string? PhoneNumber { get; set; }

    [Required(ErrorMessage = "Password is required.")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Status is required.")]
    [Display(Name = "Status")]
    public string Status { get; set; } = "ACTIVE";

    [Display(Name = "Assigned roles")]
    public List<string> SelectedRoleCodes { get; set; } = [];

    public List<StaffRoleOptionViewModel> AvailableRoles { get; set; } = [];

    public IReadOnlyList<string> AvailableStatuses { get; set; } = ["ACTIVE", "INACTIVE"];
}

public class StaffRoleOptionViewModel
{
    public Guid Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsSelected { get; set; }
}
