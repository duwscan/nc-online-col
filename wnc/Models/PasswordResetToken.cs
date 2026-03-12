using System.ComponentModel.DataAnnotations;

namespace wnc.Models;

public class PasswordResetToken
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    [MaxLength(255)] public string Token { get; set; } = string.Empty;
    [MaxLength(30)] public string TokenType { get; set; } = "RESET_PASSWORD";
    public DateTime ExpiredAt { get; set; }
    public DateTime? UsedAt { get; set; }
    public DateTime CreatedAt { get; set; }

    public AppUser User { get; set; } = null!;
}
