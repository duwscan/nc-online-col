### Features Scaffolded
- Student applications feature slice is now structured under `wnc/Features/Students/Applications/`.
- Route contracts are established for:
  - GET `/student/applications`
  - GET `/student/applications/new/{roundProgramId:guid}`
  - GET `/student/applications/{id:guid}`
  - GET `/student/applications/{id:guid}/documents`
  - POST `/student/applications/{id:guid}/upload`
  - POST `/student/applications/{id:guid}/confirm`
  - DELETE `/student/applications/{id:guid}/documents/{documentId:guid}`

### Architecture
- Followed vertical slice pattern: Controllers and ViewModels grouped by feature.
- All student views use absolute paths for consistency (`~/Views/Student/Applications/...`).
- View models initialized with placeholder data to allow immediate Razor compilation.

### Ownership Query Layer
- `StudentApplicationsController` now resolves the signed-in candidate from `ClaimTypes.NameIdentifier` using the same user-to-candidate lookup shape as `StudentProfileController`.
- Candidate-owned application access is centralized in a reusable base `IQueryable<AdmissionApplication>` so list, detail, and documents actions share the same ownership filter.
- Candidate application list ordering is now `SubmittedAt DESC`, then `CreatedAt DESC`, while detail/documents return `NotFound()` when an application is outside the current candidate scope.

### Document Upload and Storage
- `POST /student/applications/{id:guid}/upload` now accepts `documentTypeId` plus `IFormFile`, reuses the existing candidate-owned application query, and rejects non-`DRAFT` applications before touching storage.
- PDF uploads are limited to 5 MB, require both a `.pdf` filename extension and a PDF MIME type, and are written under `wwwroot/uploads/{applicationId}/{documentTypeId}/{uniqueFilename}.pdf` with `StoragePath` saved as a web-relative `uploads/...` path.
- Uploads mark prior latest documents for the same application/document type as `IsLatest = false`, then create a new `ApplicationDocument` row with `ValidationStatus = "PENDING"`; DELETE removes the row for candidate-owned draft applications and best-effort deletes the physical file.

### Upload Static File Serving
- `Program.cs` now ensures `wwwroot/uploads` exists during startup before any upload links are requested.
- Uploaded files under `wnc/wwwroot/uploads/...` are exposed with a dedicated static-file mapping at `/uploads/...` while leaving the existing `MapStaticAssets()` and route setup unchanged.

### Submission Confirmation Gate
- `POST /student/applications/{id:guid}/confirm` now reuses the authenticated candidate resolution path, loads the candidate-owned application with its `RoundProgram -> DocumentRequirements -> DocumentType`, and returns `NotFound()` for foreign application IDs.
- Confirmation is allowed only while `CurrentStatus == "DRAFT"`; candidate-owned non-draft applications return `BadRequest("Only draft applications can be confirmed.")` so the action cannot create duplicate submission history.
- Required-document completeness is computed from `RoundProgram.DocumentRequirements` where `IsRequired == true`; each required document type is satisfied once at least one `ApplicationDocument` exists for that application and document type, otherwise the endpoint returns `BadRequest` naming the missing document types.
- Successful confirmation updates the existing `AdmissionApplication` row to `SUBMITTED`, sets `SubmissionNumber = 1`, timestamps `SubmittedAt`/`UpdatedAt` with the same `DateTime.UtcNow` value, and inserts exactly one `ApplicationStatusHistory` row with `FromStatus = "DRAFT"` and `ToStatus = "SUBMITTED"` in the same save cycle.

### Student Applications List Page (Task 5)
- `StudentApplicationsIndexViewModel` now carries a nested `ApplicationSummary` type (Id, ApplicationCode, RoundName, ProgramName, MajorName, SubmittedAt, CreatedAt, CurrentStatus, IsDraft computed) and an `Applications` list.
- `GET /student/applications` now queries with `Include(RoundProgram -> Round)`, `Include(RoundProgram -> Program)`, `Include(RoundProgram -> Major)`, ordered by `SubmittedAt DESC` then `CreatedAt DESC`, and maps results to `ApplicationSummary` view models.
- `Index.cshtml` renders a card per application with `data-testid="applications-list"` on the container and `data-testid="application-card-{Id}"` per card; shows empty state when the list is empty.
- Status badge uses a switch expression mapping `DRAFT`→`bg-slate-100 text-slate-600`+"Bản nháp", `SUBMITTED`→`bg-green-100 text-green-700`+"Đã nộp", with fallbacks for other statuses; time label shows `CreatedAt` formatted for drafts or `SubmittedAt` formatted for submitted apps.

### Document Upload Wizard Step (Task 4)
- `StudentApplicationDocumentsViewModel` now includes nested `DocumentRequirementViewModel` (with `DocumentTypeId`, `DocumentName`, `Description`, `IsRequired`, `MaxFiles`, `RequiresNotarization`, `RequiresOriginalCopy`, `Notes`) and `UploadedDocumentViewModel` (with `DocumentId`, `FileName`, `StoragePath`, `FileSize`, `UploadedAt`, `ValidationStatus`).
- `GET /student/applications/{id:guid}/documents` redirects to detail if `CurrentStatus != "DRAFT"`, and otherwise loads the `RoundProgram` with `DocumentRequirements -> DocumentType`, groups latest `ApplicationDocuments` by `DocumentTypeId`, builds per-requirement view models, and computes `IsComplete` by checking all required document type IDs have at least one uploaded document.
- `Documents.cshtml` renders each requirement card with name, required/optional/notarization/original badges, uploaded count, uploaded file list (with PDF icon, filename link, file size, date, validation status badge, delete button), and a conditional upload form shown only when under `MaxFiles`.
- Confirm button uses `data-testid="confirm-submit-disabled"` when `IsComplete == false` (a styled `div` with tooltip) and `data-testid="confirm-submit-button"` (an actual `<form>` POST) when all required docs are present.
- Delete form uses `<input type="hidden" name="_method" value="DELETE" />` instead of `Html.HttpMethodOverride("DELETE")` (which does not exist in this project's Razor version).

### Re-review Outcome (post-rejection fixes)
- Re-review confirmed the upload endpoint now validates that `documentTypeId` belongs to the current application's `RoundProgram` requirements before accepting a file.
- Re-review confirmed `MaxFiles` is enforced server-side in `UploadDocument`, not just hidden in the Razor UI.
- Re-review confirmed the earlier single-file regression was removed: uploads are no longer invalidating prior files for the same document type, and the documents/detail queries now remain consistent with multi-file support inside the configured effective set.
