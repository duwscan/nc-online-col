# UI/UX System Design

## Purpose

This document defines the UI/UX system direction for the MVC-based admissions portal with separate login flows for students and administrators. The implementation supports cookie authentication, role-based routing, and clear error feedback for login-related journeys.

## Design Principles

- Mobile-first: primary student access is expected from phones, so forms use single-column layouts, large hit targets, and concise copy.
- Immediate feedback: validation errors appear inline and as a form-level banner so users understand whether the issue is missing input, invalid credentials, or insufficient role access.
- State visibility: signed-in state is always visible in the shared layout through user identity, active role chips, and a single logout action.
- Progressive disclosure: student and admin entry points are separated at the route and page level to reduce ambiguity and role confusion.
- Consistent trust cues: headings, support text, and button labels explain the destination portal before the user submits credentials.

## Personas

### Student / Candidate

- Usually accesses the portal from a mobile browser.
- Needs a low-friction login experience using either email or phone number.
- Expects clear next steps after authentication and understandable feedback when access is denied.

### Admin / Officer / Report Viewer

- Uses desktop more often and expects a distinct operational portal.
- Needs stronger context that the page is for internal roles only.
- Must be blocked with a clear message when the account exists but does not carry an allowed admin role.

## Layout Patterns

### Public Portal

- Shared layout uses a lightweight top navigation with Student login and Admin login entry points.
- Large hero content on the home page explains the two login paths and the role expectations for each.
- Student login page uses supportive language and accessible, low-friction form inputs.

### Admin Portal

- Admin login page uses a more restrained visual style and emphasizes that only internal staff roles are allowed.
- Shared header still shows the authenticated user, but role chips distinguish candidate from admin-capable sessions.

## Components And States

### Inputs

- Text inputs use strong contrast, visible focus rings, and helper copy that accepts email or phone number.
- Password fields are masked by default and return validation errors directly below the field.

### Error Banner

- A top-of-form error banner communicates authentication failure or authorization mismatch.
- Banner copy should avoid leaking whether the identifier exists unless the failure is explicitly about role mismatch after a valid credential check.

### CTA Buttons

- Primary actions are full-width on mobile and compact on desktop.
- Buttons remain visually stable between idle, hover, focus, and disabled states.

### Loading / Disabled

- Submit buttons should move to a disabled state during request processing in future enhancements.
- Current MVC implementation supports server-side validation and round-trip feedback without client-side spinners.

## Login Flows

### Student Flow

1. User opens `GET /auth/student/login`.
2. User enters email or phone number and password.
3. Server validates credentials against `AppUser.PasswordHash`.
4. Server verifies that the account has the `CANDIDATE` role.
5. Success signs the cookie principal and redirects to the home page or local return URL.
6. Failure returns the same view with a banner and audit log entry.

### Admin Flow

1. User opens `GET /auth/admin/login`.
2. User enters email or phone number and password.
3. Server validates credentials against `AppUser.PasswordHash`.
4. Server verifies that the account has one of `ADMIN`, `ADMISSION_OFFICER`, or `REPORT_VIEWER`.
5. Success signs the cookie principal and redirects to the home page or local return URL.
6. Failure returns the same view with a banner and audit log entry.

## Mapping UI To SRS

| SRS item | UI implication | Implementation note |
| --- | --- | --- |
| FR-01. Đăng ký tài khoản | Login pages should link conceptually to a future self-service candidate registration path. | Current scope documents the pattern and leaves the registration screen for a later feature. |
| FR-02. Đăng nhập | Separate student/admin login pages with role-aware messaging and validation feedback. | Implemented through MVC controllers, Tailwind views, cookie auth, and `AuthLog` entries. |
| FR-04. Quản lý hồ sơ tài khoản | Signed-in state should clearly show who is authenticated before profile features are added. | Shared layout displays user identity and role chips. |
| FR-05. Phân quyền người dùng | The interface must prevent cross-portal confusion and enforce role boundaries. | Student login only accepts `CANDIDATE`; admin login only accepts internal roles. |

## Accessibility Notes

- Each page maintains a clear heading hierarchy with one primary `h1`.
- Validation messages are rendered inline next to their related inputs and can also appear in a form-level error container.
- Color is not the sole source of meaning; error and role states also include explicit text labels.
