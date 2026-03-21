# wnc/Features

## OVERVIEW

Vertical feature slices for Admin and Students modules. Each feature contains its own Controllers and ViewModels, following domain-driven organization.

## FEATURES

### Admin Authentication
- `Features/Admin/Authentication/Controllers/Login/` — AdminLoginGetController, AdminLoginPostController
- `Features/Admin/Authentication/Controllers/Logout/` — AdminLogoutController
- `Features/Admin/Authentication/ViewModels/` — AdminLoginViewModel
- Auth flow: Validates ADMIN, ADMISSION_OFFICER, or REPORT_VIEWER role

### Students Authentication
- `Features/Students/Authentication/Controllers/Login/` — StudentLoginGetController, StudentLoginPostController
- `Features/Students/Authentication/Controllers/Logout/` — StudentLogoutController
- `Features/Students/Authentication/ViewModels/` — StudentLoginViewModel
- Auth flow: Validates CANDIDATE role only

## PATTERN

```
Features/
├── Admin/
│   └── Authentication/
│       ├── Controllers/
│       │   ├── Login/
│       │   │   ├── AdminLoginGetController.cs
│       │   │   └── AdminLoginPostController.cs
│       │   └── Logout/
│       │       └── AdminLogoutController.cs
│       └── ViewModels/
│           └── AdminLoginViewModel.cs
└── Students/
    └── Authentication/
        ├── Controllers/
        │   ├── Login/
        │   │   ├── StudentLoginGetController.cs
        │   │   └── StudentLoginPostController.cs
        │   └── Logout/
        │       └── StudentLogoutController.cs
        └── ViewModels/
            └── StudentLoginViewModel.cs
```

## CONVENTIONS

- Feature-based folder structure (not layer-based)
- Login controllers split into GET (render form) and POST (handle submission)
- Role validation happens server-side in POST controllers
