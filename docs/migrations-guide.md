# EF Core Migrations Guide

This guide covers how to create and apply Entity Framework Core migrations for the WNC Admission Portal.

## Prerequisites

1. **.NET 10 SDK** installed
2. **Docker** running with the MSSQL container (`make db-up`)
3. **EF Core CLI tools** installed globally:

   ```bash
   dotnet tool install --global dotnet-ef
   ```

   > Already installed? Make sure it is up to date:
   > ```bash
   > dotnet tool update --global dotnet-ef
   > ```

4. **NuGet packages** (already referenced in `wnc.csproj`):
   - `Microsoft.EntityFrameworkCore.SqlServer`
   - `Microsoft.EntityFrameworkCore.Tools`
   - `Microsoft.EntityFrameworkCore.Design`

## Quick Reference

All commands below should be run from the **repository root** (`/Users/duwscan/RiderProjects/wnc`).

Because the project is located at `wnc/wnc.csproj`, you must explicitly specify `--project` and `--startup-project`.

### 1. Add a Migration

```bash
dotnet ef migrations add <MigrationName> \
  --project wnc/wnc.csproj \
  --startup-project wnc/wnc.csproj \
  --output-dir Migrations
```

**Example – initial migration:**

```bash
dotnet ef migrations add InitialCreate \
  --project wnc/wnc.csproj \
  --startup-project wnc/wnc.csproj \
  --output-dir Migrations
```

### 2. Apply Migrations to the Database

```bash
dotnet ef database update \
  --project wnc/wnc.csproj \
  --startup-project wnc/wnc.csproj
```

### 3. Remove the Last Migration (if not yet applied)

```bash
dotnet ef migrations remove \
  --project wnc/wnc.csproj \
  --startup-project wnc/wnc.csproj
```

### 4. Generate a SQL Script

```bash
dotnet ef migrations script \
  --project wnc/wnc.csproj \
  --startup-project wnc/wnc.csproj \
  --output migrations.sql
```

## Environment-Specific Databases

| Environment | Database name |
|-------------|---------------|
| Production  | `WncAdmissionDb`      |
| Development | `WncAdmissionDbDev`   |

The active connection string is selected automatically via `ASPNETCORE_ENVIRONMENT`:

```bash
# Development (default when running locally)
export ASPNETCORE_ENVIRONMENT=Development

# Production
export ASPNETCORE_ENVIRONMENT=Production
```

## Database Setup (Docker)

Start the MSSQL container before running any migration commands:

```bash
make db-up        # Start container
make db-logs      # View container logs
make db-down      # Stop container
make db-restart   # Restart container
make db-reset     # Recreate container (destroys data)
```

## Common Issues

| Error | Cause | Fix |
|-------|-------|-----|
| `A network-related or instance-specific error occurred` | MSSQL container is not running | Run `make db-up` |
| `Login failed for user 'sa'` | Wrong password or container not ready | Check `docker-compose.yml` env vars and wait a few seconds after `make db-up` |
| `Cannot find compilation library` | `--startup-project` not specified | Add `--startup-project wnc/wnc.csproj` |
| `The migration has already been applied` | Removing a migration that was already applied to the DB | Revert the DB first with `dotnet ef database update <PreviousMigrationName>` |
| `There is already an object named 'users' in the database.` | Running `InitialCreate` on a non-empty DB | Ensure the DB is empty or use `dotnet ef database drop` before re-creating |

## Typical Workflow

1. **Modify entities** in `wnc/Models/*.cs` or update `AppDbContext.cs` configuration.
2. **Create migration:**
   ```bash
   dotnet ef migrations add <DescriptiveName> \
     --project wnc/wnc.csproj \
     --startup-project wnc/wnc.csproj
   ```
3. **Review generated files** in `wnc/Migrations/`.
4. **Apply migration:**
   ```bash
   dotnet ef database update \
     --project wnc/wnc.csproj \
     --startup-project wnc/wnc.csproj
   ```
5. **Commit** the generated `.cs` and `.Designer.cs` files to version control.

## File Locations

- **DbContext:** `wnc/Data/AppDbContext.cs`
- **Migrations:** `wnc/Migrations/`
- **Connection strings:** `wnc/appsettings.json` & `wnc/appsettings.Development.json`
- **Docker compose:** `docker-compose.yml`
- **Makefile targets:** `Makefile`

## Additional Resources

- [EF Core Migrations Overview](https://learn.microsoft.com/ef/core/managing-schemas/migrations/)
- [EF Core CLI Reference](https://learn.microsoft.com/ef/core/cli/dotnet)
