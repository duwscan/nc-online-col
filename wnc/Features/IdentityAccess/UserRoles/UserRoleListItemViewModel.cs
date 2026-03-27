namespace wnc.Features.IdentityAccess.UserRoles;

public sealed class UserRoleListItemViewModel
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public string UserDisplay { get; init; } = string.Empty;
    public Guid RoleId { get; init; }
    public string RoleDisplay { get; init; } = string.Empty;
    public string? AssignedByDisplay { get; init; }
    public DateTime AssignedAt { get; init; }
    public DateTime? RevokedAt { get; init; }
}
