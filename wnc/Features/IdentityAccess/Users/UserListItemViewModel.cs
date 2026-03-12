namespace wnc.Features.IdentityAccess.Users;

public sealed class UserListItemViewModel
{
    public Guid Id { get; init; }
    public string? Username { get; init; }
    public string? Email { get; init; }
    public string? PhoneNumber { get; init; }
    public string Status { get; init; } = string.Empty;
    public int ActiveRoleCount { get; init; }
    public DateTime? LastLoginAt { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? DeletedAt { get; init; }
}
