using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace wnc.Features.IdentityAccess.AuthLogs;

public sealed class AuthLogFormViewModel
{
    [Display(Name = "User")]
    public Guid? UserId { get; set; }

    [Display(Name = "Login identifier")]
    [Required]
    [MaxLength(255)]
    public string LoginIdentifier { get; set; } = string.Empty;

    [Display(Name = "Status")]
    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "SUCCESS";

    [Display(Name = "Failure reason")]
    [MaxLength(255)]
    public string? FailureReason { get; set; }

    [Display(Name = "IP address")]
    [MaxLength(64)]
    public string? IpAddress { get; set; }

    [Display(Name = "User agent")]
    [MaxLength(500)]
    public string? UserAgent { get; set; }

    [Display(Name = "Logged at")]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
    public DateTime LoggedAt { get; set; } = DateTime.UtcNow;

    public IReadOnlyList<SelectListItem> UserOptions { get; set; } = [];
    public IReadOnlyList<SelectListItem> StatusOptions { get; set; } = [];
}
