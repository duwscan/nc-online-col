# wnc/Data

## OVERVIEW

Database context, initialization, and seeding. Contains EF Core configuration and fixed seed IDs.

## FILES

- `AppDbContext.cs` — DbContext with 30+ DbSets and fluent configuration
- `DbInitializer.cs` — Database seeder (check if exists)

## DBCONTEXT STRUCTURE

### DbSets
All entities exposed as `DbSet<T>` properties.

### OnModelCreating Sections
1. `ConfigureIdentityAccess` — AppUser, Role, UserRole, PasswordResetToken, AuthLog
2. `ConfigureCandidateManagement` — Candidate, CandidateEducationProfile, CandidatePriorityProfile
3. `ConfigureAdmissionCatalog` — TrainingProgram, Major, AdmissionRound, AdmissionMethod, RoundProgram, RoundAdmissionMethod, DocumentType, RoundDocumentRequirement
4. `ConfigureApplicationProcessing` — AdmissionApplication, ApplicationPreference, ApplicationDocument, ApplicationStatusHistory, ApplicationReviewNote, ApplicationSupplementRequest, EnrollmentConfirmation
5. `ConfigureCommunicationAudit` — Notification, NotificationTemplate, AuditLog, SystemConfig

### Seed Data
`SeedLookupData()` seeds fixed GUIDs via `SeedIds` static class:
- 4 roles (Admin, AdmissionOfficer, ReportViewer, Candidate)
- 3 admission methods (HOC_BA, DIEM_THI, TUYEN_THANG)
- 3 document types (CCCD, HOC_BA, KHAI_SINH)
- 1 training program (CD_CHINH_QUY)
- 2 system configs (AUTH.LOGIN_BY_EMAIL, AUTH.LOGIN_BY_PHONE)

## CONVENTIONS

- **SeedIds**: Fixed GUIDs in `public static class SeedIds` at bottom of file
- **Seeded timestamp**: `new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)`
- **Check constraints**: Applied at table level via `HasCheckConstraint()`
- **Filtered indexes**: Unique indexes with `HasFilter()` for soft-delete patterns
