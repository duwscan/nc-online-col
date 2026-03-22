using System.ComponentModel.DataAnnotations;

namespace wnc.Features.Admin.StaffManagement.ViewModels;

public class EditStaffViewModel
{
    public Guid Id { get; set; }

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

    [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
    [DataType(DataType.Password)]
    [Display(Name = "New password")]
    public string? Password { get; set; }

    [Required(ErrorMessage = "Status is required.")]
    [Display(Name = "Status")]
    public string Status { get; set; } = "ACTIVE";

    [Display(Name = "Assigned roles")]
    public List<string> SelectedRoleCodes { get; set; } = [];

    public List<StaffRoleOptionViewModel> AvailableRoles { get; set; } = [];

    public IReadOnlyList<string> AvailableStatuses { get; set; } = ["ACTIVE", "INACTIVE"];
}
