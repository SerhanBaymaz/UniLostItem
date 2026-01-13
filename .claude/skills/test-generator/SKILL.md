---
name: test-generator
description: Analyzes git status to identify files needing unit tests, checks existing test coverage, and generates missing tests following project patterns.
allowed-tools: Read, Write, Edit, Glob, Grep, Bash
---

# Test Generator - Automated Unit Test Coverage

## Overview

This Skill analyzes changed files from git status, identifies missing or incomplete unit test coverage, and generates tests following the project's testing patterns.

**Test Framework Used:**

- xUnit as test framework
- Moq for mocking dependencies
- FluentAssertions for readable assertions
- Microsoft.EntityFrameworkCore.InMemory for database tests

## Workflow

### Step 1: Identify Changed Files

Run `git status --porcelain` to identify modified/new files. Filter for:

- `Application/**/*.cs` - Handlers, Commands, Queries, Validators, DTOs
- `API/Controllers/*.cs` - API Controllers
- `API/**/*.cs` - Middleware, Extensions, Helpers
- `Infrastructure/**/*.cs` - Services
- `Persistence/**/*.cs` - DbContext, DbInitializer
- `Domain/**/*.cs` - Entities

### Step 2: Determine Test Requirements

For each changed file, determine what tests are needed:

| File Type | Test Location | Test Coverage |
| ----------- | --------------- | --------------- |
| Command Handler | `Tests/Application_Tests/Features/{Feature}/Commands/{Operation}/` | Success path, Failure path, Edge cases |
| Query Handler | `Tests/Application_Tests/Features/{Feature}/Queries/{Operation}/` | Success path, Empty results, NotFound |
| Validator | `Tests/Application_Tests/Features/{Feature}/Commands/{Operation}/` | Valid cases, Invalid cases, Null checks |
| Controller | `Tests/API_Tests/Controllers/` | Each endpoint, Auth requirements, Model validation |
| Service | `Tests/Infrastructure_Tests/` or `Tests/Application_Tests/Services/` | All public methods, Error handling |
| Middleware | `Tests/API_Tests/Middleware/` | Invoke logic, Exception handling |
| Extension | `Tests/API_Tests/Extensions/` | Each extension method |

### Step 3: Check Existing Tests

Use `Glob` to find existing test files:

```text
Tests/**/Tests/{FileNameWithoutExtension}Tests.cs
```

For existing tests, read and analyze what test cases exist. Identify gaps:

- Missing happy path tests
- Missing error/failure paths
- Missing edge cases
- Missing validation tests

### Step 4: Generate Tests

Generate test files following existing patterns. See [patterns.md](patterns.md) for detailed templates.

### Step 5: Run Tests

```bash
dotnet test
```

Report any failures and fix them.

## Test File Naming Convention

```text
{ClassName}Tests.cs
```

Examples:

- `CreateProductCommandHandlerTests.cs`
- `AuthControllerTests.cs`
- `JwtServiceTests.cs`

## Test Structure Template

```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;

namespace Tests.{TestFolder};

public class {ClassName}Tests : IDisposable
{
    private readonly {Dependencies} _dependency;
    private readonly {SystemUnderTest} _sut;

    public {ClassName}Tests()
    {
        // Arrange - Setup mocks and SUT
    }

    [Fact]
    public async Task {MethodName}_{Scenario}_{ExpectedOutcome}()
    {
        // Arrange
        // Act
        // Assert
    }

    public void Dispose()
    {
        // Cleanup
    }
}
```

## Common Test Patterns

See [patterns.md](patterns.md) for:

- Handler test patterns (Command/Query)
- Validator test patterns
- Controller test patterns
- Service test patterns
- Mock setup patterns

## Important Notes

1. **Always implement IDisposable** for tests that create in-memory databases
2. **Use unique database names** for each test to avoid interference
3. **Follow AAA pattern** (Arrange-Act-Assert)
4. **Use descriptive test names** that describe scenario and outcome
5. **Mock external dependencies** (DbContext, AutoMapper, HttpContext, etc.)
6. **Test both success and failure paths**
7. **Use FluentAssertions** for readable assertions
8. **Verify method calls** on mocks when appropriate
