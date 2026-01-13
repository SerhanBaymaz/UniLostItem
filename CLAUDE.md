# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a .NET 9 Clean Architecture Web API project implementing CQRS with MediatR, Entity Framework Core, and PostgreSQL. The solution follows vertical slice architecture with features organized under `Application/Features/`.

**Layer Dependencies (outer to inner):** API -> Application -> Persistence -> Domain <- Infrastructure

**Existing Feature Modules:**

- `Application/Features/SerhanKitaplar` - CRUD operations for SerhanKitap entity
- `Application/Features/Auth` - Authentication module (Login, Register, RefreshToken, UpdateUserProfile, GetCurrentUser)

## Common Commands

### Build & Run

```bash
# Build solution
dotnet build

# Run API project (standard)
dotnet run --project API

# Run with hot reload (development)
dotnet watch run --project API

# Restore dependencies
dotnet restore
```

### Testing

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test Tests/Tests.csproj

# Run with coverage report
dotnet test --collect:"XPlat Code Coverage"

# Verbose test output
dotnet test --verbosity detailed
```

**Test Setup:**

- Uses `Microsoft.EntityFrameworkCore.InMemory` for database tests
- Mocking framework: Moq
- Assertions: FluentAssertions
- Integration tests use `WebApplicationFactory` - MUST mock sensitive config (e.g., `Jwt__SecretKey`) via environment variables in constructor/dispose pattern for CI/test environments without .env files

### Entity Framework Migrations

```bash
# Create new migration
dotnet ef migrations add <MigrationName> -p Persistence -s API

# Apply migrations to database
dotnet ef database update -p Persistence -s API

# Remove last migration
dotnet ef migrations remove -p Persistence -s API

# List migrations
dotnet ef migrations list -p Persistence -s API
```

### Docker Development

```bash
# Local Development environment (Just integrations runs on container ex: seq, postgreesql)
docker-compose -f docker-compose.local.yml up -d
docker-compose -f docker-compose.local.yml down

# Development environment
docker-compose -f docker-compose.dev.yml up -d
docker-compose -f docker-compose.dev.yml down

# Production environment
docker-compose -f docker-compose.prod.yml up -d
docker-compose -f docker-compose.prod.yml down
```

## Architecture Patterns

### Clean Architecture Layers

- **Domain/** - Core business entities with zero external dependencies
- **Application/** - Business logic, CQRS handlers, validation, mapping
- **Infrastructure/** - External services (JWT, file system, email, etc.)
- **Persistence/** - Data access via EF Core DbContext
- **API/** - Presentation layer (controllers, middleware, extensions)

### CQRS with MediatR

All operations follow the CQRS pattern. Each feature has:

- `Commands/` - Write operations (Create, Update, Delete)
- `Queries/` - Read operations (Get, List)
- `Common/DTOs/` - Shared data transfer objects

**Command/Query Pattern:**

```csharp
// Request implements IRequest<Result<T>>
public class CreateXCommand : IRequest<Result<string>>
{
    public required CreateXDto Dto { get; set; }
}

// Handler implements IRequestHandler<Request, Result<T>>
public class CreateXCommandHandler : IRequestHandler<CreateXCommand, Result<string>>
{
    // Handle method returns Result<T>
}
```

### Result Pattern

Use `Result<T>` from `Application/Core/Result.cs` for operation returns:

- `Result<T>.Success(message, value)` - Successful operation with data
- `Result<T>.Failure(message, code)` - Failed operation with error code

Controllers use `HandleResult<T>()` from `BaseApiController` to convert `Result<T>` to `StandardApiResponse<T>`.

### Validation with FluentValidation

- Validators inherit from `AbstractValidator<T>`
- `ValidationBehavior<TRequest, TResponse>` pipeline runs validators before handlers
- Registered in `ApplicationExtensions.cs`

### Extension-Based Configuration

`Program.cs` uses extension methods in `API/Extensions/` for clean organization:

- `AddApiConfiguration()` - Controllers, ModelState
- `AddApplicationServices()` - MediatR, AutoMapper, Validators
- `AddDatabaseServices()` - DbContext, migrations
- `AddIdentityServices()` - ASP.NET Core Identity
- `AddInfrastructureServices()` - JWT, CurrentUserService
- `AddSerilogConfiguration()` - Structured logging
- `AddSwaggerDocumentation()` - OpenAPI docs

### Environment Configuration

- `.env` files are loaded via `DotNetEnv` in `Program.cs`
- Use `dev.env` for local development
- Use `prod.env` for production
- Example files: `example.dev.env`, `example.prod.env`
- `ConfigurationExtensions.cs` maps env vars to configuration

### Health Checks

Two health endpoints:

- `/health/api` - Liveness (API only, no dependencies)
- `/health/all` - Readiness (API + PostgreSQL + Seq)

### Standard API Responses

All responses use `StandardApiResponse<T>` with:

- `SuccessResponse(data, message)`
- `ErrorResponse(message, statusCode, errorType, detail, path, traceId)`
- Model state errors use `ModelStateResponseFactory`

### API Surface

**Primary Resources:**

- `SerhanKitap` - CRUD endpoints in `SerhanKitaplarController` (`/api/v1/serhan-kitaplar`)

#### Pagination, Filtering & Sorting

The `GET /api/v1/serhan-kitaplar` endpoint supports advanced querying:

**Request DTO Pattern:**
- Controllers use dedicated Request DTOs (e.g., `GetSerhanKitaplarRequest`) with Data Annotations validation
- Parameters are bound via `[FromQuery]` and validated at the API layer

**Enum-Based Sorting:**
- Sort fields are defined as enums (e.g., `KitapSortField`) with Description attributes mapping to database field names
- This provides type safety and better Swagger/OpenAPI documentation

**Query Parameters:**

| Parameter | Type | Description |
|-----------|------|-------------|
| `pageNumber` | int | Page number (min: 1, default: 1) |
| `pageSize` | int | Page size (min: 1, max: 100, default: 10) |
| `sortBy` | enum? | Sort field (KitapName, KitapYazar, KitapSayfaSayisi) |
| `sortDescending` | bool | Sort descending (default: false) |
| `searchTerm` | string? | Search in both name and author fields |
| `kitapName` | string? | Filter by book name (contains) |
| `kitapYazar` | string? | Filter by author (contains) |
| `minPageCount` | int? | Minimum page count filter |
| `maxPageCount` | int? | Maximum page count filter |

**Response Metadata:**
```json
{
  "data": [...],
  "metadata": {
    "totalCount": 100,
    "pageNumber": 1,
    "pageSize": 10,
    "totalPages": 10,
    "hasNext": true,
    "hasPrevious": false
  }
}
```

**Swagger UI:**

- Available at `/swagger` in development
- Includes "Authorize" button for testing authenticated endpoints
- Requires valid JWT token

## Project Structure Notes

### Adding a New Feature

1. Create entity in `Domain/`
2. Add `DbSet<T>` to `AppDbContext.cs`
3. Create migration: `dotnet ef migrations add <Name> -p Persistence -s API`
4. Add feature folder under `Application/Features/<Feature>/`
5. Create Commands/Queries with Validators and DTOs
6. Add mappings to `Application/Core/MappingProfiles.cs`
7. Create Controller inheriting from `BaseApiController`
8. Inject handlers via MediatR in controller actions
9. Add parallel tests in `Tests/Features/<Feature>/`

#### Implementing Paginated List Queries with Filtering & Sorting

For list endpoints with pagination, filtering, and sorting:

1. **Create Sort Field Enum** (e.g., `KitapSortField.cs`):
   ```csharp
   public enum KitapSortField
   {
       [Description("fieldname")]
       FieldName
   }
   ```

2. **Create Request DTO** (e.g., `GetItemsRequest.cs` in `API/Controllers/Requests/`):
   ```csharp
   public class GetItemsRequest
   {
       [Range(1, int.MaxValue)]
       public int PageNumber { get; set; } = 1;

       [Range(1, 100)]
       public int PageSize { get; set; } = 10;

       public KitapSortField? SortBy { get; set; }
       public bool SortDescending { get; set; } = false;

       // Add filter properties...
   }
   ```

3. **Create Query** inheriting from `PagedAndSortedQueryBase`:
   ```csharp
   public record GetItemListQuery : PagedAndSortedQueryBase,
       IRequest<Result<PaginatedListDto<GetItemDto>>>
   {
       public string? SearchTerm { get; init; }
       // Add filter properties...
   }
   ```

4. **Create Validator** with pagination and filter rules

5. **Create Handler** that:
   - Builds `IQueryable` with filters
   - Applies sorting based on `SortBy` value (whitelist validated)
   - Uses pagination helper to create `PaginatedListDto<T>`

6. **Controller Action**:
   ```csharp
   [HttpGet]
   public async Task<ActionResult<StandardApiResponse<PaginatedListDto<GetItemDto>>>> GetItems(
       [FromQuery] GetItemsRequest request)
   {
       var query = new GetItemListQuery
       {
           PageNumber = request.PageNumber,
           PageSize = request.PageSize,
           SortBy = request.SortBy?.GetDescription(),
           // ... map other properties
       };
       return HandleResult(await Mediator.Send(query));
   }
   ```

**For features requiring authentication:**

- Inject `ICurrentUserService` in handlers to access current user
- Use `[Authorize]` attribute on controller or endpoint

### Authentication & Authorization

- ASP.NET Core Identity with `ApplicationUser` entity extending `IdentityUser`
- **ApplicationUser adds:** `FirstName`, `LastName`, `IsActive`, `RefreshToken`, `RefreshTokenExpiryTime`, `CreatedDate`, `LastLoginDate`, `ProfileImageUrl`
- JWT Bearer authentication via `IJwtService` in Infrastructure
- `[Authorize]` attribute on protected endpoints
- Current user accessed via `ICurrentUserService` (provides `UserId` from HttpContext claims)
- **JWT Configuration:** Loaded from environment variables:
  - `Jwt__SecretKey` - Must be at least 64 bytes (generate: `openssl rand -base64 64`)
  - `Jwt__Issuer` - Issuer identifier
  - `Jwt__Audience` - Audience identifier
  - `Jwt__AccessTokenExpirationMinutes` - Default 60 minutes (15 recommended for production)
  - `Jwt__RefreshTokenExpirationDays` - Default 7 days

**Auth Endpoints (`/api/v1/auth/`):**

- `POST /register` - User registration (AllowAnonymous)
- `POST /login` - User login (AllowAnonymous)
- `POST /refresh-token` - Refresh access token (AllowAnonymous)
- `GET /profile` - Get current user profile (Authorized)
- `PUT /profile` - Update current user profile (Authorized)

**Auth Handlers:** All auth commands/queries use `UserManager<ApplicationUser>` and `SignInManager<ApplicationUser>` for identity operations.

### CI/CD Pipelines

- `.github/workflows/ci-dev.yml` - Development pipeline (build, test, SonarCloud, Docker)
  - Triggers: push/PR on main/develop branches
  - Includes SonarCloud analysis
- `.github/workflows/ci-prod.yml` - Production pipeline (build, test, Docker)
  - Triggers: push on main or version tags (v*.*.*)
- Images pushed to GitHub Container Registry (GHCR)
  - Dev: `ghcr.io/serhanbaymaz/dev-templatedeneme:latest` and `:sha-<git-sha>`
  - Prod: `ghcr.io/serhanbaymaz/prod-templatedeneme:latest` and `:sha-<git-sha>`
- Dev secrets required: `SONAR_TOKEN_DEV`, `SONAR_PROJECT_KEY_DEV`, `SONAR_ORGANIZATION_DEV`, `SONAR_HOST_URL_DEV`

### Code Quality

- `TreatWarningsAsErrors` is enabled in `Directory.Build.props`
- Microsoft .NET Analyzers and SonarAnalyzer.CSharp enabled
- SonarCloud analysis runs on development pipeline

## Important Files

- `API/Program.cs` - Application entry point with extension-based configuration
- `Application/Core/Result.cs` - Result pattern implementation
- `Application/Core/MappingProfiles.cs` - AutoMapper profiles for DTO mapping
- `Application/Core/ValidationBehavior.cs` - MediatR validation pipeline
- `Application/Core/Pagination/PagedAndSortedQueryBase.cs` - Base class for paginated queries
- `Application/Core/Pagination/PaginatedListDto.cs` - Pagination response DTO with metadata
- `Application/Core/Extensions/EnumExtensions.cs` - Extension methods for enum Description attribute
- `API/Controllers/BaseApiController.cs` - Base controller with MediatR and Result handling
- `API/Controllers/Requests/` - Request DTOs for API endpoints with Data Annotations
- `API/Middleware/ExceptionMiddleware.cs` - Centralized exception handling
- `Persistence/AppDbContext.cs` - EF Core DbContext (extends `IdentityDbContext<ApplicationUser>`)
- `Persistence/IAppDbContext.cs` - DbContext interface for mocking
- `Persistence/DbInitializer.cs` - Database seeding (called on startup, seeds `BaseUser` role)
- `Infrastructure/Security/JwtService.cs` - JWT token generation/validation (implements `IJwtService`)
- `Infrastructure/Services/CurrentUserService.cs` - Current user from claims (implements `ICurrentUserService`)
- `Directory.Build.props` - Centralized package references and analyzer configuration

## Gotchas

- **Nullable reference types** are enabled - favor explicit null checks
- **Keep DTOs decoupled from EF entities** - mapping handles transformations
- **Environment configuration** - ensure new config values are read from `.env` and bound via extension methods or `EnvLoader`
- **Never commit secrets** - do NOT put secrets in `appsettings.json`; use `.env` files locally and Secrets in CI/CD
- **JWT Secret Key** must be at least 64 bytes for production; generate with: `openssl rand -base64 64`
- **Swagger UI** includes "Authorize" button for testing authenticated endpoints; requires valid JWT token
- **Exception handling** is centralized via `ExceptionMiddleware` - rely on it instead of try/catch in controllers
- **Logging** uses Serilog with Seq sink - enrich logs rather than `Console.WriteLine`
- **Pagination sorting** - Always whitelist sort fields at the handler level to prevent SQL injection, even when using enums
- **Request DTO validation** - Data Annotations validate at API layer, FluentValidation at Application layer (defense in depth)
- **SortBy null handling** - When `SortBy` is null, handlers should apply a default sort order (usually by name or ID)

## Agentic Workflow

- If requirements are unclear, ask the user first
- Share a short plan before implementing
- Execute according to the plan and adjust only after confirming with the user when ambiguity remains
- Prefer `dotnet test` for validation; only hit the real database via Docker compose when intentionally running integration scenarios
- Use MediatR requests via controllers instead of bypassing handlers
- New commands/queries must add validators and mappings in `MappingProfiles`
- Keep EF Core migrations in `Persistence/Migrations`; run migrations with `dotnet ef ... -p Persistence -s API` rather than directly scripting SQL
