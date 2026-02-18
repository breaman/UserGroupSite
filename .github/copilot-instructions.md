# UserGroupSite Copilot Instructions

## Project Overview

**UserGroupSite** is an ASP.NET Core 10 Blazor application template with integrated authentication and a robust data layer. It uses a **Blazor Web** hybrid pattern combining Server-rendered Razor Components with interactive WebAssembly (WASM) components for selective interactivity. The app is orchestrated via .NET Aspire for local development.

### Architecture

- **Multi-project structure**: Modular separation across Server, Client (WASM), Data, and Shared layers
- **Aspire orchestration**: Local dev uses containers (SQL Server) managed via `aspire/UserGroupSite.AppHost/`
- **Authentication-first**: ASP.NET Core Identity integrated with EF Core, supporting passkeys, 2FA, and external logins
- **Render modes**: Server-rendered by default; specific components use interactive WebAssembly render mode

## Key Files & Patterns

### Core Architecture Files

| File | Purpose |
|------|---------|
| [src/UserGroupSite.Server/Program.cs](src/UserGroupSite.Server/Program.cs) | Main service configuration: Identity, DbContext, Razor components, Authentication |
| [src/UserGroupSite.Data/Models/ApplicationDbContext.cs](src/UserGroupSite.Data/Models/ApplicationDbContext.cs) | EF Core DbContext extending AuthDbContext with audit logging & fingerprinting |
| [aspire/UserGroupSite.AppHost/AppHost.cs](aspire/UserGroupSite.AppHost/AppHost.cs) | Aspire orchestration: SQL Server container, migrations execution, service registration |
| [aspire/UserGroupSite.ServiceDefaults/Extensions.cs](aspire/UserGroupSite.ServiceDefaults/Extensions.cs) | Shared defaults: OpenTelemetry, health checks, service discovery, resilience handlers |

### Component Structure

- **Server components** (Razor pages): `src/UserGroupSite.Server/Components/Pages/`, `Account/` (Login, Register, MFA endpoints)
- **Client components** (WASM interactive): `src/UserGroupSite.Client/Components/`
- **Shared library** (`src/UserGroupSite.Shared/`): DTOs, common types, and shared models used by both Server and Client projects
- **Shared account infrastructure**: `IdentityComponentsEndpointRouteBuilderExtensions.cs` maps Identity endpoints (Account hooks, passkey creation, personal data export)
- **Code-behind requirement**: Keep Razor markup only in `.razor` files and place all component logic in matching `.razor.cs` code-behind files.

## Developer Workflows

### Starting the Application

```bash
# From repo root - Aspire orchestrates everything
cd aspire/UserGroupSite.AppHost
dotnet run
```

This automatically:
1. Starts SQL Server container (`UserGroupSite-sqlserver`)
2. Restores .NET tools
3. Executes EF migrations via `dotnet ef database update`
4. Launches the Server and Client projects

### Adding/Running Migrations

```bash
cd src/UserGroupSite.Server
dotnet ef migrations add MigrationName -p ../UserGroupSite.Data
```

Migrations auto-run on Aspire startup via the executable task in `AppHost.cs`.

### Key Commands

- **Debug**: Use VS Code launch configs or `dotnet run` from `aspire/UserGroupSite.AppHost/`
- **Database reset**: Stop Aspire, remove `UserGroupSite-sqlserver` container, restart
- **Build**: Implicit via `dotnet run`; explicit: `dotnet build` from solution root

## Project-Specific Conventions

### Identity & Authentication

- **User model**: `UserGroupSite.Data.Models.User` (extends IdentityUser<int>)
- **Roles enabled**: `AddRoles<Role>()` explicitly configured despite using IdentityCore
- **Passkey support**: Built-in via Identity Passkey endpoints (`/Account/Passkey*`)
- **Email sender**: Currently no-op (`IdentityNoOpEmailSender`) — replace for real email
- **Password policy**: Minimal (length >= 6) for development; tighten in production

### DbContext & Auditing

- **Base class**: `AuthDbContext` implements fingerprinting (CreatedBy, ModifiedBy) and audit logging
- **Models**: Inherit from `AuthDbContext` and call `AddFingerPrinting()` automatically on SaveChanges
- **AuditLog table**: Auto-populated via **SaveChangesAsync** hook
- **See**: [src/UserGroupSite.Data/Models/AuthDbContext.cs](src/UserGroupSite.Data/Models/AuthDbContext.cs) for audit hooks

### Rendering & Interactivity

- **Default**: Server-rendered components
- **WASM interactive**: Explicitly marked components use `@rendermode InteractiveWebAssembly`
- **Auth state serialization**: Enabled via `.AddAuthenticationStateSerialization()` to pass auth context to WASM components
- **Client setup**: [src/UserGroupSite.Client/Program.cs](src/UserGroupSite.Client/Program.cs) registers root component

### URL Conventions

- **Lowercase + no trailing slashes** enforced via `RouteOptions` (SEO best practice)
- **Account endpoints**: `/Account/*` for auth flows, `/Account/Manage/*` for profile management

## Critical Integration Points

### Service Discovery

Under Aspire, all services use built-in service discovery. HTTP client factories automatically resolve service names.

### Telemetry

OpenTelemetry configured in `ServiceDefaults`:
- **Logging**: Formatted messages + scopes included
- **Tracing**: Console exporters in development
- **Metrics**: Standard .NET runtime + custom counters supported
- See [aspire/UserGroupSite.ServiceDefaults/Extensions.cs](aspire/UserGroupSite.ServiceDefaults/Extensions.cs#L50+) for configuration

### Entity Framework

- **Connection string**: Injected via `Constants.DatabaseConnectionString` from Aspire configuration
- **Lazy loading**: Disabled (explicit `.Include()` recommended)
- **Sensitive data logging**: Enabled in Development only
- **Design-time factory**: Not configured; use Aspire AppHost for migrations

## Important Codebase Notes

1. **Shared project**: Use `src/UserGroupSite.Shared/` for all DTOs, view models, and common types that need to be accessed by both Server and Client. The Client project references this to avoid code duplication and maintain consistency across API contracts.
2. **No Repository pattern**: DbContext accessed directly; consider adding service layer for business logic
3. **Minimal Identity setup**: Email confirmation commented out (`RequireConfirmedEmail`); enable cautiously
4. **Passkey endpoints**: Require both antiforgery tokens + authentication; see endpoint implementations for security details
5. **`IdentityRedirectManager`**: Custom helper for auth-related redirects; used across Account components
6. **Routes.razor**: Maps all pages; nested Account section auto-routes to Account/Pages/**

## References

- **Blazor**: [.github/instructions/blazor.instructions.md](.github/instructions/blazor.instructions.md)
- **C# guidelines**: [.github/instructions/csharp.instructions.md](.github/instructions/csharp.instructions.md)
- **README**: [README.md](README.md) (post-template setup notes)
