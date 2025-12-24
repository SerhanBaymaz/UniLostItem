# Project Context

## Purpose
TemplateDeneme is a .NET 9 Clean Architecture Web API used as a baseline for building maintainable services. It demonstrates CQRS with MediatR, validation, mapping, and persistence patterns through the SerhanKitap CRUD feature and serves as a starter for new APIs.

## Tech Stack
- .NET 9, C# 13
- ASP.NET Core Web API
- CQRS with MediatR
- Entity Framework Core 9 with Npgsql (PostgreSQL)
- AutoMapper, FluentValidation
- Serilog + Seq
- Docker and Docker Compose
- xUnit, FluentAssertions, Moq, EFCore.InMemory, coverlet.collector
- Static analysis: Microsoft.CodeAnalysis.NetAnalyzers, SonarAnalyzer.CSharp (warnings treated as errors)

## Project Conventions

### Code Style
- C# 13 with nullable enabled and implicit usings; warnings-as-errors enforced via Directory.Build.props.
- Keep controllers thin; business logic in Application handlers; no EF entities returned from API.
- Use DTOs with AutoMapper; responses use StandardApiResponse on success and AppProblemDetails for errors.
- Inputs validated with FluentValidation; pipeline behaviors handle cross-cutting concerns (e.g., ValidationBehavior).
- Structured logging via Serilog with enrichment (correlation IDs when available); avoid string-concatenated logs.
- Configuration comes from appsettings plus .env (DotNetEnv); never commit real secrets—only example.*.env files.
- Naming: PascalCase for types/members, async methods use Async suffix, DI over statics/singletons.

### Architecture Patterns
- Clean Architecture layering: Domain (entities/rules), Application (CQRS handlers, validators, mapping, Result), Persistence (EF Core DbContext, migrations, seeding), API (controllers, middleware, responses, Swagger).
- CQRS: Commands mutate state; Queries are read-only. Each request has a MediatR handler and a FluentValidation validator.
- ExceptionMiddleware standardizes errors to AppProblemDetails; ModelStateResponseFactory unifies validation errors.
- Database: PostgreSQL via EF Core; migrations live in Persistence/Migrations and run at startup with DbInitializer.
- API surface: REST endpoints under /api; Swagger/OpenAPI kept in sync with request/response contracts.
- Deployment: Docker Compose defines dev/prod stacks (API, Postgres, Seq); configs stay aligned with appsettings and *.env files.

### Testing Strategy
- Tests live in Tests/ mirroring feature paths. Use xUnit + FluentAssertions; Moq for external boundary mocks; EFCore.InMemory for data-focused tests.
- Add unit tests for Application/Domain logic and validators; add integration tests for API/persistence when behavior or data changes.
- Run `dotnet test` before PRs; coverage optional via coverlet collector. Keep analyzer warnings at zero.
- Prefer red-green-refactor; document any test omissions with rationale.

### Git Workflow
- Default branch: main. Develop on short-lived feature branches (e.g., feature/<slug> or change-id from OpenSpec).
- For new capabilities or breaking/architectural changes, create an OpenSpec change with a verb-led kebab-case change-id and draft proposal/tasks/spec deltas.
- PR expectations: include tests and EF migrations when contracts or schemas change; keep README/env examples in sync. Run `openspec validate --strict` when proposals exist.
- Commit messages are concise (e.g., feat/fix/chore) and grouped per logical change.

## Domain Context
- Core entity: SerhanKitap (Id string Guid, KitapName, KitapYazar, KitapSayfaSayisi).
- Feature: CRUD over `/api/serhankitaplar` with DTOs and AutoMapper mappings in Application/Features/SerhanKitaplar.
- Responses standardized via StandardApiResponse/AppProblemDetails; validation handled centrally.

## Important Constraints
- TreatWarningsAsErrors enforced; analyzer warnings must be fixed or justified.
- No secrets in the repo; use dev.env/prod.env derived from example.*.env. Keep appsettings and compose files consistent.
- CORS limited to development defaults (localhost:3000) unless reviewed.
- Database schema changes require EF migrations plus rollback notes; migrations run at startup.
- Logging and exception middleware must remain enabled; API must not return EF entities directly.

## External Dependencies
- PostgreSQL (Dockerized in dev/prod compose).
- Seq for structured log ingestion/viewing.
- Docker/Docker Compose for environment orchestration.
- DotNetEnv for environment variable loading.
