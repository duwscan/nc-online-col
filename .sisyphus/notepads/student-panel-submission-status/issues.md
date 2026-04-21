### Issue: Out-of-Scope Dockerfile
- The first scaffold run inadvertently created a Dockerfile in the project root.
- This was identified as scope creep during verification.
- The retry has removed the file to restore original scope discipline.

### Issue: C# LSP Runtime Unavailable In Tooling
- `lsp_diagnostics` could not run for `StudentApplicationsController.cs` because the bundled `csharp-ls` process exited with code 131 after failing to locate a .NET runtime in the tool environment.
- Project verification falls back to `dotnet build` from `wnc/wnc`, which is the required compile check for this task.

### Issue: Program Detail Runtime NullReferenceException
- Authenticated `GET /student/programs/{roundProgramId}` crashed in `ProgramDetail.cshtml` because the view rendered `AdmissionMethods` via `method.Method.MethodName` and `method.Method.Description`, but `StudentProgramsController` had not eagerly loaded the nested `Method` navigation.
- Fix: updated the round-program detail query to include `AdmissionMethods -> Method`, which restores the page render path while preserving the task 3a CTA behavior for both wizard entry and existing-application status links.

### Issue: C# Diagnostics Still Blocked For Upload Task
- A fresh `lsp_diagnostics` attempt on `StudentApplicationsController.cs` and `StudentApplicationDocumentStorage.cs` failed again because the bundled `csharp-ls` host cannot find a .NET runtime in the tool environment.
- Verification for task 3b therefore relied on `dotnet build` from `wnc/wnc`, which succeeded; the only reported warnings were pre-existing nullable warnings in `Features/Students/Authentication/Controllers/Signup/StudentSignupController.cs`.

### Issue: C# Diagnostics Unavailable For Static Upload Serving Change
- `lsp_diagnostics` on `wnc/Program.cs` failed with the same `csharp-ls` runtime error (`You must install .NET to run this application`) because the tool environment still lacks a discoverable .NET runtime for the language server.
- Verification for this task therefore relied on `dotnet build` from `wnc/wnc`, which succeeded; the only warnings remained the pre-existing nullable warnings in `Features/Students/Authentication/Controllers/Signup/StudentSignupController.cs`.

### Issue: C# Diagnostics Unavailable For Confirmation Endpoint Change
- `lsp_diagnostics` on `wnc/Features/Students/Applications/Controllers/StudentApplicationsController.cs` failed again because the bundled `csharp-ls` host still cannot find a local .NET runtime (`You must install .NET to run this application`).
- Verification for task 3c therefore relied on `dotnet build` from `wnc/wnc`, which succeeded; the only warnings remained the pre-existing nullable warnings in `Features/Students/Authentication/Controllers/Signup/StudentSignupController.cs`.

### Issue: Upload Validation Used Global Document Types Instead Of Round Requirements
- The upload endpoint previously accepted any existing `DocumentType` record, so a candidate could attach files for document types that were not configured in the current application's `RoundProgram.DocumentRequirements`.
- Fix: `UploadDocument` now resolves the current application's matching `RoundDocumentRequirement` and rejects uploads unless the selected `documentTypeId` belongs to that round program.

### Issue: Single-Latest Toggle Broke Multi-File Semantics And Created Hidden Stale Rows
- Each new upload used to mark every prior same-type document as `IsLatest = false`, which collapsed visibility to one file per type even when `RoundDocumentRequirement.MaxFiles` allowed multiple uploads.
- That also left historical hidden rows in the table, so deleting the only visible file could still leave stale documents that other logic accidentally counted.
- Fix: current-file logic now treats `IsLatest = true` rows as the shared effective set, stops demoting prior current files during normal uploads, and enforces `MaxFiles` against that effective set on the server.

### Issue: Documents, Detail, And Confirm Used Different Document Sets
- The documents page showed only `IsLatest` rows, the detail page also used `IsLatest`, but the confirmation gate counted all historical `ApplicationDocument` rows regardless of whether they were still part of the visible current set.
- Fix: documents, detail, and confirm now all use the same effective-current query and restrict it to document types that belong to the application's configured round requirements.

### Issue: Ordinary Status Navigation Incorrectly Triggered Duplicate Banner
- `ProgramDetail.cshtml` appended `?source=existing` to the normal “view status” link, so opening an existing application from the regular status CTA rendered the duplicate-submit informational banner even though no duplicate-create redirect had happened.
- Fix: the regular status link now points to `/student/applications/{id}` without the query flag; `source=existing` remains reserved for real duplicate-create redirects.

### Issue: Ineligible Wizard Entry Returned 404 Instead Of Guard State
- Visiting `/student/applications/new/{roundProgramId}` for an inactive or closed round program returned `NotFound()`, which surfaced as an error instead of a normal guard response.
- Fix: ineligible wizard entry now redirects back to the related program detail page, where the user can see the non-submittable state without getting an error response.

### Issue: Owned Non-DRAFT Upload/Delete Attempts Returned 404 Instead of 400/403
- The `UploadDocument` and `DeleteDocument` endpoints previously used a combined query `where Id == id && CurrentStatus == "DRAFT"`, which returned `NotFound()` even if the user owned the application but it was already submitted.
- Fix: these endpoints now resolve the application first and return `BadRequest()` with a descriptive message if the application is owned but not in `DRAFT` status, while preserving `NotFound()` for non-owned or non-existent applications.

### Issue: Inconsistent Status Labeling And Missing Test IDs In Application Views
- The application detail page used a manual `switch` for status badges that omitted `UNDER_REVIEW`, `APPROVED`, and `CANCELLED`, leading to inconsistent labeling compared to the list page.
- The document upload input was missing the required `data-testid` attribute for automated QA.
- Fix: aligned `Detail.cshtml` status logic with `Index.cshtml` using a complete `switch` expression and added the missing `data-testid="doc-upload-input-{documentTypeId}"` to the file input in `Documents.cshtml`.
