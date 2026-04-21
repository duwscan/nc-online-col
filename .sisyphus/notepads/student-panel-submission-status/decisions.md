### Controller Routing
- Explicit route attributes used on each action method to maintain clear contract control.
- Authorize(Roles = "CANDIDATE") applied at controller level for consistent security.

### View Strategy
- Layout `~/Views/Shared/_StudentLayout.cshtml` applied to all application views.
- Placeholder markup uses Tailwind classes to match existing student profile/rounds UI.

### Upload Serving Strategy
- Used a dedicated `UseStaticFiles` registration for the physical `wwwroot/uploads` folder with `RequestPath = "/uploads"` as the simplest way to serve uploaded PDFs without changing existing static asset mappings.
- Ignored `wnc/wwwroot/uploads/` in git so runtime upload artifacts remain local and out of version control.
