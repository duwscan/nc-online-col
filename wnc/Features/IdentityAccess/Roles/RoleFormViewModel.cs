using System.ComponentModel.DataAnnotations;

namespace wnc.Features.IdentityAccess.Roles;

public sealed class RoleFormViewModel
{
    [Display(Name = "Code")]
    [Required]
    [MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    [Display(Name = "Name")]
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Description")]
    public string? Description { get; set; }

    [Display(Name = "System role")]
    public bool IsSystemRole { get; set; } = true;
}
