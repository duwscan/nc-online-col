# Complete Student Application Submission and Status Panel

## TL;DR
> **Summary**: Complete the student-side admission flow so a candidate can submit one read-only application per active round program — with a required multi-step document upload wizard — and later view the current status plus full status timeline.
> **Deliverables**:
> - Student application submission flow from program detail (multi-step wizard: create draft → upload documents → confirm)
> - Required-document enforcement (block confirmation until all required docs are uploaded)
> - PDF-only uploads, max 5MB per file, multiple files per document type per `RoundDocumentRequirement.MaxFiles`
> - Student-owned applications list page
> - Read-only application detail page with full status timeline and uploaded document list
> - Duplicate-submit and inactive-round guard behavior
> **Effort**: Medium-High
> **Parallel**: YES - 2 waves
> **Critical Path**: 1 → 2 → 3a/3b → 3c → 4 → 5 → 6 → 7

## Context
### Original Request
Complete the Student panel so students can submit to rounds with every program in that round and check submission status.

### Interview Summary
- Submission model: separate submission per program within a round.
- Submission UX: **multi-step wizard** — (1) create draft → (2) upload required documents → (3) confirm submission.
- Post-submit behavior: read-only; no edit, withdraw, or resubmit.
- Status UX: show full status timeline.
- Testing: do not add automated test infrastructure; use `dotnet build` plus agent-executed QA.
- **Document upload decisions**:
  - Upload timing: multi-step wizard; confirmation is blocked until all required documents are uploaded.
  - File constraints: PDF only, 5MB max per file.
  - Multi-file: allowed per `RoundDocumentRequirement.MaxFiles` (1–N files per document type).
  - Storage: local file system (`wwwroot/uploads/{applicationId}/{documentTypeId}/{filename}`).
  - Post-upload editing: not allowed after submission confirmation; replacement uploads allowed before confirm.

### Metis Review (gaps addressed)
- Resolved duplicate-submit behavior: if the candidate already has an application for the same `RoundProgram`, redirect to the existing detail page with an informational state.
- Resolved initial persistence behavior: draft application created with `CurrentStatus = "DRAFT"`. On confirm: `CurrentStatus = "SUBMITTED"`, `SubmissionNumber = 1`, `SubmittedAt = DateTime.UtcNow`, plus one `ApplicationStatusHistory` row with `FromStatus = null` and `ToStatus = "SUBMITTED"`.
- Resolved ownership/security behavior: every applications query must scope to the authenticated candidate; foreign application IDs return `NotFound()` to avoid information leakage.
- Resolved v1 scope for related models: do not create `ApplicationPreference`, notification, or admin-side side effects. `ApplicationDocument` is created by the upload flow only.
- Resolved document upload scope: upload flow creates `ApplicationDocument` records but submission confirmation requires all `IsRequired = true` documents to be present. No document replacement after SUBMITTED.

## Work Objectives
### Core Objective
Deliver a complete student-facing application flow using existing MVC and EF Core patterns so candidates can submit to active round programs with required document uploads and inspect their application status history without any lifecycle editing capability.

### Deliverables
- New student applications feature slice under `wnc/Features/Students/Applications/`
- Three-step wizard: (1) create draft → (2) upload documents → (3) confirm submission
- PDF-only file upload handling with local storage at `wwwroot/uploads/`
- Required-document gate: confirmation blocked until all required documents are uploaded
- `/student/applications` list page scoped to the authenticated candidate
- `/student/applications/{id:guid}` read-only detail page with timeline and document list
- Stable UI hooks (`data-testid`) for agent-executed QA

### Definition of Done (verifiable conditions with commands)
- `dotnet build` succeeds from `/Users/duwscan/RiderProjects/wnc/wnc`.
- A candidate can complete the full wizard: create draft → upload required PDFs → confirm submission.
- Confirmation is blocked until all required documents are uploaded (per `RoundProgram.DocumentRequirements` where `IsRequired = true`).
- PDF files only (`.pdf` extension, `application/pdf` MIME); non-PDF uploads are rejected with an error message.
- Files larger than 5MB are rejected with an error message.
- A second wizard start for the same candidate + `RoundProgram` redirects to the existing draft or submitted application detail.
- `/student/applications` shows only that candidate's applications, sorted newest-first.
- `/student/applications/{id}` shows read-only summary, document list, and full status timeline without exposing `InternalNote`.
- Inactive or closed programs cannot be newly submitted.
- Another candidate's application ID cannot be viewed from the current candidate account.

### Must Have
- Follow existing student MVC patterns and `_StudentLayout` navigation.
- Use one `AdmissionApplication` row per `CandidateId + RoundProgramId`.
- Generate unique `ApplicationCode` as `APP-{yyyyMMddHHmmss}-{8 uppercase hex chars}`.
- Set `SubmissionNumber = 1` for every new submission in this scope.
- Wizard Step 1 (create-draft): creates `AdmissionApplication` with `CurrentStatus = "DRAFT"`.
- Wizard Step 2 (upload): stores files under `wwwroot/uploads/{applicationId}/{documentTypeId}/`; creates `ApplicationDocument` rows.
- Wizard Step 3 (confirm): transitions `DRAFT` → `SUBMITTED`, sets `SubmittedAt`, creates initial `ApplicationStatusHistory`.
- Confirm button is disabled/hidden until all required documents (`IsRequired = true`) have at least one uploaded file.
- Multiple files per document type allowed up to `MaxFiles` limit.
- Show timeline entries in chronological order (oldest to newest) on the detail page.
- Static file serving configured for `wwwroot/uploads/`.

### Must NOT Have (guardrails, AI slop patterns, scope boundaries)
- No edit, withdraw, cancel, or delete functionality after submission.
- No document replacement or re-upload after submission is confirmed.
- No non-PDF file uploads.
- No files exceeding 5MB.
- No `ApplicationPreference`, notification, payment, or admin workflow changes.
- No schema redesign or new test project setup.
- No generic workflow abstraction/service framework unless strictly required by compile errors.

## Verification Strategy
> ZERO HUMAN INTERVENTION — all verification is agent-executed.
- Test decision: none for automated project tests; use `dotnet build` + Playwright/manual HTTP validation patterns.
- QA policy: Every task includes agent-executed happy-path and failure-path scenarios.
- Deterministic QA credentials: seeded student account `candidate@wnc.local` / `Admin@123` from `wnc/Data/DbInitializer.cs:105-203`.
- Deterministic active targets on 2026-03-27: `RoundProgramCnttId = aaaaaaa5-1111-1111-1111-111111111111`, `RoundProgramKeToanId = aaaaaaa6-1111-1111-1111-111111111111`, both active in `DOT1-2026` per `wnc/Data/AppDbContext.cs:640-706, 845-850`.
- DB reset/fixture policy for QA: use `docker exec nc-online-col-mssql /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "12062003An@" -d wnc -Q "..."` as defined by `docker-compose.yml:2-33`.
- Deterministic cleanup fixture for demo candidate `cccccccc-cccc-cccc-cccc-ccccccccccc1`:
  ```bash
  docker exec nc-online-col-mssql /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "12062003An@" -d wnc -Q "DELETE FROM application_status_histories WHERE ApplicationId IN (SELECT Id FROM admission_applications WHERE CandidateId='cccccccc-cccc-cccc-cccc-ccccccccccc1'); DELETE FROM application_documents WHERE ApplicationId IN (SELECT Id FROM admission_applications WHERE CandidateId='cccccccc-cccc-cccc-cccc-ccccccccccc1'); DELETE FROM admission_applications WHERE CandidateId='cccccccc-cccc-cccc-cccc-ccccccccccc1';"
  ```
- Deterministic foreign-app fixture for ownership QA:
  ```bash
  docker exec nc-online-col-mssql /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "12062003An@" -d wnc -Q "IF NOT EXISTS (SELECT 1 FROM users WHERE Id='dddddddd-dddd-dddd-dddd-dddddddddd11') INSERT INTO users (Id, Username, Email, PhoneNumber, PasswordHash, Status, CreatedAt, UpdatedAt) VALUES ('dddddddd-dddd-dddd-dddd-dddddddddd11','foreign.candidate','foreign-candidate@wnc.local','0900000099','fixture-hash','ACTIVE',GETUTCDATE(),GETUTCDATE()); IF NOT EXISTS (SELECT 1 FROM candidates WHERE Id='dddddddd-dddd-dddd-dddd-dddddddddd01') INSERT INTO candidates (Id, UserId, FullName, DateOfBirth, Gender, NationalId, Email, PhoneNumber, CreatedAt, UpdatedAt) VALUES ('dddddddd-dddd-dddd-dddd-dddddddddd01','dddddddd-dddd-dddd-dddd-dddddddddd11','Foreign Candidate', '2007-02-01','MALE','079204000099','foreign-candidate@wnc.local','0900000099',GETUTCDATE(),GETUTCDATE()); IF NOT EXISTS (SELECT 1 FROM admission_applications WHERE Id='eeeeeeee-eeee-eeee-eeee-eeeeeeeeeee1') INSERT INTO admission_applications (Id, ApplicationCode, CandidateId, RoundProgramId, CurrentStatus, SubmissionNumber, SubmittedAt, CreatedAt, UpdatedAt) VALUES ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeeee1','APP-FOREIGN-0001','dddddddd-dddd-dddd-dddd-dddddddddd01','aaaaaaa5-1111-1111-1111-111111111111','SUBMITTED',1,GETUTCDATE(),GETUTCDATE(),GETUTCDATE()); IF NOT EXISTS (SELECT 1 FROM application_status_histories WHERE Id='ffffffff-ffff-ffff-ffff-fffffffffff1') INSERT INTO application_status_histories (Id, ApplicationId, FromStatus, ToStatus, ChangedAt, PublicNote) VALUES ('ffffffff-ffff-ffff-ffff-fffffffffff1','eeeeeeee-eeee-eeee-eeee-eeeeeeeeeee1',NULL,'SUBMITTED',GETUTCDATE(),'Foreign fixture submission');"
  ```
- Deterministic document-requirements fixture for QA (no RoundDocumentRequirement data is seeded in the codebase — must be inserted before Tasks 3c and 4):
  ```bash
  docker exec nc-online-col-mssql /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "12062003An@" -d wnc -Q "IF NOT EXISTS (SELECT 1 FROM round_document_requirements WHERE RoundProgramId='aaaaaaa5-1111-1111-1111-111111111111' AND DocumentTypeId='dddddddd-4444-4444-4444-444444444444') INSERT INTO round_document_requirements (Id, RoundProgramId, DocumentTypeId, IsRequired, RequiresNotarization, RequiresOriginalCopy, MaxFiles, Notes, CreatedAt) VALUES (NEWID(),'aaaaaaa5-1111-1111-1111-111111111111','dddddddd-4444-4444-4444-444444444444',1,0,0,1,NULL,GETUTCDATE()); IF NOT EXISTS (SELECT 1 FROM round_document_requirements WHERE RoundProgramId='aaaaaaa5-1111-1111-1111-111111111111' AND DocumentTypeId='eeeeeeee-5555-5555-5555-555555555555') INSERT INTO round_document_requirements (Id, RoundProgramId, DocumentTypeId, IsRequired, RequiresNotarization, RequiresOriginalCopy, MaxFiles, Notes, CreatedAt) VALUES (NEWID(),'aaaaaaa5-1111-1111-1111-111111111111','eeeeeeee-5555-5555-5555-555555555555',1,0,0,1,NULL,GETUTCDATE());"
  ```
- Evidence: `.sisyphus/evidence/task-{N}-{slug}.{ext}`

## Execution Strategy
### Parallel Execution Waves
> Target: 5-8 tasks per wave. <3 per wave (except final) = under-splitting.
> Extract shared dependencies as Wave-1 tasks for max parallelism.

Wave 1: feature scaffolding, candidate ownership helpers, draft creation, document upload endpoint, static file serving

Wave 2: wizard upload page, confirmation endpoint, program detail integration, applications list, application detail/timeline, guard-state polish

### Dependency Matrix (full, all tasks)
- 1 blocks 2, 3a, 3b, 4, 5, 6, 7
- 2 blocks 3a, 3b, 5, 6
- 3a blocks 3c, 4
- 3b blocks 3c, 4
- 3c blocks 4
- 4 blocks none
- 5 blocks 6
- 6 blocks final verification only
- 7 blocks 4 (static serving needed for upload page)

### Agent Dispatch Summary (wave → task count → categories)
- Wave 1 → 4 tasks → `unspecified-high`
- Wave 2 → 3 tasks → `visual-engineering`

## TODOs
> Implementation + Test = ONE task. Never separate.
> EVERY task MUST have: Agent Profile + Parallelization + QA Scenarios.

- [x] 1. Scaffold the student applications feature slice and route contracts

  **What to do**: Create `wnc/Features/Students/Applications/Controllers/StudentApplicationsController.cs` plus any required view models under `wnc/Features/Students/Applications/ViewModels/`. Define all wizard routes in the same controller:
  - `GET /student/applications` — list page
  - `GET /student/applications/new/{roundProgramId:guid}` — wizard Step 2 (document upload page, draft created on GET)
  - `GET /student/applications/{id:guid}` — detail/status page
  - `POST /student/applications/{id:guid}/upload` — handle PDF file upload
  - `POST /student/applications/{id:guid}/confirm` — confirm and submit
  - `DELETE /student/applications/{id:guid}/documents/{documentId:guid}` — remove an uploaded file before confirm
  Create placeholder Razor views: `~/Views/Student/Applications/Index.cshtml`, `~/Views/Student/Applications/Documents.cshtml`, `~/Views/Student/Applications/Detail.cshtml`. Use the same constructor-injected `AppDbContext` pattern and `[Authorize(Roles = "CANDIDATE")]` guard used by existing student controllers.
  **Must NOT do**: Do not add new service layers, repositories, migrations, or placeholder routes outside the student applications slice. Do not implement upload logic in this task.

  **Recommended Agent Profile**:
  - Category: `unspecified-high` — Reason: multi-file MVC slice scaffolding with route contracts.
  - Skills: [`dotnet-best-practices`] — Reason: keeps ASP.NET MVC structure and controller conventions aligned.
  - Omitted: [`ef-core`] — Reason: no schema/config changes in this task.

  **Parallelization**: Can Parallel: NO | Wave 1 | Blocks: 2, 3a, 3b, 4, 5, 6, 7 | Blocked By: none

  **References** (executor has NO interview context — be exhaustive):
  - Pattern: `wnc/Features/Students/Rounds/Controllers/StudentRoundsController.cs:15-40` — existing student controller route + `AppDbContext` injection pattern.
  - Pattern: `wnc/Features/Students/Profile/Controllers/StudentProfileController.cs:11-75` — authenticated candidate controller structure with GET/POST actions in one controller.
  - Layout: `wnc/Views/Shared/_StudentLayout.cshtml:30-44` — existing student nav already links to `/student/applications`.
  - API/Type: `wnc/Models/AdmissionApplication.cs:7-29` — `CurrentStatus = "DRAFT"` and `CurrentStatus = "SUBMITTED"` usage.
  - API/Type: `wnc/Models/ApplicationDocument.cs:7-22` — upload target model.

  **Acceptance Criteria** (agent-executable only):
  - [ ] `dotnet build` succeeds from `wnc/` after adding the new controller, view models, and Razor views.
  - [ ] Route attributes exist for all six routes listed above.
  - [ ] All new views use `~/Views/Shared/_StudentLayout.cshtml`.

  **QA Scenarios** (MANDATORY — task incomplete without these):
  ```
  Scenario: Route contracts compile cleanly
    Tool: Bash
    Steps: Run `dotnet build` in `/Users/duwscan/RiderProjects/wnc/wnc`.
    Expected: Build exits 0 with no missing-controller, missing-view, or route-binding errors.
    Evidence: .sisyphus/evidence/task-1-applications-slice-build.txt

  Scenario: Unauthenticated requests remain protected
    Tool: Playwright
    Steps: Open `/student/applications`, `/student/applications/new/aaaaaaa5-1111-1111-1111-111111111111`, and `/student/applications/00000000-0000-0000-0000-000000000000` without signing in.
    Expected: Each request redirects to the student login flow rather than rendering application data.
    Evidence: .sisyphus/evidence/task-1-applications-slice-auth.png
  ```

  **Commit**: YES | Message: `feat(student-applications): scaffold student application routes and contracts` | Files: `wnc/Features/Students/Applications/**`, `wnc/Views/Student/Applications/**`

- [x] 2. Implement candidate resolution and ownership-scoped application queries

  **What to do**: Add private helper/query logic inside `StudentApplicationsController` to resolve the authenticated `Candidate` from `ClaimTypes.NameIdentifier`, mirroring the existing student profile flow. Centralize the base query for student-owned applications so all wizard steps and list/detail routes always filter by the current candidate. Foreign application IDs must return `NotFound()`, not `Forbid()`. Sort list queries by `SubmittedAt desc`, then `CreatedAt desc`. Also add a helper to check whether a candidate already has any application (draft or submitted) for a given `RoundProgramId`.
  **Must NOT do**: Do not duplicate candidate lookup logic across multiple action methods or allow any application lookup by raw `Id` without candidate scoping.

  **Recommended Agent Profile**:
  - Category: `unspecified-high` — Reason: security-sensitive query shaping and ownership enforcement.
  - Skills: [`dotnet-best-practices`, `ef-core`] — Reason: covers claims-based access and EF query composition.
  - Omitted: [`fluentui-blazor`] — Reason: not relevant to MVC Razor/Tailwind views.

  **Parallelization**: Can Parallel: NO | Wave 1 | Blocks: 3a, 3b, 5, 6 | Blocked By: 1

  **References** (executor has NO interview context — be exhaustive):
  - Pattern: `wnc/Features/Students/Profile/Controllers/StudentProfileController.cs:19-25` — existing `ClaimTypes.NameIdentifier` → candidate lookup pattern.
  - Pattern: `wnc/Models/Candidate.cs:7-27` — `Candidate.Id` and `UserId` relationship used for scoping.
  - API/Type: `wnc/Models/AdmissionApplication.cs:7-29` — candidate/application ownership fields.
  - API/Type: `wnc/Data/AppDbContext.cs:280-297` — unique `CandidateId + RoundProgramId` index and application relationship mapping.

  **Acceptance Criteria** (agent-executable only):
  - [ ] Every list/detail/wizard query in `StudentApplicationsController` scopes by the authenticated candidate before materialization.
  - [ ] Foreign application IDs return `NotFound()`.
  - [ ] Application list ordering is newest submission first using `SubmittedAt desc`, then `CreatedAt desc`.

  **QA Scenarios** (MANDATORY — task incomplete without these):
  ```
  Scenario: Student only sees own applications
    Tool: Playwright
    Steps: Run SQL cleanup to delete demo-candidate applications; sign in as `candidate@wnc.local` / `Admin@123`; complete wizard for `aaaaaaa5-1111-1111-1111-111111111111` (draft + upload + confirm); complete wizard for `aaaaaaa6-1111-1111-1111-111111111111`; open `/student/applications`; capture rendered cards with `data-testid="applications-list"`.
    Expected: Exactly two cards render for the demo candidate and both map to the submitted CNTT/KeToan round programs; no unrelated cards appear.
    Evidence: .sisyphus/evidence/task-2-ownership-list.png

  Scenario: Foreign application detail is hidden
    Tool: Playwright
    Steps: Run SQL fixture to insert foreign user `foreign-candidate@wnc.local`, candidate `dddddddd-dddd-dddd-dddd-dddddddddd01`, application `eeeeeeee-eeee-eeee-eeee-eeeeeeeeeee1` for `RoundProgramCnttId`; sign in as `candidate@wnc.local` / `Admin@123`; navigate directly to `/student/applications/eeeeeeee-eeee-eeee-eeee-eeeeeeeeeee1`.
    Expected: Server returns Not Found page or equivalent 404 response; no foreign application data is rendered.
    Evidence: .sisyphus/evidence/task-2-ownership-404.png
  ```

  **Commit**: YES | Message: `feat(student-applications): enforce candidate-scoped application access` | Files: `wnc/Features/Students/Applications/Controllers/StudentApplicationsController.cs`, `wnc/Features/Students/Applications/ViewModels/**`

- [x] 3a. Implement draft creation endpoint for wizard Step 1

  **What to do**: Implement `GET /student/applications/new/{roundProgramId:guid}` to handle wizard entry. It must: resolve the current candidate, check if a DRAFT or SUBMITTED application already exists for this candidate + `RoundProgramId` (if exists → redirect to existing detail with `?source=existing`), load the target `RoundProgram` with its `Round`, `DocumentRequirements` (including `DocumentType`), and verify the round is currently active (`StartAt <= UtcNow <= EndAt`), `Status = "PUBLISHED"`, and `RoundProgram.Status = "ACTIVE"`. If the round is not active or an application exists, render a guard state page (inactive-round notice or existing-application redirect). If eligible and no prior application, create a new `AdmissionApplication` with `CurrentStatus = "DRAFT"`, `ApplicationCode = APP-{yyyyMMddHHmmss}-{8 uppercase hex chars}`, `SubmissionNumber = 0`, and redirect to the documents page. Also update `wnc/Views/Student/ProgramDetail.cshtml` to replace the inert "Đăng ký ngay" button with an anchor link pointing to `/student/applications/new/{roundProgramId}` for active, submittable programs; replace the submit CTA with a "view status" link if the candidate already has an application for that round program; and remove the inert button for inactive programs. Add `data-testid="wizard-entry-link"` to the new anchor and `data-testid="view-application-status-link"` to the status link.
  **Must NOT do**: Do not create SUBMITTED applications in this step. Do not create `ApplicationStatusHistory` in this step. Do not add modal confirmations or JavaScript-only submission logic.

  **Recommended Agent Profile**:
  - Category: `unspecified-high` — Reason: write-path logic with domain guardrails and eligibility checks.
  - Skills: [`dotnet-best-practices`, `ef-core`] — Reason: C# controller flow and EF persistence behavior.
  - Omitted: [`ui-ux-pro-max`] — Reason: this task is back-end/controller-centric.

  **Parallelization**: Can Parallel: YES (with 3b) | Wave 1 | Blocks: 3c, 4 | Blocked By: 1, 2

  **References** (executor has NO interview context — be exhaustive):
  - Pattern: `wnc/Views/Student/ProgramDetail.cshtml:119-127` — current active-round CTA location that will target this wizard entry.
  - API/Type: `wnc/Models/AdmissionApplication.cs:7-29` — `DRAFT` status and `ApplicationCode` generation fields.
  - API/Type: `wnc/Models/RoundProgram.cs:7-22` — `RoundProgram`/`Round`/`DocumentRequirements` relation root.
  - API/Type: `wnc/Models/AdmissionRound.cs:7-23` — round time window and publication status inputs.
  - API/Type: `wnc/Models/RoundDocumentRequirement.cs:5-18` — `IsRequired`, `MaxFiles`, document type linkage.
  - API/Type: `wnc/Data/AppDbContext.cs:280-297` — application uniqueness, relationship mapping.

  **Acceptance Criteria** (agent-executable only):
  - [ ] Navigating to `/student/applications/new/{validActiveRoundProgramId}` creates a DRAFT application and redirects to the documents wizard page.
  - [ ] Navigating to `/student/applications/new/{alreadySubmittedRoundProgramId}` redirects to the existing detail page with `?source=existing`.
  - [ ] Navigating to `/student/applications/new/{inactiveRoundProgramId}` renders a guard page (not an error) without creating any application row.

  **QA Scenarios** (MANDATORY — task incomplete without these):
  ```
  Scenario: Wizard creates draft application for active round program
    Tool: Playwright
    Steps: Run SQL cleanup deleting demo-candidate applications; sign in as `candidate@wnc.local` / `Admin@123`; navigate to `/student/applications/new/aaaaaaa5-1111-1111-1111-111111111111`.
    Expected: Redirect lands on `/student/applications/{newDraftId}/documents`; DRAFT application row exists in DB with `CurrentStatus = "DRAFT"` and `CandidateId = cccccccc-cccc-cccc-cccc-ccccccccccc1`.
    Evidence: .sisyphus/evidence/task-3a-draft-created.png

  Scenario: Wizard redirects for existing application
    Tool: Playwright
    Steps: Run SQL cleanup; sign in as `candidate@wnc.local` / `Admin@123`; complete full wizard for `aaaaaaa5-1111-1111-1111-111111111111`; navigate again to `/student/applications/new/aaaaaaa5-1111-1111-1111-111111111111`.
    Expected: Redirect lands on `/student/applications/{existingId}?source=existing` instead of creating a second application.
    Evidence: .sisyphus/evidence/task-3a-existing-redirect.png
  ```

  **Commit**: YES | Message: `feat(student-applications): implement wizard draft creation` | Files: `wnc/Features/Students/Applications/Controllers/StudentApplicationsController.cs`

- [x] 3b. Implement document upload endpoint and local file storage service

  **What to do**: Implement the upload infrastructure: `POST /student/applications/{id:guid}/upload` handles `IFormFile` PDF uploads, validates `.pdf` extension and `application/pdf` MIME, enforces 5MB size limit, generates a unique stored filename, saves to `wwwroot/uploads/{applicationId}/{documentTypeId}/{uniqueFilename}.pdf`, and creates an `ApplicationDocument` row with `FileName` (original name), `StoragePath`, `MimeType`, `FileSize`, `UploadedAt`, `UploadedBy` (current user), `IsLatest = true`, `ValidationStatus = "PENDING"`. Also implement `DELETE /student/applications/{id:guid}/documents/{documentId:guid}` to delete the file from disk and remove the `ApplicationDocument` row (only allowed for DRAFT applications, candidate-scoped). Reject uploads for non-DRAFT applications with `Forbid` or `BadRequest`.
  **Must NOT do**: Do not expose `InternalNote` in any response. Do not allow upload to SUBMITTED applications. Do not save non-PDF files.

  **Recommended Agent Profile**:
  - Category: `unspecified-high` — Reason: file I/O, security validation, and persistence logic.
  - Skills: [`dotnet-best-practices`, `ef-core`] — Reason: C# controller/file handling and EF persistence.
  - Omitted: [`ui-ux-pro-max`] — Reason: this task is back-end/service-centric.

  **Parallelization**: Can Parallel: YES (with 3a) | Wave 1 | Blocks: 3c, 4 | Blocked By: 1, 2

  **References** (executor has NO interview context — be exhaustive):
  - API/Type: `wnc/Models/ApplicationDocument.cs:7-22` — upload target model with all fields to populate.
  - API/Type: `wnc/Data/AppDbContext.cs:323-341` — `ApplicationDocument` configuration and `UploadedBy` relationship.
  - Pattern: `wnc/Program.cs:1-92` — `WebApplication.CreateBuilder` and `app.MapStaticAssets()` for static file serving context.
  - File storage: `wwwroot/uploads/` — create directory structure at runtime if not exists using `Directory.CreateDirectory`.

  **Acceptance Criteria** (agent-executable only):
  - [ ] A valid PDF ≤ 5MB uploaded to `POST /student/applications/{draftId}/upload` saves the file to `wwwroot/uploads/{draftId}/{documentTypeId}/{filename}.pdf` and creates an `ApplicationDocument` row.
  - [ ] A PDF > 5MB is rejected with HTTP 400 and no file or DB record is created.
  - [ ] A non-PDF file is rejected with HTTP 400 and no file or DB record is created.
  - [ ] `DELETE /student/applications/{draftId}/documents/{documentId}` removes the file from disk and the DB row.
  - [ ] Upload or delete on a non-DRAFT application returns HTTP 400/403.

  **QA Scenarios** (MANDATORY — task incomplete without these):
  ```
  Scenario: PDF upload creates file and DB record
    Tool: Playwright + Bash
    Steps: Run SQL cleanup; sign in as `candidate@wnc.local` / `Admin@123`; create draft via `/student/applications/new/aaaaaaa5-1111-1111-1111-111111111111`; POST a 1MB PDF to `/student/applications/{draftId}/upload` with `documentTypeId` param.
    Expected: File exists at `wwwroot/uploads/{draftId}/{documentTypeId}/*.pdf`; `ApplicationDocument` row exists in DB with matching `FileName`, `FileSize`, `MimeType = "application/pdf"`.
    Evidence: .sisyphus/evidence/task-3b-upload-success.png + `ls wwwroot/uploads/`

  Scenario: Oversized file is rejected
    Tool: Bash
    Steps: Create draft; attempt to POST a 10MB PDF to `/student/applications/{draftId}/upload`.
    Expected: HTTP 400 response; no new file on disk; no new `ApplicationDocument` row.
    Evidence: .sisyphus/evidence/task-3b-oversized-reject.txt

  Scenario: Non-PDF file is rejected
    Tool: Bash
    Steps: Create draft; attempt to POST a DOCX file to `/student/applications/{draftId}/upload`.
    Expected: HTTP 400 response; no file on disk; no new `ApplicationDocument` row.
    Evidence: .sisyphus/evidence/task-3b-nonpdf-reject.txt

  Scenario: Delete removes file and DB row
    Tool: Bash
    Steps: Upload a file; then DELETE `/student/applications/{draftId}/documents/{documentId}`.
    Expected: File no longer exists on disk; `ApplicationDocument` row deleted from DB.
    Evidence: .sisyphus/evidence/task-3b-delete-doc.txt
  ```

  **Commit**: YES | Message: `feat(student-applications): add document upload endpoint and local file storage` | Files: `wnc/Features/Students/Applications/Controllers/StudentApplicationsController.cs`, `wnc/Features/Students/Applications/Services/DocumentStorageService.cs` (create if needed; simple static helper class is acceptable)

- [x] 7. Configure static file serving for uploaded documents

  **What to do**: Update `wnc/Program.cs` to serve files from `wwwroot/uploads/` as static files so uploaded PDFs can be downloaded or previewed. Add `app.UseStaticFiles(new StaticFileOptions { FileProvider = new PhysicalFileProvider(Path.Combine(builder.Environment.ContentRootPath, "wwwroot", "uploads")) })` before the existing `app.MapControllerRoute`. Ensure the uploads directory is excluded from source control via `.gitignore`. If `wwwroot/uploads` does not exist, create it on app startup.
  **Must NOT do**: Do not serve files outside `wwwroot/uploads/`. Do not expose any other static directories beyond what exists.

  **Recommended Agent Profile**:
  - Category: `unspecified-high` — Reason: Program.cs configuration and static file routing.
  - Skills: [`dotnet-best-practices`] — Reason: ASP.NET Core static file middleware and content root path usage.
  - Omitted: [`ef-core`, `ui-ux-pro-max`] — Reason: infrastructure/config only.

  **Parallelization**: Can Parallel: YES | Wave 1 | Blocks: 4 | Blocked By: 1, 3b

  **References** (executor has NO interview context — be exhaustive):
  - Pattern: `wnc/Program.cs:77-89` — existing middleware ordering and `app.UseStaticFiles()` placement.
  - Pattern: `wnc/Program.cs:84` — `app.MapStaticAssets()` as reference for ASP.NET static file registration.

  **Acceptance Criteria** (agent-executable only):
  - [ ] After upload, a PDF stored at `wwwroot/uploads/{applicationId}/{documentTypeId}/file.pdf` is accessible via `/uploads/{applicationId}/{documentTypeId}/file.pdf`.
  - [ ] `wwwroot/uploads` is listed in `.gitignore`.
  - [ ] `dotnet build` succeeds after Program.cs changes.

  **QA Scenarios** (MANDATORY — task incomplete without these):
  ```
  Scenario: Uploaded PDF is accessible via HTTP
    Tool: Bash
    Steps: Upload a PDF to a draft; extract the stored path from the DB; run `curl -I http://localhost:5000/uploads/{applicationId}/{documentTypeId}/{filename}.pdf` (or appropriate port from docker-compose).
    Expected: HTTP 200 with `Content-Type: application/pdf`.
    Evidence: .sisyphus/evidence/task-7-static-serving.png

  Scenario: Uploads directory is gitignored
    Tool: Bash
    Steps: Run `grep "uploads" /Users/duwscan/RiderProjects/wnc/.gitignore`.
    Expected: Output contains `uploads` or `wwwroot/uploads`.
    Evidence: .sisyphus/evidence/task-7-gitignore.txt
  ```

  **Commit**: YES | Message: `feat(student-applications): serve uploaded files as static content` | Files: `wnc/Program.cs`, `.gitignore`

- [x] 3c. Implement submission confirmation endpoint (wizard Step 3)

  **What to do**: Implement `POST /student/applications/{id:guid}/confirm`. Validate that: the application belongs to the current candidate, the application is in `DRAFT` status, all required documents (`RoundDocumentRequirement.IsRequired = true`) have at least one uploaded `ApplicationDocument`. If any required document is missing, return HTTP 400 with a list of missing required document names. If all required docs are present, transition: set `CurrentStatus = "SUBMITTED"`, `SubmissionNumber = 1`, `SubmittedAt = DateTime.UtcNow`, then in the same `SaveChangesAsync` call create one `ApplicationStatusHistory` row with `FromStatus = "DRAFT"`, `ToStatus = "SUBMITTED"`, `ChangedAt = DateTime.UtcNow`. Redirect to the detail page on success.
  **Must NOT do**: Do not allow confirm on already-submitted applications. Do not create multiple history rows on confirm.

  **Recommended Agent Profile**:
  - Category: `unspecified-high` — Reason: write-path transition with validation gate.
  - Skills: [`dotnet-best-practices`, `ef-core`] — Reason: EF save-changes atomicity and status transition logic.
  - Omitted: [`ui-ux-pro-max`] — Reason: back-end/controller logic.

  **Parallelization**: Can Parallel: NO | Wave 2 | Blocks: 4 | Blocked By: 1, 2, 3a, 3b

  **References** (executor has NO interview context — be exhaustive):
  - API/Type: `wnc/Models/AdmissionApplication.cs:7-29` — `DRAFT` → `SUBMITTED` transition fields.
  - API/Type: `wnc/Models/ApplicationStatusHistory.cs:7-18` — `FromStatus = "DRAFT"`, `ToStatus = "SUBMITTED"` initial row.
  - API/Type: `wnc/Models/RoundDocumentRequirement.cs:5-18` — `IsRequired` gate logic.
  - API/Type: `wnc/Data/AppDbContext.cs:343-357` — history relationship and cascade rules.

  **Acceptance Criteria** (agent-executable only):
  - [ ] Confirm on a DRAFT application with all required docs → transitions to `SUBMITTED`, creates exactly one history row.
  - [ ] Confirm on a DRAFT application with missing required docs → HTTP 400, no state change.
  - [ ] Confirm on an already-SUBMITTED application → HTTP 400/403, no duplicate history row.

  **QA Scenarios** (MANDATORY — task incomplete without these):
  ```
  Scenario: Confirm succeeds with all required docs
    Tool: Playwright
    Steps: Run SQL cleanup; run document-requirements fixture (see Verification Strategy); sign in as `candidate@wnc.local` / `Admin@123`; create draft for `aaaaaaa5-1111-1111-1111-111111111111`; upload required PDFs for each required document type; POST confirm to `/student/applications/{draftId}/confirm`.
    Expected: HTTP redirect to `/student/applications/{id}`; DB shows `CurrentStatus = "SUBMITTED"` and exactly one `ApplicationStatusHistory` row with `FromStatus = "DRAFT"`, `ToStatus = "SUBMITTED"`.
    Evidence: .sisyphus/evidence/task-3c-confirm-success.png

  Scenario: Confirm blocked when required docs missing
    Tool: Playwright
    Steps: Run SQL cleanup; sign in as `candidate@wnc.local` / `Admin@123`; create draft; upload only optional docs (or none); POST confirm.
    Expected: HTTP 400 response; DB still shows `CurrentStatus = "DRAFT"`; no history row created.
    Evidence: .sisyphus/evidence/task-3c-confirm-missing-docs.png
  ```

  **Commit**: YES | Message: `feat(student-applications): implement submission confirmation with required-doc gate` | Files: `wnc/Features/Students/Applications/Controllers/StudentApplicationsController.cs`

- [x] 4. Build the document upload wizard page (wizard Step 2)

  **What to do**: Implement `GET /student/applications/{id:guid}/documents` (the wizard Step 2 page). It must load the `AdmissionApplication` (candidate-scoped, must be `DRAFT`), the `RoundProgram` with its `DocumentRequirements` (including `DocumentType`), and the already-uploaded `ApplicationDocument` records for this application grouped by `DocumentTypeId`. Render a form per document requirement showing: document type name, required/optional badge, current uploaded files list (with filename, delete button for each), an upload input (accept `.pdf`), and a counter showing `uploadedCount / MaxFiles`. The confirm button at the bottom must only be enabled when all `IsRequired = true` document types have at least one uploaded file. The page must show per-document-type upload status and file count. Add `data-testid="doc-upload-form-{documentTypeId}"`, `data-testid="doc-upload-input-{documentTypeId}"`, `data-testid="doc-uploaded-list-{documentTypeId}"`, `data-testid="doc-uploaded-file-{documentId}"`, `data-testid="confirm-submit-button"`, and `data-testid="confirm-submit-disabled"` (when blocked). This page is only accessible for DRAFT applications; non-DRAFT applications should redirect to the detail page.
  **Must NOT do**: Do not allow any uploads or file management for SUBMITTED applications. Do not expose internal notes or admin-only fields.

  **Recommended Agent Profile**:
  - Category: `visual-engineering` — Reason: multi-section upload form with per-document state, file lists, and conditional confirm button.
  - Skills: [`ui-ux-pro-max`] — Reason: complex form state, upload feedback, required-field enforcement, and disabled/enabled CTA patterns.
  - Omitted: [`ef-core`] — Reason: data usage is already specified; focus is on UI rendering and interaction.

  **Parallelization**: Can Parallel: NO | Wave 2 | Blocks: none | Blocked By: 1, 2, 3a, 3b, 7

  **References** (executor has NO interview context — be exhaustive):
  - Pattern: `wnc/Views/Student/ProgramDetail.cshtml:79-117` — existing document requirements rendering as reference for section style.
  - API/Type: `wnc/Models/RoundDocumentRequirement.cs:5-18` — `IsRequired`, `MaxFiles`, `DocumentType` for per-doc rendering.
  - API/Type: `wnc/Models/ApplicationDocument.cs:7-22` — uploaded file display fields.
  - Layout: `wnc/Views/Shared/_StudentLayout.cshtml:30-44` — top navigation conventions.

  **Acceptance Criteria** (agent-executable only):
  - [ ] Document requirements page renders all document types for the round program with per-type file lists and upload inputs.
  - [ ] Uploaded files appear in the list with a delete button immediately after upload.
  - [ ] Confirm button is disabled and shows `data-testid="confirm-submit-disabled"` when any required document is missing.
  - [ ] Confirm button is enabled when all required docs have at least one uploaded file.
  - [ ] Navigating to documents page for a non-DRAFT application redirects to the detail page.

  **QA Scenarios** (MANDATORY — task incomplete without these):
  ```
  Scenario: Upload page shows all requirements with correct required/optional badges
    Tool: Playwright
    Steps: Run SQL cleanup; sign in as `candidate@wnc.local` / `Admin@123`; create draft for `aaaaaaa5-1111-1111-1111-111111111111`; open `/student/applications/{draftId}/documents`.
    Expected: Page renders each document requirement with name, required/optional badge, MaxFiles limit, and an upload input; no confirm button enabled.
    Evidence: .sisyphus/evidence/task-4-doc-page-render.png

  Scenario: Confirm button enables after all required docs uploaded
    Tool: Playwright
    Steps: Run SQL cleanup; sign in; create draft; upload required PDFs for all required doc types; observe `data-testid="confirm-submit-disabled"` disappears and `data-testid="confirm-submit-button"` becomes visible/enabled.
    Expected: Confirm button is enabled and visible; no `confirm-submit-disabled` indicator present.
    Evidence: .sisyphus/evidence/task-4-confirm-enabled.png

  Scenario: Submitted application redirects away from documents page
    Tool: Playwright
    Steps: Complete full wizard (upload + confirm) for `aaaaaaa5`; open `/student/applications/{submittedId}/documents`.
    Expected: Redirect to `/student/applications/{submittedId}` detail page; documents page is not rendered.
    Evidence: .sisyphus/evidence/task-4-submitted-redirect.png
  ```

  **Commit**: YES | Message: `feat(student-applications): build document upload wizard page` | Files: `wnc/Views/Student/Applications/Documents.cshtml`, `wnc/Features/Students/Applications/Controllers/StudentApplicationsController.cs`, `wnc/Features/Students/Applications/ViewModels/**`

- [x] 5. Build the student applications list page

  **What to do**: Implement the `GET /student/applications` page to render the authenticated candidate's applications only, newest-first. Each row/card must show application code, round name, program name, optional major, submitted timestamp (or "Bản nháp" for DRAFT), current status badge (DRAFT vs SUBMITTED), and a "view status" link to `/student/applications/{id}`. Add `data-testid="applications-list"` to the list container and `data-testid="application-card-{applicationId}"` to each item. Render an empty state when the candidate has no applications.
  **Must NOT do**: Do not include edit/delete actions, admin-only metadata, or information from other candidates.

  **Recommended Agent Profile**:
  - Category: `visual-engineering` — Reason: Razor list-page implementation with status presentation.
  - Skills: [`ui-ux-pro-max`] — Reason: consistent card layout, badges, and empty-state design.
  - Omitted: [`fluentui-blazor`] — Reason: not used in this MVC Tailwind page.

  **Parallelization**: Can Parallel: YES | Wave 2 | Blocks: 6 | Blocked By: 1, 2, 3a, 3b

  **References** (executor has NO interview context — be exhaustive):
  - Pattern: `wnc/Views/Student/Rounds.cshtml:21-84` — card-based student listing style and section structure.
  - Layout: `wnc/Views/Shared/_StudentLayout.cshtml:39-41` — existing "Đơn đăng ký" nav entry pointing at `/student/applications`.
  - API/Type: `wnc/Models/AdmissionApplication.cs:7-29` — fields to surface on the list page.

  **Acceptance Criteria** (agent-executable only):
  - [ ] `/student/applications` renders only current-candidate applications, sorted newest-first.
  - [ ] Each rendered card includes application code, program/round label, submitted time (or "Bản nháp"), current status badge, and a detail link.
  - [ ] Empty state renders when there are no applications.

  **QA Scenarios** (MANDATORY — task incomplete without these):
  ```
  Scenario: Applications list renders owned submissions newest-first with draft badge
    Tool: Playwright
    Steps: Run SQL cleanup for the demo candidate; sign in as `candidate@wnc.local` / `Admin@123`; create draft (no confirm) for `aaaaaaa5`; complete full wizard (confirm) for `aaaaaaa6`; open `/student/applications`; inspect cards under `data-testid="applications-list"`.
    Expected: Two cards appear: one DRAFT ("Bản nháp") for `aaaaaaa5` and one SUBMITTED for `aaaaaaa6`; SUBMITTED card appears first (newest).
    Evidence: .sisyphus/evidence/task-5-applications-list.png

  Scenario: Empty state handles no submissions
    Tool: Playwright
    Steps: Run SQL cleanup deleting all demo-candidate application/history rows; sign in as `candidate@wnc.local` / `Admin@123`; open `/student/applications`.
    Expected: Page renders an empty-state message and no `application-card-*` entries.
    Evidence: .sisyphus/evidence/task-5-applications-empty.png
  ```

  **Commit**: YES | Message: `feat(student-applications): add student applications list` | Files: `wnc/Views/Student/Applications/Index.cshtml`, `wnc/Features/Students/Applications/Controllers/StudentApplicationsController.cs`, `wnc/Features/Students/Applications/ViewModels/**`

- [x] 6. Build the read-only application detail page with full status timeline, uploaded documents list, and guard messaging

  **What to do**: Implement `GET /student/applications/{id:guid}` as a read-only detail page showing: application summary (application code, round, program, optional major, submitted time, current status badge, rejection reason if present), uploaded document list (filename, upload time, download link via `/uploads/...` path), and a full status timeline sourced from `ApplicationStatusHistories`. Timeline entries must use `PublicNote` when present, must never render `InternalNote`, and must be ordered oldest-to-newest. If the route was reached from a duplicate submit redirect (`source=existing`), render a non-error informational banner with `data-testid="application-info-banner"`. Add `data-testid="application-status-badge"` to the summary badge, `data-testid="application-timeline"` to the timeline container, and `data-testid="document-list"` to the uploaded files section. Show a "Tiếp tục nộp" (continue submitting) link if the application is still in DRAFT status and documents are incomplete.
  **Must NOT do**: Do not add editable controls, officer-only notes, mutation buttons (except continue-link for DRAFT), or hidden debug information.

  **Recommended Agent Profile**:
  - Category: `visual-engineering` — Reason: detail-page presentation plus read-only status timeline and document list.
  - Skills: [`ui-ux-pro-max`] — Reason: timeline layout, information hierarchy, alert/banner states, and document download list.
  - Omitted: [`ef-core`] — Reason: data model usage is already decided; this task focuses on rendering/query composition.

  **Parallelization**: Can Parallel: NO | Wave 2 | Blocks: none | Blocked By: 1, 2, 3a, 3b, 5

  **References** (executor has NO interview context — be exhaustive):
  - Pattern: `wnc/Views/Student/ProgramDetail.cshtml:10-117` — summary card hierarchy and section styling for a detailed student page.
  - API/Type: `wnc/Models/ApplicationStatusHistory.cs:7-18` — timeline fields, including `PublicNote` vs `InternalNote`.
  - API/Type: `wnc/Models/AdmissionApplication.cs:11-19` — status, submitted time, rejection reason, and cancellation fields.
  - API/Type: `wnc/Models/ApplicationDocument.cs:7-22` — uploaded document display (file name, upload time, storage path for download link).
  - API/Type: `wnc/Data/AppDbContext.cs:343-357` — application status history relationship/indexing.

  **Acceptance Criteria** (agent-executable only):
  - [ ] Detail page is read-only and shows summary fields, uploaded document list with download links, and full timeline for the selected owned application.
  - [ ] Timeline entries are ordered oldest-to-newest and exclude `InternalNote`.
  - [ ] DRAFT applications show a "continue submitting" link back to the documents wizard page.
  - [ ] Duplicate-submit redirect renders an informational banner rather than an error page.

  **QA Scenarios** (MANDATORY — task incomplete without these):
  ```
  Scenario: Detail page shows submitted application with timeline and document list
    Tool: Playwright
    Steps: Sign in as the owning candidate with a confirmed (SUBMITTED) application; open `/student/applications/{applicationId}`.
    Expected: `data-testid="application-status-badge"` shows "SUBMITTED"; `data-testid="application-timeline"` contains ordered entries; `data-testid="document-list"` contains uploaded file entries with download links; page has no edit/withdraw controls.
    Evidence: .sisyphus/evidence/task-6-application-detail.png

  Scenario: Draft application shows continue-submit link
    Tool: Playwright
    Steps: Sign in as the owning candidate with a DRAFT application that is incomplete (missing required docs); open `/student/applications/{draftId}`.
    Expected: "Tiếp tục nộp" link to `/student/applications/{draftId}/documents` is visible; no confirm/submit button.
    Evidence: .sisyphus/evidence/task-6-draft-continue-link.png

  Scenario: Duplicate redirect renders info banner
    Tool: Playwright
    Steps: Open `/student/applications/{applicationId}?source=existing` as the owning candidate.
    Expected: Page renders `data-testid="application-info-banner"` with a non-error message explaining the application already exists.
    Evidence: .sisyphus/evidence/task-6-application-existing-banner.png
  ```

  **Commit**: YES | Message: `feat(student-applications): add application detail status timeline and document list` | Files: `wnc/Views/Student/Applications/Detail.cshtml`, `wnc/Features/Students/Applications/Controllers/StudentApplicationsController.cs`, `wnc/Features/Students/Applications/ViewModels/**`

## Final Verification Wave (MANDATORY — after ALL implementation tasks)
> 4 review agents run in PARALLEL. ALL must APPROVE. Present consolidated results to user and get explicit "okay" before completing.
> **Do NOT auto-proceed after verification. Wait for the user's explicit approval before marking work complete.**
> **Never mark F1-F4 as checked before getting user's okay.** Rejection or user feedback -> fix -> re-run -> present again -> wait for okay.
- [x] F1. Plan Compliance Audit — oracle

  **Tool**: `task(subagent_type="oracle")`
  **Steps**:
  1. Review `.sisyphus/plans/student-panel-submission-status.md` and the changed implementation files.
  2. Verify every implemented route, view, guard, and persistence behavior matches Tasks 1–7 exactly.
  3. Confirm all required `data-testid` hooks and guard states exist.
  4. Verify document upload constraints (PDF-only, 5MB limit, required-doc gate) are enforced.
  **Expected**: Oracle reports no deviations from the plan, or lists exact mismatches to fix.
  **Evidence**: `.sisyphus/evidence/f1-plan-compliance.md`

- [x] F2. Code Quality Review — unspecified-high

  **Tool**: `task(category="unspecified-high")`
  **Steps**:
  1. Review changed C# and Razor files for duplication, null-safety, route correctness, and EF query efficiency.
  2. Verify candidate ownership filters exist on every applications query.
  3. Verify file validation (PDF, 5MB) is enforced server-side in the upload endpoint.
  4. Confirm no unrelated refactors or dead code were introduced.
  **Expected**: Reviewer approves code quality or returns exact defects to fix.
  **Evidence**: `.sisyphus/evidence/f2-code-quality.md`

- [x] F3. Real Manual QA — unspecified-high (+ playwright if UI)

  **Tool**: `task(category="unspecified-high")` plus Playwright
  **Steps**:
  1. Run the deterministic cleanup fixture, then execute the full wizard end-to-end (Tasks 3a → 3b upload → 3c confirm).
  2. Execute the full-task Playwright scenarios: wizard entry, PDF upload, oversized reject, confirm success, confirm-blocked, list page, detail page, foreign-access 404, draft continue-link.
  3. Capture screenshots for each scenario.
  4. Run `dotnet build` after QA fixes.
  **Expected**: All UI flows pass exactly as specified and build exits 0.
  **Evidence**: `.sisyphus/evidence/f3-manual-qa.md`

- [x] F4. Scope Fidelity Check — deep

  **Tool**: `task(category="deep")`
  **Steps**:
  1. Compare implemented files against the plan's IN/OUT scope.
  2. Confirm excluded capabilities were not added: edit post-submit, withdraw, non-PDF uploads, >5MB uploads, document replacement after submit, notifications, admin workflow, test-project setup.
  3. Confirm required inclusions were delivered: wizard flow, PDF-only uploads, required-doc gate, list page, read-only detail page, timeline, document list, duplicate/ineligible guards.
  **Expected**: Reviewer explicitly approves scope fidelity or flags exact overreach/missing scope.
  **Evidence**: `.sisyphus/evidence/f4-scope-fidelity.md`

## Commit Strategy
- Commit 1: `feat(student-applications): scaffold student application routes and contracts`
- Commit 2: `feat(student-applications): enforce candidate-scoped application access`
- Commit 3a: `feat(student-applications): implement wizard draft creation`
- Commit 3b: `feat(student-applications): add document upload endpoint and local file storage`
- Commit 3c: `feat(student-applications): implement submission confirmation with required-doc gate`
- Commit 4: `feat(student-applications): build document upload wizard page`
- Commit 5: `feat(student-applications): add student applications list`
- Commit 6: `feat(student-applications): add application detail status timeline and document list`
- Commit 7: `feat(student-applications): serve uploaded files as static content`

## Success Criteria
- Student panel supports end-to-end submission wizard from active program detail through required-document upload to confirmed application status review.
- All student application pages are candidate-scoped and read-only after submission.
- Document uploads enforce PDF-only, 5MB limit, and required-doc gate before confirmation.
- Duplicate and ineligible submission paths are handled deterministically and verified.
- Build passes and verification evidence exists for every task.
