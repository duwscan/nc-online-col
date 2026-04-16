using BCryptNet = BCrypt.Net.BCrypt;
using Microsoft.AspNetCore.Identity;
using wnc.Models;

namespace wnc.Infrastructure.Identity;

public class BcryptPasswordHasher : IPasswordHasher<AppUser>
{
    public string HashPassword(AppUser user, string password)
    {
        return BCryptNet.HashPassword(password);
    }

    public PasswordVerificationResult VerifyHashedPassword(AppUser user, string hashedPassword, string providedPassword)
    {
        if (string.IsNullOrWhiteSpace(hashedPassword))
        {
            return PasswordVerificationResult.Failed;
        }

        return BCryptNet.Verify(providedPassword, hashedPassword)
            ? PasswordVerificationResult.Success
            : PasswordVerificationResult.Failed;
    }
}
