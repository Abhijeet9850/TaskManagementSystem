# Task Management System

Full-stack solution: **ASP.NET Core 8 Web API** backend + **Angular 17 (standalone components)** frontend.

## Folder structure

```
TaskManagementSystem/
├── Backend/TaskManager.API/   # ASP.NET Core Web API
└── Frontend/task-management-angular/   # Angular SPA
```

## Backend setup

1. Requires .NET 8 SDK and SQL Server (or LocalDB).
2. Update `appsettings.json`:
   - `ConnectionStrings:DefaultConnection` — your SQL Server connection string.
   - `Jwt:Key` — replace with a long random secret (32+ chars) before any real use.
3. Create the database and apply migrations:
   ```bash
   cd Backend/TaskManagementSystem.API
   dotnet restore
   dotnet tool install --global dotnet-ef   # if not already installed
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```
   > If you're regenerating migrations after pulling this file-management update, note the schema now includes a `TaskAttachments` table (replacing the old single `AttachmentPath` column on `Tasks`).
4. Run the API:
   ```bash
   dotnet run
   ```
   Swagger UI will be available at `https://localhost:5001/swagger` (port may vary — check console output and update the Angular `API_URL` constants in `core/services/*.service.ts` if different).

### Notes on how requirements were implemented
- **Auth**: `AuthController` handles register/login/logout. JWT is signed with HMAC-SHA256; `RememberMe` extends token life from `AccessTokenExpiryMinutes` to `RememberMeExpiryDays`. Password rule (8+ chars, upper/lower/number) is enforced via a `[RegularExpression]` on `RegisterDto`. Email uniqueness is checked against `Users` table (and separately for `Employees`).
- **Roles**: `UserRole` enum (`Admin`/`Employee`). Controllers use `[Authorize(Roles = "Admin")]` where admin-only, and `TasksController` filters by the caller's linked `employeeId` claim for non-admins.
- **Dashboard**: `DashboardController` exposes `/api/dashboard/admin` and `/api/dashboard/employee`, each returning the exact counts requested.
- **Employees**: Full CRUD in `EmployeesController` with server-side search, sort (`sortBy`/`sortDir`), and pagination (`page`/`pageSize`), returned as a `PagedResult<T>`.
- **Tasks**: Full CRUD in `TasksController`. Business rules enforced server-side: due date ≥ start date, completed tasks reject edits, and employees only ever see their own tasks (admins see all).
- **Notifications**: `NotificationService` writes in-app `Notification` records on assignment and completion. `CheckAndNotifyDueSoonAsync()` is written to be invoked by a scheduled job (e.g. a `BackgroundService`/Hangfire/cron calling it periodically) to flag tasks due within 24 hours — wire that trigger up based on your hosting setup.
- **File upload**: A dedicated `TaskAttachment` entity (many-per-task) replaces the old single `AttachmentPath` field, to support the full admin/employee file-permission model:
  - **Admin**: uploads an optional file directly on `POST /api/tasks` (multipart, `[FromForm]`); can **replace** the main attachment via `PUT /api/tasks/{id}/attachment` (blocked once the task is `Completed`); can **download** any attachment via `GET /api/tasks/attachments/{attachmentId}/download`; can **delete** any attachment via `DELETE /api/tasks/attachments/{attachmentId}`.
  - **Employee**: can **list**/**download** attachments only on tasks assigned to them (`GET /api/tasks/{id}/attachments`, same download endpoint — server checks `AssignedEmployeeId` against the caller's `employeeId` claim and returns `403` otherwise); can **upload supporting documents** to their own tasks via `POST /api/tasks/{id}/attachments` (marked `IsMain = false` to distinguish from the admin's main file); cannot see or touch another employee's task attachments — every attachment endpoint re-checks task ownership server-side, not just in the Angular UI.
  - All uploads still go through `FileService` (PDF/JPG/PNG, 5 MB cap). Deleting an attachment or a task also deletes the underlying physical file.
- **Reports**: `ReportService` uses ClosedXML (Excel) and CsvHelper (CSV) to generate Completed / Pending / Employee-wise reports, streamed back via `ReportsController` (`GET /api/reports?type=...&format=...`).

### Suggested next steps for production hardening
- Add FluentValidation or more granular DTO validation messages.
- Add a background hosted service to actually trigger `CheckAndNotifyDueSoonAsync()` on a timer.
- Add refresh-token rotation if you want silent renewal beyond the JWT's lifetime.
- Add integration tests (WebApplicationFactory) for the business rules (due date, completed-task lock, employee task scoping).
- Serve `/Uploads` behind authorization (currently static-served) if attachments are sensitive.

## Frontend setup

1. Requires Node.js 18+ and Angular CLI 17.
2. Install dependencies:
   ```bash
   cd Frontend/task-management-angular
   npm install
   ```
3. Update the `API_URL` constant at the top of each file in `src/app/core/services/` if your backend runs on a different port than `https://localhost:5001`.
4. Run the dev server:
   ```bash
   npm start
   ```
   App will be available at `http://localhost:4200`.

### Structure
- `core/` — models, services (one per resource), route guards (`authGuard`, `adminGuard`), and the JWT HTTP interceptor.
- `auth/` — login & register standalone components.
- `dashboard/` — role-aware dashboard (`dashboard-router.component` picks admin vs employee view).
- `employees/` — list (search/sort/paginate) + modal add/edit form. Admin-only route.
- `tasks/` — list (role-scoped) + modal add/edit form + inline file upload.
- `reports/` — Excel/CSV export buttons for each report type.

All components are Angular standalone components (no NgModules), lazy-loaded via `app.routes.ts`.

## Known simplifications (flagged for your review)
- Styling is intentionally plain inline CSS in `styles.css` — swap in your preferred UI kit (e.g. Angular Material, PrimeNG) as needed.
- The "logout" endpoint doesn't maintain a server-side token blocklist — it's a client-side token discard, which is standard for stateless JWT unless you need forced early invalidation.
- Notification delivery is in-app only (DB records) — no email/push wired up.
