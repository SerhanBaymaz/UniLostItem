# CLAUDE.md

This is **UniLostItem** — a .NET 9 Clean Architecture Web API with CQRS (MediatR), EF Core, PostgreSQL.

**Layers:** API -> Application -> Persistence -> Domain <- Infrastructure

**Features:** `Auth` (Login/Register/Profile), `LostItems` (CRUD+Queries), `ItemClaims` (Workflow Commands+Queries)

## Commands

```bash
dotnet build UniLostItem.sln        # Build
dotnet test                        # Run all tests (391 tests)
dotnet ef migrations add X -p Persistence -s API   # Add migration
dotnet ef database update -p Persistence -s API     # Apply migration
```

## Formatting & Quality
- **Husky.Net:** `dotnet tool restore && dotnet husky install` (auto-formats staged files on commit).

## Generating New Features & Tests

- **CRUD scaffolding:** Use `/crud-complete` skill — generates entity, commands, queries, handler, validator, controller, mapping, and tests following the project's established patterns (BaseEntity, ICurrentUserService, soft delete, ownership checks, Turkish messages, SQLite in-memory tests)
- **Test generation:** Use `/test-generator` skill — analyzes git diff, identifies coverage gaps, generates tests matching project patterns (TestDbContextFactory, Mock<ICurrentUserService>, FK seeding)

## Architecture

### CQRS Pattern

```
Commands/  — Create{X}Command, Handler, Dto, Validator
Queries/   — Get{X}List/DetailsQuery, Handler, Dto, Validator (optional)
Common/    — DTOs, Enums (sort fields)
```

- Commands return `Result<string>` (create) or `Result<Unit>` (update/delete)
- Handlers inject `IAppDbContext`, `ICurrentUserService`, optionally `IMapper`
- Validators use `When(x => x.Dto != null, () => { ... })` pattern with Turkish messages

### Result Pattern

`Result<T>.Success(message, value)` / `Result<T>.Failure(message, code)` — codes: 400, 403, 404, 410, 423

### Key Patterns

- **BaseEntity** — all business entities inherit from it (Id, CreatedDate, CreatedBy, UpdatedDate, UpdatedBy, IsDeleted, IsActive). `ApplicationUser` does NOT (extends IdentityUser)
- **Soft delete** — `IsDeleted = true`, not physical removal
- **Ownership** — Update/Delete check `entity.UserId != currentUserId` → 403
- **Update handlers** — property-by-property mapping (NOT AutoMapper)
- **Query projections** — `.Select()` for computed fields (UserFullName, ClaimCount), NOT AutoMapper
- **Paginated queries** — inherit `PagedAndSortedQueryBase<TSortEnum>`, use `PaginatedListDto<T>`
- **AutoMapper** — only for Create DTO→Entity mapping; use `IgnoreAllBaseEntityProperties()` + `.Ignore()` for each navigation property

### Auth

- JWT Bearer via `IJwtService`, current user via `ICurrentUserService.UserId`
- `[Authorize]` on protected endpoints, `[AllowAnonymous]` for public ones

## API Surface

| Controller | Route | Auth |
|-----------|-------|------|
| LostItemsController | `/api/v1/items` | List/Details: anonymous, CUD: `[Authorize]` |
| ItemClaimsController | `/api/v1/claims` | List: `[Authorize]`, Pending: `[Authorize(Roles="Admin")]`, AdminReview: `[Authorize(Roles="Admin")]` |

## Test Setup

- `TestDbContextFactory.CreateInMemoryDbContext()` — real SQLite in-memory (NOT `Mock<IAppDbContext>`)
- `Mock<ICurrentUserService>` for user identity
- Seed FK-referenced entities (ApplicationUser) before entities with FKs
- Moq cannot mock extension methods — use real DB, not `Mock<DbSet<T>>`

## Configuration

- `.env` files via `DotNetEnv` (`dev.env`, `prod.env`)
- Extension-based setup in `Program.cs` (AddDatabaseServices, AddIdentityServices, AddInfrastructureServices, etc.)
- Swagger at `/swagger` in development
- Health: `/health/api` (liveness), `/health/all` (readiness)

## Gotchas

- **Always use braces `{ }`** on if/else, even single-line bodies
- **Nullable reference types** enabled — explicit null checks
- **Never commit secrets** — use `.env` files
- **Soft delete filtering** — list queries must filter `!IsDeleted`; NOT automatic at DbContext level
- **Navigation properties** — must `.Ignore()` in AutoMapper, `.Include()` in EF queries
- **DateTime consistency** — always `DateTime.UtcNow`
- **No comments** unless the WHY is non-obvious

## Agentic Workflow

- Ask first if requirements are unclear
- Share a short plan before implementing
- Use `/crud-complete` and `/test-generator` skills for feature/test generation
- `dotnet test` for validation; Docker only for integration scenarios
- EF migrations via `dotnet ef ... -p Persistence -s API`, not raw SQL
- **Coverage on New Code (required ≥ 80%)** — All new/modified code must have ≥80% test coverage (SonarQube quality gate)
