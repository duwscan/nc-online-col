using System.ComponentModel.DataAnnotations;

namespace wnc.Models;

public class UserRole
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    public Guid? AssignedBy { get; set; }
    public DateTime AssignedAt { get; set; }
    public DateTime? RevokedAt { get; set; }

    public AppUser User { get; set; } = null!;
    public Role Role { get; set; } = null!;
    public AppUser? AssignedByUser { get; set; }
}
