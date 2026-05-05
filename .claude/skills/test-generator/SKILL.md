---
name: test-generator
description: Analyzes git status to identify files needing unit tests, checks existing test coverage, and generates missing tests following UniLostItem project patterns.
allowed-tools: Read, Write, Edit, Glob, Grep, Bash
---

# Test Generator — UniLostItem Automated Test Coverage

## Overview

Analyzes changed files from git status, identifies missing or incomplete unit test coverage, and generates tests following the project's established patterns.

## Test Infrastructure

- **xUnit** — test framework
- **FluentAssertions** — readable assertions
- **Moq** — mocking `ICurrentUserService`, `IMapper` (NOT `IAppDbContext` or `DbSet<T>`)
- **Microsoft.Data.Sqlite** — in-memory database via `TestDbContextFactory`
- **Real `AppDbContext`** — NOT `Mock<IAppDbContext>` (Moq cannot mock EF Core extension methods)

## Workflow

### Step 1: Identify Changed Files

```bash
git status --porcelain
```

Filter for:
- `Application/Features/**/Commands/**/*.cs` — Handlers, Commands, DTOs, Validators
- `Application/Features/**/Queries/**/*.cs` — Handlers, Queries, DTOs
- `Domain/**/*.cs` — Entity changes (may require test updates)

### Step 2: Determine Test Requirements

| File Type | Test File | Test Coverage |
|-----------|-----------|---------------|
| Command Handler | `Tests/Application_Tests/Features/{Feature}/Commands/{Op}/{Op}HandlerTests.cs` | Success, NotFound(404), Forbidden(403), BadRequest(400) |
| Query Handler | `Tests/Application_Tests/Features/{Feature}/Queries/{Op}/{Op}HandlerTests.cs` | Success, Empty, Filter, SoftDelete, Pagination, Sort |
| Validator | `Tests/Application_Tests/Features/{Feature}/Commands/{Op}/{Op}ValidatorTests.cs` | Valid, NullDto, EmptyField, MaxLength, MultipleErrors |

### Step 3: Check Existing Tests

Use `Glob` to find existing test files under `Tests/Application_Tests/Features/`. For existing tests, identify gaps:
- Missing success/failure paths
- Missing ownership (403) tests
- Missing validator edge cases

### Step 4: Generate Tests

Generate test files following the patterns in [patterns.md](patterns.md).

**Critical rules:**
1. Use `TestDbContextFactory.CreateInMemoryDbContext()` — NOT `Mock<IAppDbContext>`
2. Seed FK-referenced entities (ApplicationUser) before adding entities with FKs
3. Use `Mock<ICurrentUserService>` for user identity
4. Use `Mock<IMapper>` only for Create handlers (Update handlers use property-by-property mapping)
5. Use `CreateContextWithItem()` helper for Update/Delete tests needing pre-seeded data
6. Always use braces `{ }` on if/else
7. Assert Turkish error messages

### Step 5: Build & Run Tests

```bash
dotnet build UniLostItem.sln
dotnet test
```

## Test File Naming

```
{ClassName}Tests.cs
```

Examples:
- `CreateLostItemCommandHandlerTests.cs`
- `UpdateLostItemCommandValidatorTests.cs`
- `GetLostItemListQueryHandlerTests.cs`

## Test Structure

```csharp
using Application.Core;
using Application.Interfaces;
using Domain;
using FluentAssertions;
using Moq;
using Persistence;
using Tests.Helpers;

namespace Tests.Application_Tests.Features.{Feature}.Commands.{Operation};

public class {Operation}HandlerTests
{
    private readonly AppDbContext _context;
    // ... dependencies

    public {Operation}HandlerTests()
    {
        _context = TestDbContextFactory.CreateInMemoryDbContext();
        // ... setup mocks
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenValid()
    {
        // Arrange
        // Act
        // Assert
    }
}
```

## Important Notes

1. **No `Mock<IAppDbContext>`** — Moq cannot mock `FirstOrDefaultAsync` (extension method)
2. **No `Mock<DbSet<T>>`** — same reason
3. **No `IDisposable`** — just fresh context per test class
4. **FK constraints are real** — SQLite in-memory enforces them; seed ApplicationUser before entities with UserId
5. **Test names** — follow `Handle_ShouldReturn{Code}_When{Scenario}` pattern
6. **No AutoMapper for Update** — Update handlers use property-by-property mapping
