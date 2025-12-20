# WARP.md

This file provides guidance to WARP (warp.dev) when working with code in this repository.

## Project overview

SmartHR is an ASP.NET Core 8.0 MVC web application for HR management (users/roles, leave requests, calendars, reports, documents).

The main web project is `SMART-HR/SmartHR.csproj`, targeting `net8.0` and using Entity Framework Core with SQL Server (`ApplicationDbContext` + `Migrations/`).

## How to build and run

All commands below assume your current directory is the repository root.

- Change into the web project directory:
  - PowerShell:
    - `cd SMART-HR`

- Build the web app:
  - `dotnet build`

- Run the web app (uses `appsettings.json` connection string and applies pending EF Core migrations at startup via `Database.Migrate()` in `Program.cs`):
  - `dotnet run`

- Debug via Visual Studio/IDE:
  - Open `SMART-HR.sln` and run the `SmartHR` project; profiles are defined in `SMART-HR/Properties/launchSettings.json`.

### Tests

There is currently no separate test project (no `*.Tests.csproj` in this repo). If you add one later, use standard .NET commands from that test project directory, e.g. `dotnet test` or `dotnet test --filter FullyQualifiedName~SomeTestName` to run a single test.

## Database & migrations

- The EF Core `DbContext` is `ApplicationDbContext` in `SMART-HR/ApplicationDbContext.cs`.
- Connection string `DefaultConnection` is configured in `SMART-HR/appsettings.json` (local SQL Server LocalDB by default).
- `Program.cs` registers `ApplicationDbContext` with SQL Server and calls `Database.Migrate()` on startup to apply any pending migrations automatically.
- Code‑first migrations live under `SMART-HR/Migrations/` and are applied to the `SmartHRDb` database.

Typical EF tooling (from `SMART-HR/`):

- Add a migration: `dotnet ef migrations add <MigrationName>`
- Apply migrations: `dotnet ef database update`

(Requires the EF Core tooling CLI to be available; the project already references `Microsoft.EntityFrameworkCore.Tools`.)

## High-level application architecture

### HTTP pipeline & cross-cutting concerns

- Entry point: `SMART-HR/Program.cs` configures a standard MVC pipeline (`UseRouting`, `UseStaticFiles`, `UseSession`, `UseAuthorization`) and maps a default route `{controller=Home}/{action=Index}/{id?}`.
- Session is enabled via `AddSession` and used as the main authentication state store (`UserId`, `UserEmail`, `UserName`, `UserRole`).
- `SmartHR.Filters.AuthenticationFilter` is registered globally in `AddControllersWithViews(options => options.Filters.Add<AuthenticationFilter>())`:
  - Allows anonymous access only to a small set of `Home` actions (`Index`, `Login`, `Register`, `Logout`, `Error`, `Privacy`).
  - For any other route, if `UserId` is absent in session, it redirects to `Home/Login` with a `returnUrl` back to the original request.

When adding new controllers/actions, be aware that **they are protected by this filter by default**; if they must be anonymously accessible, they need to be added to the allowed route list in `AuthenticationFilter`.

### Persistence layer (EF Core)

- `ApplicationDbContext` defines `DbSet<T>`s for core entities in `SMART-HR/Models/`:
  - `Utilisateur`, `Employe`, `Manager`, `RessourceHumaine`, `Admin`
  - `TypeConge`, `DemandeConge`
  - `Rapport`
  - `Calendriers`
- Entity relationships are configured in `OnModelCreating`:
  - One‑to‑one between `Utilisateur` and each role entity (`Admin`, `Employe`, `RessourceHumaine`) with `DeleteBehavior.Restrict` to prevent accidental cascade delete of users.
  - `Manager` has many `Employes` and many `Rapports` with `DeleteBehavior.SetNull` on dependents.
  - `Employe` has many `DemandesConges` (cascade delete) and optional `Manager`.
  - `TypeConge` has many `DemandesConges` (cascade delete).

If you introduce new domain entities, keep this convention: put POCOs in `Models/` and wire them in `ApplicationDbContext` with explicit relationships.

### Domain model & roles

- `Models/Utilisateur` is the core user record (name, email, hashed password, `Role`, `Actif`).
- Each “role” has its own table with a 1‑1 link to `Utilisateur`:
  - `Admin`, `RessourceHumaine`, `Manager`, `Employe`.
- `Employe` models HR‑specific data and leave allowance (`JoursCongesTotal` default 30) and links to its `Manager`.
- Leave requests are modeled by `DemandeConge` (employee, type, dates, status, optional approving manager).
- `Calendriers` is a generic calendar event (holidays, events, or approved leave) optionally linked to an `Employe`.
- `Rapport` stores generated report metadata (title, description, type, file path, optional manager owner).

### Services layer

Services encapsulate core business logic over the DbContext and are registered as scoped services in `Program.cs`.

- `Services/Interfaces/IUtilisateurService` + `Services/Implementations/UtilisateurService`:
  - User CRUD helpers (`GetAll`, `GetById`, `GetByEmail`, `Create`, `Update`, `Delete`).
  - Authentication (`Authenticate`) and password handling (`HashPassword`, `VerifyPassword`).
  - Passwords are hashed using SHA‑256 with a fixed salt (`SmartHR_{password}_Salt2025`) and stored as Base64 strings in `Utilisateur.MotDePasse`.
  - Controllers such as `HomeController` and `AdminsController` rely on this service rather than hashing/comparing themselves.

- `Services/Interfaces/ICongeService` + `Services/Implementations/CongeService`:
  - Computes leave balances per employee and per type (`LeaveBalanceItem`), based on approved (`Statut == "Accepté"`) `DemandeConge` rows.
  - Exposes leave history and requests collections for an employee.
  - Validates and creates new leave requests (`RequestLeave`) with rules:
    - End date must be after start date.
    - Type of leave must exist; duration must be positive.
    - Requested days must not exceed remaining balance for that leave type.
    - No overlap with already approved leaves for that employee.
  - Provides a “team calendar” view combining static public holidays and approved leaves for teammates reporting to the same manager.

When adding new business logic around users or leaves, prefer extending these services and injecting them into controllers, instead of pushing logic into controllers directly.

### MVC layer (controllers, view models, views)

Controllers live in `SMART-HR/Controllers/`, with matching view folders under `SMART-HR/Views/` and per‑feature view models under `SMART-HR/ViewModels/`.

Key controllers and their roles:

- `HomeController` (namespace `SMART_HR.Controllers`):
  - Public landing (`Index`, `Privacy`).
  - Authentication (`Login` GET/POST, `Register` GET/POST) using `IUtilisateurService`.
  - Session management (`Logout`).
  - User profile (`Profile`) and password change (`ChangePassword`) views built on `ProfileViewModel` and `ChangePasswordViewModel` from `AuthViewModels`.

- `AdminsController` (admin dashboard + user management):
  - Uses `CheckAdminAccess()` / `IsAdmin()` and session `UserRole` to enforce admin‑only access to new actions.
  - `Dashboard` builds an `AdminDashboardViewModel` with user statistics and recent users.
  - `Users` lists all `Utilisateur` records with search and filters.
  - `CreateUser` / `EditUser` / `UserDetails` / `ResetPassword` / `ToggleActive` / `DeleteUser` are the main admin CRUD endpoints over users and their role‑specific entities.
  - Older `Admins` CRUD actions (`Index`, `Details`, `Create`, `Edit`, `Delete`) remain for backward compatibility and operate directly on the `Admin` entity.

- `DemandeCongesController`, `CalendrierController`, `EmployesController`, `ManagersController`, `DocumentsController`, `RapportsController`:
  - Implement the leave management, calendar, employee/manager management, document, and reporting modules.
  - `RapportsController.Index` uses `RapportDashboardViewModel` to compute analytics: employees per department, leave type utilization, peak request periods, annual trends, and global approval stats.

View models provide strongly‑typed data for each UI:

- `AuthViewModels.cs` – login, registration, profile, and password change.
- `AdminViewModels.cs` – admin dashboard and user management shapes.
- `CongeViewModels.cs` – leave request lists, creation form, detail view, and “Mes demandes” balances.
- `CalendarViewModels.cs` – team calendar (holidays + team leaves).
- `RapportViewModels.cs` – data structures for the HR reporting dashboard and charts.

The views in `Views/Admins/`, `Views/DemandeConges/`, `Views/Calendriers/`, `Views/Rapports/`, etc. are tightly coupled to these view models; when changing a view model property, update the corresponding views and controller actions together.

### UI layout & shared components

- `_Layout.cshtml` (in `Views/Shared/`) defines the global layout, navigation bar, footer, and references Bootstrap 5 and Bootstrap Icons.
  - The “Admin” navigation link is shown only when the session `UserRole` is `"Admin"`, and routes to `Admins/Dashboard`.
- `_ValidationScriptsPartial.cshtml` is included in forms for client‑side validation.
- `_CompanyNews.cshtml` is a shared partial intended to render company news from `App_Data/companyNews.json` and `Models/CompanyNewsItem` (currently minimal/placeholder).

## Authentication & authorization model

- Authentication is custom, session‑based, and implemented via `HomeController` + `IUtilisateurService`.
  - On successful login, `HomeController.Login` stores `UserId`, `UserEmail`, `UserName`, and `UserRole` in session.
  - Registration creates a new `Utilisateur` with role `"Employe"` and immediately logs the user in via session.
- Authorization is enforced in two places:
  - Globally: `AuthenticationFilter` ensures that any non‑whitelisted route requires an authenticated session (`UserId` in session).
  - Per‑feature: admin endpoints in `AdminsController` check `UserRole == "Admin"` via `IsAdmin()` / `CheckAdminAccess()` and redirect to `Home/Index` with an error message if unauthorized.

When adding new sensitive features (e.g., admin tools, manager‑only screens), follow the existing patterns:

- Use `AuthenticationFilter` for login requirements.
- Use `UserRole` from session and helper methods similar to `IsAdmin()` or dedicated role checks in the relevant controller.

## Existing documentation inside the repo

The `SMART-HR` project includes detailed documentation for the Admin module:

- `ADMIN_FEATURES.md` – full description of admin dashboard features, security, and UI.
- `ADMIN_SETUP_GUIDE.md` – step‑by‑step guide to set up an admin user and manually test all admin flows.
- `ADMIN_IMPLEMENTATION_SUMMARY.md` – technical implementation summary (files, controllers, view models, and statistics).
- `ADMIN_QUICK_REFERENCE.md` – quick reference of admin URLs, common tasks, and visual cues.

When modifying or extending the admin dashboard, consult these files to keep UX, wording, and feature behaviors consistent.
