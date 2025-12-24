# TemplateDeneme Assistant Instructions

- Codebase: .NET 9 Clean Architecture Web API with CQRS + MediatR, EF Core (PostgreSQL), AutoMapper, FluentValidation, Serilog + Seq.
- Layers: Domain (entities/rules), Application (CQRS handlers, validators, mapping, Result), Persistence (EF Core context/migrations/seeding), API (controllers, middleware, responses, Swagger). Keep controllers thin; no EF entities returned directly.
- Config: appsettings + .env via DotNetEnv. Use example.dev.env/example.prod.env to derive dev.env/prod.env; never commit secrets. Docker Compose stacks: docker-compose.dev.yml (API 8089, Seq 8088, Postgres 5432), docker-compose.prod.yml (API 8080, Seq 8081, Postgres 5433).
- Error/Responses: ExceptionMiddleware + AppProblemDetails for errors; ModelStateResponseFactory for validation; StandardApiResponse for success. Maintain consistency across new endpoints.
- Testing: xUnit + FluentAssertions + Moq + EFCore.InMemory. Mirror feature folder structure under Tests/. Run dotnet test (or focused assemblies) before PRs; TreatWarningsAsErrors is on.
- Conventions: C# 13 nullable enabled; DI over statics; async suffix; logger messages structured (no string concat). Keep analyzers clean (Microsoft + Sonar). Update migrations when schema changes and keep seeds aligned.
- How to run:
  - Restore: dotnet restore
  - Dev stack: docker-compose -f docker-compose.dev.yml up -d
  - Migrations (local): dotnet ef database update -p Persistence -s API
  - API: dotnet run --project API
  - Tests: dotnet test
- OpenSpec: For new capabilities or breaking/architectural changes, create a change under openspec/changes/<id>/ with proposal/tasks/spec deltas and validate with openspec validate --strict before implementation.
