using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace wnc.Features.IdentityAccess.Users;

public sealed class UserFormViewModel
{
    [Display(Name = "Username")]
    [MaxLength(100)]
    public string? Username { get; set; }

    [Display(Name = "Email")]
    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string? Email { get; set; }

    [Display(Name = "Phone number")]
    [Required]
    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    [Display(Name = "Password hash")]
    [Required]
    [MaxLength(255)]
    public string PasswordHash { get; set; } = string.Empty;

    [Display(Name = "Status")]
    [Required]
    [MaxLength(30)]
    public string Status { get; set; } = "ACTIVE";

    [Display(Name = "Email verified at")]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
    public DateTime? EmailVerifiedAt { get; set; }

    [Display(Name = "Phone verified at")]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
    public DateTime? PhoneVerifiedAt { get; set; }

    [Display(Name = "Last login at")]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
    public DateTime? LastLoginAt { get; set; }

    public IReadOnlyList<SelectListItem> StatusOptions { get; set; } = [];
}
