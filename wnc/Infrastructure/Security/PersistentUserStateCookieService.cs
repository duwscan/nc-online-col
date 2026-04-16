using System.Text;
using System.Text.Json;

namespace wnc.Infrastructure.Security;

public class PersistentUserStateCookieService
{
    public const string CookieName = "wnc.user-state";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public PersistentUserState ReadFromRequest(HttpRequest request)
    {
        if (!request.Cookies.TryGetValue(CookieName, out var rawValue) || string.IsNullOrWhiteSpace(rawValue))
        {
            return new PersistentUserState();
        }

        try
        {
            var json = Encoding.UTF8.GetString(Convert.FromBase64String(rawValue));
            return JsonSerializer.Deserialize<PersistentUserState>(json, JsonOptions) ?? new PersistentUserState();
        }
        catch
        {
            return new PersistentUserState();
        }
    }

    public void WriteToResponse(HttpResponse response, PersistentUserState state)
    {
        var featureHits = state.FeatureHits
            .OrderByDescending(x => x.Value)
            .ThenBy(x => x.Key, StringComparer.OrdinalIgnoreCase)
            .Take(8)
            .ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);

        var value = Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(state with
        {
            FeatureHits = featureHits
        }, JsonOptions)));

        response.Cookies.Append(CookieName, value, new CookieOptions
        {
            IsEssential = true,
            HttpOnly = true,
            SameSite = SameSiteMode.Lax,
            Secure = false,
            Expires = DateTimeOffset.UtcNow.AddDays(30)
        });
    }
}

public record PersistentUserState
{
    public int TotalVisits { get; init; }
    public DateTime? LastVisitedAtUtc { get; init; }
    public Dictionary<string, int> FeatureHits { get; init; } = new(StringComparer.OrdinalIgnoreCase);
}
