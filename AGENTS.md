# PROJECT KNOWLEDGE BASE

**Generated:** 2026-03-21
**Commit:** 1fa7df4
**Branch:** duwscan/develop

## OVERVIEW

ASP.NET Core MVC admission portal with separate student/admin authentication flows, Entity Framework Core + SQL Server, and vertical feature slices for Admin and Students modules.

## STRUCTURE

```
./
├── wnc.sln                    # VS solution
├── wnc/                       # Main project (nested double-folder)
│   ├── Program.cs             # Entry point + DI setup
│   ├── Controllers/           # HomeController only
│   ├── Features/              # Vertical slices
│   │   ├── Admin/             # Admin auth
│   │   └── Students/          # Student auth
│   ├── Models/                # 28 domain entities
│   ├── Data/                  # DbContext + seeder
│   ├── Views/                 # Razor views (Auth, Home, Shared)
│   ├── Migrations/            # EF migrations
│   └── wwwroot/               # Static assets
├── docker-compose.yml         # MSSQL 2022 container
├── Makefile                   # db-up/down/restart/logs
└── docs/                      # SRS, DB design, UI/UX
```

## WHERE TO LOOK

| Task | Location | Notes |
|------|----------|-------|
| Auth logic | `Features/{Admin,Students}/Authentication/` | Controllers + ViewModels |
| Auth views | `Views/Auth/{Admin,Student}/` | Role-specific login pages |
| DB schema | `Models/*.cs` + `Data/AppDbContext.cs` | 28 entities, EF config |
| Entry point | `Program.cs` | Cookie auth, role routing |
| UI/UX rules | `docs/ui-ux-system-design.md` | Design principles |
| DB design | `database-design.md` | ERD documentation |

## CONVENTIONS

- **Vertical slices**: Feature folders (`Features/Admin/`, `Features/Students/`) organize by domain, not layer
- **Primary key**: `Guid Id` on all entities
- **Audit fields**: `CreatedAt`, `UpdatedAt` with `GETUTCDATE()` default
- **Foreign keys**: `OnDelete(DeleteBehavior.Restrict)` unless cascade is intentional
- **Unique indexes**: `.HasIndex().IsUnique()` for business keys
- **Check constraints**: DB-level constraints (e.g., `ck_training_programs_quota`)
- **Soft patterns**: `RevokedAt` nullable for user_roles
- **Auth events**: `CookieAuthenticationEvents` for redirect logic
- **Roles**: `ADMIN`, `ADMISSION_OFFICER`, `REPORT_VIEWER`, `CANDIDATE`

## ANTI-PATTERNS (THIS PROJECT)

- **Student login**: Students must NOT use admin login (`Views/Auth/Admin/Login.cshtml`)
- **Staff login**: Staff must NOT use student login (`Views/Auth/Student/Login.cshtml`)
- **No consent inference**: Do NOT preselect options or infer consent (`Views/Auth/Student/Login.cshtml`)
- **No stack traces in prod**: Error pages must not expose internal data (`Views/Shared/Error.cshtml`)
- **No DB cascades on User**: User deletion restricted to protect Candidate data

## UNIQUE STYLES

- **Nested project folder**: `wnc/wnc/` (not flat)
- **db-compose for dev**: MSSQL runs in Docker, managed via `make db-*`
- **Seed data in DbContext**: Lookup data seeded via `OnModelCreating` with fixed `SeedIds` GUIDs
- **Feature-based Controllers**: Under `Features/{Feature}/Authentication/Controllers/`
- **Login path routing**: Cookie events detect `/admin` prefix to redirect to correct login

## COMMANDS

```bash
# Database (requires Docker)
make db-up          # Start MSSQL container
make db-down        # Stop container
make db-restart     # Restart container
make db-logs        # View container logs
make db-reset       # Recreate container

# .NET
dotnet build        # Build project
dotnet run          # Run dev server
dotnet ef migrations add <Name>   # Add migration
dotnet ef database update          # Apply migrations
```

## NOTES

- .NET 10.0 (pre-release), EF Core 10.0, SQL Server
- No test project (testing not set up)
- No CI/CD workflows (dev-focused)
- Auth: Cookie-based, 8-hour sliding expiration
- Login accepts email OR phone number
