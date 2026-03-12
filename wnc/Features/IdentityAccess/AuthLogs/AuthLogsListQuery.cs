using System.ComponentModel.DataAnnotations;
using wnc.Infrastructure.Queries;

namespace wnc.Features.IdentityAccess.AuthLogs;

public sealed class AuthLogsListQuery : ListQueryParameters
{
    public Guid? UserId { get; set; }
    public string? Status { get; set; }

    [DataType(DataType.Date)]
    public DateTime? LoggedFrom { get; set; }

    [DataType(DataType.Date)]
    public DateTime? LoggedTo { get; set; }
}
