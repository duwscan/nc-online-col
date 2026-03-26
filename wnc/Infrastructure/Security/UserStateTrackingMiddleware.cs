namespace wnc.Infrastructure.Security;

public class UserStateTrackingMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(
        HttpContext context,
        PersistentUserStateCookieService persistentCookieService,
        PortalSessionService portalSessionService)
    {
        if (ShouldTrack(context.Request.Path))
        {
            var persistentState = persistentCookieService.ReadFromRequest(context.Request);
            var featureKey = ResolveFeatureKey(context);
            var updatedHits = new Dictionary<string, int>(persistentState.FeatureHits, StringComparer.OrdinalIgnoreCase);

            if (!string.IsNullOrWhiteSpace(featureKey))
            {
                updatedHits[featureKey] = updatedHits.TryGetValue(featureKey, out var count) ? count + 1 : 1;
            }

            var updatedState = persistentState with
            {
                TotalVisits = persistentState.TotalVisits + 1,
                LastVisitedAtUtc = DateTime.UtcNow,
                FeatureHits = updatedHits
            };

            portalSessionService.TrackRequest(context);

            context.Response.OnStarting(() =>
            {
                persistentCookieService.WriteToResponse(context.Response, updatedState);
                return Task.CompletedTask;
            });
        }

        await next(context);
    }

    private static bool ShouldTrack(PathString path)
    {
        if (!path.HasValue)
        {
            return false;
        }

        var value = path.Value!;
        return !value.StartsWith("/css", StringComparison.OrdinalIgnoreCase)
               && !value.StartsWith("/js", StringComparison.OrdinalIgnoreCase)
               && !value.StartsWith("/lib", StringComparison.OrdinalIgnoreCase)
               && !value.StartsWith("/favicon", StringComparison.OrdinalIgnoreCase)
               && !value.Contains('.', StringComparison.OrdinalIgnoreCase);
    }

    private static string ResolveFeatureKey(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        if (endpoint is null)
        {
            return context.Request.Path.Value?.Trim('/').Replace('/', ':') ?? "home";
        }

        return endpoint.DisplayName ?? context.Request.Path.Value ?? "home";
    }
}
