namespace wnc.Features.IdentityAccess.Roles;

public sealed class RoleListItemViewModel
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsSystemRole { get; init; }
    public int ActiveAssignmentCount { get; init; }
    public DateTime UpdatedAt { get; init; }
}
