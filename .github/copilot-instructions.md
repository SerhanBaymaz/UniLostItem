# Agentic Instructions (Copilot, Gemini CLI, Claude Code) for TemplateDeneme

## Core Architecture

- Clean Architecture with five layers: Domain (pure entities), Application (CQRS + MediatR), Infrastructure (External services, JWT, Auth), Persistence (EF Core + PostgreSQL), API (ASP.NET Core controllers, middleware, Swagger).
- Feature slices under `Application/Features/SerhanKitaplar` and `Application/Features/Auth` use Commands/Queries folders with DTOs, validators, and handlers; mapping in `Application/Core/MappingProfiles.cs`; validation pipeline via `ValidationBehavior`.
- Auth module (`Application/Features/Auth`) includes: `Commands/Login`, `Commands/Register`, `Commands/RefreshToken`, `Commands/UpdateUserProfile`, `Queries/GetCurrentUser` with corresponding DTOs and Validators.
- Domain entity `ApplicationUser` extends `IdentityUser` with `FirstName`, `LastName`, `IsActive`, `RefreshToken`, `RefreshTokenExpiryTime`, `CreatedDate`, `LastLoginDate`, `ProfileImageUrl`.
- Persistence layer uses `AppDbContext` implementing `IAppDbContext` and extends `IdentityDbContext<ApplicationUser>`; migrations live in `Persistence/Migrations`; database seeding in `DbInitializer` (called on startup) - seeds `BaseUser` role.
- Infrastructure provides `IJwtService` (JWT generation/validation) and `ICurrentUserService` (access current authenticated user from claims).
- API startup is extension-driven (`API/Extensions/*`) wired in `API/Program.cs`; custom error handling in `API/Middleware/ExceptionMiddleware.cs`; standardized responses in `API/Responses`.

## Build & Run

- Restore/build/run: `dotnet restore`, `dotnet build`, `dotnet run --project API` (hot reload: `dotnet watch run --project API`).
- Docker dev stack: `docker-compose -f docker-compose.dev.yml up -d`; default ports: API 8089, PostgreSQL 5432, Seq 8088 (UI) / 5348 (ingestion).
- Environment files: managed via `EnvLoader` helper. Copy `example.dev.env` → `dev.env` (or `example.prod.env` → `prod.env`); `EnvLoader` loads variables before builder construction.

## CI/CD Pipelines

- Two GitHub Actions: `ci-dev.yml` (dev + SonarCloud) and `ci-prod.yml` (prod).
- Triggers: dev → push/PR on main/develop; prod → push on main or version tags (v*.*.\*).
- Steps: build, test, Docker build & push to GHCR; dev job includes SonarCloud analysis.
- Images: tagged `latest` and `sha-<git-sha>`; names `ghcr.io/serhanbaymaz/dev-templatedeneme` and `ghcr.io/serhanbaymaz/prod-templatedeneme`.
- Dev secrets: `SONAR_TOKEN_DEV`, `SONAR_PROJECT_KEY_DEV`, `SONAR_ORGANIZATION_DEV`, `SONAR_HOST_URL_DEV`.

## Testing

- Run all tests: `dotnet test` (Tests project). Coverage example: `dotnet test --collect:"XPlat Code Coverage"`.
- In-memory EF used in tests (`Microsoft.EntityFrameworkCore.InMemory`); mocks via Moq; assertions via FluentAssertions.
- Integration tests (e.g., `HealthCheckTests`) use `WebApplicationFactory` and MUST mock sensitive configuration (like `Jwt:SecretKey`) via environment variables in constructor/dispose pattern to pass in CI/test environments without .env files.

## Conventions & Patterns

- Treat warnings as errors (see `Directory.Build.props` analyzers); keep code analyzer-friendly.
- Use Result pattern (`Application/Core/Result.cs`) for operation outcomes; prefer returning `Result<T>` from handlers and surface via controllers with standardized responses.
- Validation: FluentValidation validators per command (e.g., `CreateSerhanKitapCommandValidator`, `RegisterCommandValidator`); pipeline behavior enforces before handlers.
- Mapping: add AutoMapper profiles to `MappingProfiles`; commands/queries expect DTOs (e.g., `CreateSerhanKitapDto`, `RegisterDto`).
- Controllers inherit `BaseApiController` to access `Mediator` and standardized responses; prefer MediatR requests over direct service calls.
- Exception handling centralized via `ExceptionMiddleware`; rely on it instead of try/catch in controllers.
- Logging uses Serilog (Seq sink); configuration in `LoggingExtensions`; enrich logs rather than Console.WriteLine.
- **Identity & Auth:** Uses ASP.NET Core Identity (`ApplicationUser` extends `IdentityUser` with custom properties); JWT bearer authentication for API security.
- **JWT Configuration:** Loaded from environment variables (`Jwt__SecretKey`, `Jwt__Issuer`, `Jwt__Audience`, `Jwt__AccessTokenExpirationMinutes`, `Jwt__RefreshTokenExpirationDays`).
- **JWT Service:** `IJwtService` in Infrastructure creates access tokens (short-lived, default 60 min) and refresh tokens (long-lived, default 7 days, stored in `ApplicationUser.RefreshToken` field).
- **Current User Service:** `ICurrentUserService` provides access to current user's ID from HttpContext claims (`UserId` property).
- **Auth Handlers:** All auth commands/queries use `UserManager<ApplicationUser>` and `SignInManager<ApplicationUser>` for identity operations.

## Data & Migrations

- Connection string comes from `.env` variables; `DatabaseExtensions` configures EF Core and ensures migrations run at startup.
- Create migration: `dotnet ef migrations add <Name> -p Persistence -s API`; update DB: `dotnet ef database update -p Persistence -s API`.

## API Surface

- Primary resource: `SerhanKitap`; CRUD endpoints in `SerhanKitaplarController` (`/api/serhankitaplar`).
- Authentication: `AuthController` (`/api/v1/auth/`) endpoints:
  - `POST /register` - User registration (AllowAnonymous)
  - `POST /login` - User login (AllowAnonymous)
  - `POST /refresh-token` - Refresh access token (AllowAnonymous)
  - `GET /profile` - Get current user profile (Authorized)
  - `PUT /profile` - Update current user profile (Authorized)
- Health Checks: `/health/api` (liveness, no deps) and `/health/all` (readiness, includes DB/Seq); configured in `ApiExtensions`.
- Standard API responses serialized via `StandardApiResponse`; model state errors use `ModelStateResponseFactory`.
- Controllers and middleware wrap both success and error responses in `StandardApiResponse`; `BaseApiController.HandleResult` and `ExceptionMiddleware` enforce the envelope.

## How to Extend

- New feature: create feature folder under `Application/Features/<Feature>` with Commands/Queries, DTOs, validators, handlers; add mappings; expose via controller; ensure tests in parallel `Tests/Features` path.
- For features requiring authentication: inject `ICurrentUserService` in handlers to access current user; use `[Authorize]` attribute on controller or endpoint.
- For cross-cutting concerns, prefer extension classes under `API/Extensions` or pipeline behaviors.

## Agentic Usage (Copilot, Gemini CLI, Claude Code)

- All agents must follow this file; keep responses in English unless asked otherwise, and avoid Turkish-only output for code comments.
- Prefer `dotnet test` for validation; only hit the real database via Docker compose when intentionally running integration scenarios.
- When automating setup, copy `example.dev.env` → `dev.env` (never commit secrets) and start the stack with `docker-compose -f docker-compose.dev.yml up -d` if Postgres/Seq are needed.
- Use MediatR requests via controllers instead of bypassing handlers; new commands/queries must add validators and mappings in `MappingProfiles`.
- Keep EF Core migrations in `Persistence/Migrations`; run migrations with `dotnet ef ... -p Persistence -s API` rather than directly scripting SQL.
- Workflow: if requirements are unclear, ask the user first; then share a short plan before implementing; execute according to the plan and adjust only after confirming with the user when ambiguity remains.

## Gotchas

- Nullable reference types enabled; favor explicit null checks.
- Keep DTOs decoupled from EF entities; mapping handles transformations.
- Ensure new configuration values are read from `.env` and bound via extension methods or `EnvLoader`.
- Do NOT put secrets in `appsettings.json`; use `.env` files locally and Secrets in CI/CD.
- **JWT Secret Key** must be at least 64 bytes for production; generate with: `openssl rand -base64 64`
- Access tokens expire in 15 minutes; refresh tokens in 7 days (configurable via Jwt__* env vars).
- Swagger UI includes "Authorize" button for testing authenticated endpoints; requires valid JWT token.
