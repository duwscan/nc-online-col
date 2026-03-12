namespace wnc.Features.IdentityAccess.Users;

public sealed class UserDetailsViewModel
{
    public Guid Id { get; init; }
    public string? Username { get; init; }
    public string? Email { get; init; }
    public string? PhoneNumber { get; init; }
    public string Status { get; init; } = string.Empty;
    public string PasswordHash { get; init; } = string.Empty;
    public DateTime? EmailVerifiedAt { get; init; }
    public DateTime? PhoneVerifiedAt { get; init; }
    public DateTime? LastLoginAt { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public DateTime? DeletedAt { get; init; }
    public IReadOnlyList<string> Roles { get; init; } = [];
    public IReadOnlyList<string> RecentAuthLogs { get; init; } = [];
}
