namespace wnc.Features.IdentityAccess.AuthLogs;

public sealed class AuthLogDetailsViewModel
{
    public Guid Id { get; init; }
    public Guid? UserId { get; init; }
    public string? UserDisplay { get; init; }
    public string LoginIdentifier { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string? FailureReason { get; init; }
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }
    public DateTime LoggedAt { get; init; }
}
