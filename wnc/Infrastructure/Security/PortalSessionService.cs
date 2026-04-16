using System.Text.Json;
using wnc.Models;

namespace wnc.Infrastructure.Security;

public class PortalSessionService
{
    public const string SessionCookieName = "wnc.session";
    private const string RequestCountKey = "Session.RequestCount";
    private const string LastPathKey = "Session.LastPath";
    private const string LastSeenAtKey = "Session.LastSeenAtUtc";
    private const string PortalKey = "Session.Portal";
    private const string DisplayNameKey = "Session.DisplayName";
    private const string UserIdKey = "Session.UserId";
    private const string RolesKey = "Session.Roles";
    private const string AuthenticatedAtKey = "Session.AuthenticatedAtUtc";

    public void TrackRequest(HttpContext context)
    {
        var requestCount = context.Session.GetInt32(RequestCountKey) ?? 0;
        context.Session.SetInt32(RequestCountKey, requestCount + 1);
        context.Session.SetString(LastPathKey, context.Request.Path.Value ?? "/");
        context.Session.SetString(LastSeenAtKey, DateTime.UtcNow.ToString("O"));
    }

    public void StoreSignIn(HttpContext context, AppUser user, IReadOnlyCollection<string> roles, string portal)
    {
        context.Session.SetString(UserIdKey, user.Id.ToString());
        context.Session.SetString(PortalKey, portal);
        context.Session.SetString(DisplayNameKey, user.Username ?? user.Email ?? user.PhoneNumber ?? user.Id.ToString());
        context.Session.SetString(AuthenticatedAtKey, DateTime.UtcNow.ToString("O"));
        context.Session.SetString(RolesKey, JsonSerializer.Serialize(roles));
    }

    public void Clear(HttpContext context)
    {
        context.Session.Clear();
    }

    public SessionSnapshot GetSnapshot(HttpContext context)
    {
        var rolesJson = context.Session.GetString(RolesKey);
        var roles = string.IsNullOrWhiteSpace(rolesJson)
            ? []
            : JsonSerializer.Deserialize<string[]>(rolesJson) ?? [];

        return new SessionSnapshot
        {
            RequestCount = context.Session.GetInt32(RequestCountKey) ?? 0,
            Portal = context.Session.GetString(PortalKey),
            DisplayName = context.Session.GetString(DisplayNameKey),
            Roles = roles,
            LastPath = context.Session.GetString(LastPathKey),
            AuthenticatedAtUtc = TryParse(context.Session.GetString(AuthenticatedAtKey))
        };
    }

    private static DateTime? TryParse(string? value)
    {
        return DateTime.TryParse(value, out var parsed) ? parsed : null;
    }
}

public class SessionSnapshot
{
    public int RequestCount { get; init; }
    public string? Portal { get; init; }
    public string? DisplayName { get; init; }
    public string[] Roles { get; init; } = [];
    public DateTime? AuthenticatedAtUtc { get; init; }
    public string? LastPath { get; init; }
}
