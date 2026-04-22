# BaseController Usage Guide

## Overview

`BaseController` is an abstract controller class that provides common functionality for all controllers in the WNC admission portal that require authentication.

## Location

`wnc/Controllers/BaseController.cs`

## Inheritance

All controllers requiring authentication should extend `BaseController` instead of `Controller`:

```csharp
public class AdminDashboardController(AppDbContext dbContext) : BaseController(dbContext)
{
    public IActionResult Index()
    {
        return View();
    }
}
```

**Note**: `BaseController` automatically applies the `[Authorize]` attribute, so all inheriting controllers require authentication by default.

## Utility Methods

### GetCurrentUserAsync

Retrieves the currently logged-in user from the database, including their roles.

```csharp
public async Task<IActionResult> Profile()
{
    var user = await GetCurrentUserAsync();
    if (user == null)
    {
        return RedirectToAction("Login", "Auth");
    }

    return View(user);
}
```

**Returns**: `Task<AppUser?>` — The current user with `UserRoles` and `Role` eagerly loaded, or `null` if not authenticated.

### IsInRole

Checks if the current user has a specific role.

```csharp
public IActionResult AdminOnly()
{
    if (!IsInRole("ADMIN"))
    {
        return Forbid();
    }

    return View();
}
```

### IsInAnyRole

Checks if the current user has at least one of the specified roles.

```csharp
public IActionResult StaffArea()
{
    if (!IsInAnyRole("ADMIN", "ADMISSION_OFFICER", "REPORT_VIEWER"))
    {
        return Forbid();
    }

    return View();
}
```

### SetSuccessMessage

Sets a success flash message that will be displayed on the next page load.

```csharp
[HttpPost]
public IActionResult Create(CreateModel model)
{
    // ... validation and saving logic

    SetSuccessMessage("Item created successfully!");
    return RedirectToAction("Index");
}
```

### SetErrorMessage

Sets an error flash message that will be displayed on the next page load.

```csharp
[HttpPost]
public IActionResult Create(CreateModel model)
{
    if (!ModelState.IsValid)
    {
        SetErrorMessage("Please correct the errors and try again.");
        return View(model);
    }

    // ... saving logic
}
```

## Flash Messages (TempData)

`BaseController` uses `TempData` for flash messages. These messages persist for exactly one request and are automatically removed after being read.

### TempData Keys

| Key Name | Usage | Suggested Styling |
|----------|-------|-------------------|
| `SuccessMessage` | Positive feedback (create, update, delete success) | Green alert box |
| `ErrorMessage` | Negative feedback (validation errors, failures) | Red alert box |

### Displaying Flash Messages in Views

**Inline in individual views** (current pattern in existing controllers):

```razor
@if (TempData["SuccessMessage"] is string successMessage)
{
    <div class="alert alert-success">
        @successMessage
    </div>
}

@if (TempData["ErrorMessage"] is string errorMessage)
{
    <div class="alert alert-danger">
        @errorMessage
    </div>
}
```

**Recommended: Centralized in `_Layout.cshtml`** (with a partial view):

Create `Views/Shared/_FlashMessages.cshtml`:

```razor
@if (TempData["SuccessMessage"] is string successMessage)
{
    <div class="rounded-xl bg-green-50 p-4 text-sm text-green-700 mb-4">
        @successMessage
    </div>
}

@if (TempData["ErrorMessage"] is string errorMessage)
{
    <div class="rounded-xl bg-red-50 p-4 text-sm text-red-700 mb-4">
        @errorMessage
    </div>
}
```

Then include it in `_Layout.cshtml` before `@RenderBody()`:

```razor
<div class="container">
    <partial name="_FlashMessages" />
    @RenderBody()
</div>
```

## Authorization Behavior

Because `BaseController` has `[Authorize]`:

1. Unauthenticated users will be redirected to the login page (`/auth/student/login` or `/auth/admin/login` depending on the request path).
2. To allow anonymous access, apply `[AllowAnonymous]` at the controller or action level.

```csharp
[AllowAnonymous]
public class PublicController(AppDbContext dbContext) : BaseController(dbContext)
{
    public IActionResult About() => View();
}
```

## Complete Example

```csharp
using Microsoft.AspNetCore.Mvc;
using wnc.Controllers;
using wnc.Data;

public class TrainingProgramController(AppDbContext dbContext) : BaseController(dbContext)
{
    public async Task<IActionResult> Details(Guid id)
    {
        // Only ADMIN or ADMISSION_OFFICER can access
        if (!IsInAnyRole("ADMIN", "ADMISSION_OFFICER"))
        {
            return Forbid();
        }

        var user = await GetCurrentUserAsync();
        var program = await DbContext.TrainingPrograms.FindAsync(id);
        
        if (program == null)
        {
            SetErrorMessage("Training program not found.");
            return RedirectToAction("Index");
        }

        return View(program);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(Guid id)
    {
        if (!IsInRole("ADMIN"))
        {
            return Forbid();
        }

        var program = await DbContext.TrainingPrograms.FindAsync(id);
        if (program == null)
        {
            SetErrorMessage("Training program not found.");
            return RedirectToAction("Index");
        }

        try
        {
            DbContext.TrainingPrograms.Remove(program);
            await DbContext.SaveChangesAsync();
            SetSuccessMessage("Training program deleted successfully.");
        }
        catch (DbUpdateException)
        {
            SetErrorMessage("Cannot delete a training program with related data.");
        }

        return RedirectToAction("Index");
    }
}
```

## Migration Note

Existing controllers currently inherit directly from `Controller` and use raw `TempData["SuccessMessage"]` / `TempData["ErrorMessage"]` assignments. When migrating to `BaseController`:

1. Change `Controller` to `BaseController`
2. Pass `AppDbContext` to the base constructor
3. Replace `TempData["SuccessMessage"] = "..."` with `SetSuccessMessage("...")`
4. Replace `TempData["ErrorMessage"] = "..."` with `SetErrorMessage("...")`
5. Consider removing inline flash message rendering from views if using the centralized `_FlashMessages.cshtml` partial.
