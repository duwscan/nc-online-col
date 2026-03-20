using BCryptNet = BCrypt.Net.BCrypt;
using Microsoft.EntityFrameworkCore;
using wnc.Models;

namespace wnc.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await context.Database.MigrateAsync();
        await EnsureLookupDataAsync(context);
        await EnsureDemoUsersAsync(context);
    }

    private static async Task EnsureLookupDataAsync(AppDbContext context)
    {
        var seededAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var roles = new[]
        {
            new Role
            {
                Id = SeedIds.AdminRoleId,
                Code = "ADMIN",
                Name = "Administrator",
                Description = "System administrator",
                IsSystemRole = true,
                CreatedAt = seededAt,
                UpdatedAt = seededAt
            },
            new Role
            {
                Id = SeedIds.AdmissionOfficerRoleId,
                Code = "ADMISSION_OFFICER",
                Name = "Admission Officer",
                Description = "Admission operations staff",
                IsSystemRole = true,
                CreatedAt = seededAt,
                UpdatedAt = seededAt
            },
            new Role
            {
                Id = SeedIds.ReportViewerRoleId,
                Code = "REPORT_VIEWER",
                Name = "Report Viewer",
                Description = "Read-only reporting role",
                IsSystemRole = true,
                CreatedAt = seededAt,
                UpdatedAt = seededAt
            },
            new Role
            {
                Id = SeedIds.CandidateRoleId,
                Code = "CANDIDATE",
                Name = "Candidate",
                Description = "Candidate account role",
                IsSystemRole = true,
                CreatedAt = seededAt,
                UpdatedAt = seededAt
            }
        };

        foreach (var role in roles)
        {
            if (!await context.Roles.AnyAsync(x => x.Code == role.Code))
            {
                context.Roles.Add(role);
            }
        }

        var configs = new[]
        {
            new SystemConfig
            {
                Id = SeedIds.LoginByEmailConfigId,
                ConfigKey = "AUTH.LOGIN_BY_EMAIL",
                ConfigValue = "true",
                Description = "Allow login by email",
                UpdatedAt = seededAt
            },
            new SystemConfig
            {
                Id = SeedIds.LoginByPhoneConfigId,
                ConfigKey = "AUTH.LOGIN_BY_PHONE",
                ConfigValue = "true",
                Description = "Allow login by phone",
                UpdatedAt = seededAt
            }
        };

        foreach (var config in configs)
        {
            if (!await context.SystemConfigs.AnyAsync(x => x.ConfigKey == config.ConfigKey))
            {
                context.SystemConfigs.Add(config);
            }
        }

        await context.SaveChangesAsync();
    }

    private static async Task EnsureDemoUsersAsync(AppDbContext context)
    {
        const string demoPassword = "Admin@123";
        var now = DateTime.UtcNow;
        var adminRole = await context.Roles.SingleAsync(x => x.Code == "ADMIN");
        var candidateRole = await context.Roles.SingleAsync(x => x.Code == "CANDIDATE");

        var adminUser = await context.Users
            .Include(x => x.UserRoles)
            .SingleOrDefaultAsync(x => x.Email == "admin@wnc.local");

        if (adminUser is null)
        {
            adminUser = new AppUser
            {
                Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1"),
                Username = "admin.demo",
                Email = "admin@wnc.local",
                PhoneNumber = "0900000001",
                PasswordHash = BCryptNet.HashPassword(demoPassword),
                Status = "ACTIVE",
                EmailVerifiedAt = now,
                PhoneVerifiedAt = now,
                CreatedAt = now,
                UpdatedAt = now
            };

            context.Users.Add(adminUser);
            await context.SaveChangesAsync();
        }

        if (!adminUser.UserRoles.Any(x => x.RoleId == adminRole.Id && x.RevokedAt == null))
        {
            context.UserRoles.Add(new UserRole
            {
                Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb1"),
                UserId = adminUser.Id,
                RoleId = adminRole.Id,
                AssignedAt = now
            });
        }

        var candidateUser = await context.Users
            .Include(x => x.UserRoles)
            .SingleOrDefaultAsync(x => x.Email == "candidate@wnc.local");

        if (candidateUser is null)
        {
            candidateUser = new AppUser
            {
                Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2"),
                Username = "candidate.demo",
                Email = "candidate@wnc.local",
                PhoneNumber = "0900000002",
                PasswordHash = BCryptNet.HashPassword(demoPassword),
                Status = "ACTIVE",
                EmailVerifiedAt = now,
                PhoneVerifiedAt = now,
                CreatedAt = now,
                UpdatedAt = now
            };

            context.Users.Add(candidateUser);
            await context.SaveChangesAsync();
        }

        if (!candidateUser.UserRoles.Any(x => x.RoleId == candidateRole.Id && x.RevokedAt == null))
        {
            context.UserRoles.Add(new UserRole
            {
                Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb2"),
                UserId = candidateUser.Id,
                RoleId = candidateRole.Id,
                AssignedAt = now
            });
        }

        var candidateProfile = await context.Candidates.SingleOrDefaultAsync(x => x.UserId == candidateUser.Id);
        if (candidateProfile is null)
        {
            context.Candidates.Add(new Candidate
            {
                Id = Guid.Parse("cccccccc-cccc-cccc-cccc-ccccccccccc1"),
                UserId = candidateUser.Id,
                FullName = "Candidate Demo",
                DateOfBirth = new DateOnly(2007, 1, 15),
                Gender = "FEMALE",
                NationalId = "079204000001",
                Email = candidateUser.Email ?? string.Empty,
                PhoneNumber = candidateUser.PhoneNumber ?? string.Empty,
                AddressLine = "1 Tran Hung Dao",
                District = "District 1",
                ProvinceCode = "79",
                CreatedAt = now,
                UpdatedAt = now
            });
        }

        await context.SaveChangesAsync();
    }
}
