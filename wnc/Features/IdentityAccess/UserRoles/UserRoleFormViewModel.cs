using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace wnc.Features.IdentityAccess.UserRoles;

public sealed class UserRoleFormViewModel
{
    [Display(Name = "User")]
    [Required]
    public Guid UserId { get; set; }

    [Display(Name = "Role")]
    [Required]
    public Guid RoleId { get; set; }

    [Display(Name = "Assigned by")]
    public Guid? AssignedBy { get; set; }

    [Display(Name = "Assigned at")]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    [Display(Name = "Revoked at")]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
    public DateTime? RevokedAt { get; set; }

    public IReadOnlyList<SelectListItem> UserOptions { get; set; } = [];
    public IReadOnlyList<SelectListItem> RoleOptions { get; set; } = [];
    public IReadOnlyList<SelectListItem> AssignedByOptions { get; set; } = [];
}
