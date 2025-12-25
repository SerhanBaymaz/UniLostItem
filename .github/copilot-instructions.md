# Agentic Instructions (Copilot, Gemini CLI, Claude Code) for TemplateDeneme

## Core Architecture

- Clean Architecture with four layers: Domain (pure entities), Application (CQRS + MediatR), Persistence (EF Core + PostgreSQL), API (ASP.NET Core controllers, middleware, Swagger).
- Feature slices under `Application/Features/SerhanKitaplar` use Commands/Queries folders with DTOs, validators, and handlers; mapping in `Application/Core/MappingProfiles.cs`; validation pipeline via `ValidationBehavior`.
- Persistence layer uses `AppDbContext` implementing `IAppDbContext`; migrations live in `Persistence/Migrations`; database seeding in `DbInitializer` (called on startup).
- API startup is extension-driven (`API/Extensions/*`) wired in `API/Program.cs`; custom error handling in `API/Middleware/ExceptionMiddleware.cs`; standardized responses in `API/Responses`.

## Build & Run

- Restore/build/run: `dotnet restore`, `dotnet build`, `dotnet run --project API` (hot reload: `dotnet watch run --project API`).
- Docker dev stack: `docker-compose -f docker-compose.dev.yml up -d`; default ports: API 8089, PostgreSQL 5432, Seq 8088 (UI) / 5348 (ingestion).
- Environment files: copy `example.dev.env` → `dev.env` (or `example.prod.env` → `prod.env`); `ConfigurationExtensions` loads `.env` early.

## Testing

- Run all tests: `dotnet test` (Tests project). Coverage example: `dotnet test --collect:"XPlat Code Coverage"`.
- In-memory EF used in tests (`Microsoft.EntityFrameworkCore.InMemory`); mocks via Moq; assertions via FluentAssertions.

## Conventions & Patterns

- Treat warnings as errors (see `Directory.Build.props` analyzers); keep code analyzer-friendly.
- Use Result pattern (`Application/Core/Result.cs`) for operation outcomes; prefer returning `Result<T>` from handlers and surface via controllers with standardized responses.
- Validation: FluentValidation validators per command (e.g., `CreateSerhanKitapCommandValidator`); pipeline behavior enforces before handlers.
- Mapping: add AutoMapper profiles to `MappingProfiles`; commands/queries expect DTOs (e.g., `CreateSerhanKitapDto`, `GetSerhanKitapDto`).
- Controllers inherit `BaseApiController` to access `Mediator` and standardized responses; prefer MediatR requests over direct service calls.
- Exception handling centralized via `ExceptionMiddleware`; rely on it instead of try/catch in controllers.
- Logging uses Serilog (Seq sink); configuration in `LoggingExtensions`; enrich logs rather than Console.WriteLine.

## Data & Migrations

- Connection string comes from `.env` variables; `DatabaseExtensions` configures EF Core and ensures migrations run at startup.
- Create migration: `dotnet ef migrations add <Name> -p Persistence -s API`; update DB: `dotnet ef database update -p Persistence -s API`.

## API Surface

- Primary resource: `SerhanKitap`; CRUD endpoints in `SerhanKitaplarController` (`/api/serhankitaplar`).
- Standard API responses serialized via `StandardApiResponse`; model state errors use `ModelStateResponseFactory`.
- Controllers and middleware wrap both success and error responses in `StandardApiResponse`; `BaseApiController.HandleResult` and `ExceptionMiddleware` enforce the envelope.

## How to Extend

- New feature: create feature folder under `Application/Features/<Feature>` with Commands/Queries, DTOs, validators, handlers; add mappings; expose via controller; ensure tests in parallel `Tests/Features` path.
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
- Ensure new configuration values are read from `.env` and bound via extension methods.
