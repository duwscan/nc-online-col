using Microsoft.EntityFrameworkCore;
using wnc.Data;
using wnc.Features.IdentityAccess.Common;
using wnc.Infrastructure.Queries;

namespace wnc.Features.IdentityAccess.AuthLogs;

public sealed class AuthLogsListQueryService(AppDbContext dbContext) : IListQueryService<AuthLogsListQuery, AuthLogListItemViewModel>
{
    private static readonly SortMap<Models.AuthLog> SortMap = new SortMap<Models.AuthLog>()
        .Add("user", authLog => authLog.User != null ? authLog.User.Username ?? authLog.User.Email ?? string.Empty : string.Empty)
        .Add("loginIdentifier", authLog => authLog.LoginIdentifier)
        .Add("status", authLog => authLog.Status)
        .Add("ipAddress", authLog => authLog.IpAddress ?? string.Empty)
        .Add("loggedAt", authLog => authLog.LoggedAt);

    public async Task<PagedResult<AuthLogListItemViewModel>> ExecuteAsync(AuthLogsListQuery query, CancellationToken cancellationToken = default)
    {
        var authLogs = dbContext.AuthLogs
            .AsNoTracking()
            .Include(authLog => authLog.User)
            .ApplyFilterIf(query.UserId.HasValue, source => source.Where(authLog => authLog.UserId == query.UserId))
            .ApplyFilterIf(!string.IsNullOrWhiteSpace(query.Status), source => source.Where(authLog => authLog.Status == query.Status))
            .ApplyFilterIf(query.LoggedFrom.HasValue, source => source.Where(authLog => authLog.LoggedAt >= query.LoggedFrom!.Value.Date))
            .ApplyFilterIf(query.LoggedTo.HasValue, source => source.Where(authLog => authLog.LoggedAt < query.LoggedTo!.Value.Date.AddDays(1)))
            .ApplySearch(query.SearchTerm, (source, term) =>
            {
                var pattern = $"%{term}%";
                return source.Where(authLog =>
                    EF.Functions.Like(authLog.LoginIdentifier, pattern) ||
                    EF.Functions.Like(authLog.Status, pattern) ||
                    EF.Functions.Like(authLog.FailureReason ?? string.Empty, pattern) ||
                    EF.Functions.Like(authLog.IpAddress ?? string.Empty, pattern) ||
                    EF.Functions.Like(authLog.UserAgent ?? string.Empty, pattern) ||
                    EF.Functions.Like(authLog.User != null ? authLog.User.Username ?? string.Empty : string.Empty, pattern) ||
                    EF.Functions.Like(authLog.User != null ? authLog.User.Email ?? string.Empty : string.Empty, pattern));
            })
            .ApplySorting(query, SortMap, "loggedAt");

        var projection = authLogs.Select(authLog => new AuthLogListItemViewModel
        {
            Id = authLog.Id,
            UserId = authLog.UserId,
            UserDisplay = authLog.User != null
                ? authLog.User.Username ?? authLog.User.Email ?? authLog.User.PhoneNumber
                : null,
            LoginIdentifier = authLog.LoginIdentifier,
            Status = authLog.Status,
            FailureReason = authLog.FailureReason,
            IpAddress = authLog.IpAddress,
            LoggedAt = authLog.LoggedAt
        });

        return await projection.ToPagedResultAsync(query, cancellationToken);
    }
}
