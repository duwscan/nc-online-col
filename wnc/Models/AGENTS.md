# wnc/Models

## OVERVIEW

28 domain entities for the admission portal. Flat structure with entity classes in root, `Entities/` subfolder empty.

## ENTITIES

### Identity & Access
- `AppUser` — User accounts (email, phone, password hash)
- `Role` — System roles (ADMIN, ADMISSION_OFFICER, REPORT_VIEWER, CANDIDATE)
- `UserRole` — User-role assignments with soft delete (`RevokedAt`)
- `PasswordResetToken` — Password recovery tokens
- `AuthLog` — Authentication audit trail

### Candidate Management
- `Candidate` — Candidate profile linked 1:1 to AppUser
- `CandidateEducationProfile` — Education history with GPA
- `CandidatePriorityProfile` — Priority scores

### Admission Catalog
- `TrainingProgram` — Programs with quota check constraint
- `Major` — Majors under programs, quota >= 0
- `AdmissionRound` — Rounds with time validation (StartAt < EndAt)
- `AdmissionMethod` — Methods (HOC_BA, DIEM_THI, TUYEN_THANG)
- `RoundProgram` — Program-round associations with published quota
- `RoundAdmissionMethod` — Method eligibility per round-program

### Document Management
- `DocumentType` — Document types (CCCD, HOC_BA, KHAI_SINH)
- `RoundDocumentRequirement` — Required docs per round-program

### Application Processing
- `AdmissionApplication` — Applications with unique submission number
- `ApplicationPreference` — Program/major/method preferences
- `ApplicationDocument` — Uploaded documents with file size
- `ApplicationStatusHistory` — Status change audit trail
- `ApplicationReviewNote` — Officer notes
- `ApplicationSupplementRequest` — Document supplement requests
- `EnrollmentConfirmation` — Post-admission enrollment

### Communication & Config
- `Notification` — User notifications
- `NotificationTemplate` — Notification templates
- `AuditLog` — General audit log
- `SystemConfig` — Key-value configuration

## CONVENTIONS

- **Primary key**: `Guid Id` on all entities
- **Audit fields**: `CreatedAt`, `UpdatedAt` with `GETUTCDATE()` default
- **Table naming**: Snake_case plural (`users`, `roles`, `user_roles`)
- **Unique indexes**: Business keys marked `.IsUnique()`
- **Foreign keys**: Always `DeleteBehavior.Restrict` except where cascade is intentional
