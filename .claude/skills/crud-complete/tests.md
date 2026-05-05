# Test Templates — UniLostItem

Unit test templates matching the actual test patterns from `LostItems` and `ItemClaims` features.

## Test Infrastructure

- **Framework:** xUnit
- **Assertions:** FluentAssertions
- **Mocking:** Moq
- **Database:** `Microsoft.Data.Sqlite` in-memory via `TestDbContextFactory`
- **No IDisposable** — fresh context per test class

## Critical Rules

1. **Use real `AppDbContext`** (NOT `Mock<IAppDbContext>`) — Moq cannot mock EF Core extension methods like `FirstOrDefaultAsync`
2. **Use `TestDbContextFactory.CreateInMemoryDbContext()`** for DB creation
3. **Seed FK-referenced entities** — SQLite enforces FK constraints even in-memory
4. **Use `Mock<ICurrentUserService>`** for user identity mocking
5. **Always use braces `{ }`** on if/else statements

## Test File Location

```
Tests/Application_Tests/Features/{Feature}/Commands/{Operation}/{Operation}HandlerTests.cs
Tests/Application_Tests/Features/{Feature}/Queries/{Operation}/{Query}HandlerTests.cs
Tests/Application_Tests/Features/{Feature}/Commands/{Operation}/{Operation}ValidatorTests.cs
```

## Handler Test Patterns

### Create Handler Tests

**Key:** Seed user in DB (FK constraint), Mock IMapper + ICurrentUserService

```csharp
using Application.Core;
using Application.Features.{Feature}.Commands.Create{Entity};
using Application.Interfaces;
using AutoMapper;
using Domain;
using FluentAssertions;
using Moq;
using Persistence;
using Tests.Helpers;

namespace Tests.Application_Tests.Features.{Feature}.Commands.Create{Entity};

public class Create{Entity}CommandHandlerTests
{
    private readonly AppDbContext _context;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ICurrentUserService> _currentUserMock;
    private readonly Create{Entity}CommandHandler _handler;

    public Create{Entity}CommandHandlerTests()
    {
        _context = TestDbContextFactory.CreateInMemoryDbContext();
        _mapperMock = new Mock<IMapper>();
        _currentUserMock = new Mock<ICurrentUserService>();
        _currentUserMock.Setup(x => x.UserId).Returns("user-1");
        _handler = new Create{Entity}CommandHandler(_context, _mapperMock.Object, _currentUserMock.Object);
    }

    private async Task SeedUser()
    {
        var user = new ApplicationUser
        {
            Id = "user-1",
            UserName = "testuser",
            Email = "test@test.com",
            EmailConfirmed = true,
            FirstName = "Test",
            LastName = "User"
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
    }

    [Fact]
    public async Task Handle_ShouldCreateEntity_WhenValid()
    {
        await SeedUser();

        var dto = new Create{Entity}Dto { Property1 = "Test" };
        var command = new Create{Entity}Command { Create{Entity}Dto = dto };

        _mapperMock.Setup(m => m.Map<{Entity}>(dto)).Returns(new {Entity} { Property1 = "Test" });

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Handle_ShouldSetUserId_FromCurrentUserService()
    {
        await SeedUser();

        var dto = new Create{Entity}Dto { Property1 = "Test" };
        var command = new Create{Entity}Command { Create{Entity}Dto = dto };

        _mapperMock.Setup(m => m.Map<{Entity}>(dto)).Returns(new {Entity} { Property1 = "Test" });

        await _handler.Handle(command, CancellationToken.None);

        var savedEntity = await _context.{Entity}Plural.FirstOrDefaultAsync();
        savedEntity.Should().NotBeNull();
        savedEntity!.UserId.Should().Be("user-1");
    }

    [Fact]
    public async Task Handle_ShouldCallMapper()
    {
        await SeedUser();

        var dto = new Create{Entity}Dto { Property1 = "Test" };
        var command = new Create{Entity}Command { Create{Entity}Dto = dto };

        _mapperMock.Setup(m => m.Map<{Entity}>(dto)).Returns(new {Entity} { Property1 = "Test" });

        await _handler.Handle(command, CancellationToken.None);

        _mapperMock.Verify(m => m.Map<{Entity}>(dto), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenSaveFails()
    {
        await SeedUser();

        var dto = new Create{Entity}Dto { Property1 = "Test" };
        var command = new Create{Entity}Command { Create{Entity}Dto = dto };

        _mapperMock.Setup(m => m.Map<{Entity}>(dto)).Returns(new {Entity} { Property1 = "Test" });

        await _context.DisposeAsync(); // Simulate save failure

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Code.Should().Be(400);
    }
}
```

### Update/Delete Handler Tests (with ownership)

**Key:** Use `CreateContextWithItem()` helper to seed user + entity together

```csharp
public class Update{Entity}CommandHandlerTests
{
    private const string OwnerUserId = "owner-id";
    private const string OtherUserId = "other-id";

    private async Task<AppDbContext> CreateContextWithItem(string userId = OwnerUserId)
    {
        var context = TestDbContextFactory.CreateInMemoryDbContext();
        context.Users.Add(new ApplicationUser
        {
            Id = userId,
            UserName = "owner",
            Email = "owner@test.com",
            EmailConfirmed = true,
            FirstName = "Owner",
            LastName = "User"
        });
        context.{Entity}Plural.Add(new {Entity}
        {
            Id = "item-1",
            Property1 = "Original",
            UserId = userId,
            IsActive = true
        });
        await context.SaveChangesAsync();
        return context;
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenItemDoesNotExist()
    {
        var context = TestDbContextFactory.CreateInMemoryDbContext();
        var currentUserMock = new Mock<ICurrentUserService>();
        currentUserMock.Setup(x => x.UserId).Returns(OwnerUserId);

        var handler = new Update{Entity}CommandHandler(context, currentUserMock.Object);

        var command = new Update{Entity}Command
        {
            Id = "non-existent",
            Update{Entity}Dto = new Update{Entity}Dto { Property1 = "Updated" }
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Code.Should().Be(404);
    }

    [Fact]
    public async Task Handle_ShouldReturnForbidden_WhenUserIsNotOwner()
    {
        var context = await CreateContextWithItem(OwnerUserId);
        var currentUserMock = new Mock<ICurrentUserService>();
        currentUserMock.Setup(x => x.UserId).Returns(OtherUserId);

        var handler = new Update{Entity}CommandHandler(context, currentUserMock.Object);

        var command = new Update{Entity}Command
        {
            Id = "item-1",
            Update{Entity}Dto = new Update{Entity}Dto { Property1 = "Hacked" }
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Code.Should().Be(403);
    }

    [Fact]
    public async Task Handle_ShouldUpdateItem_WhenUserIsOwner()
    {
        var context = await CreateContextWithItem(OwnerUserId);
        var currentUserMock = new Mock<ICurrentUserService>();
        currentUserMock.Setup(x => x.UserId).Returns(OwnerUserId);

        var handler = new Update{Entity}CommandHandler(context, currentUserMock.Object);

        var command = new Update{Entity}Command
        {
            Id = "item-1",
            Update{Entity}Dto = new Update{Entity}Dto { Property1 = "New Value" }
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var item = await context.{Entity}Plural.FindAsync("item-1");
        item!.Property1.Should().Be("New Value");
        item.UpdatedBy.Should().Be(OwnerUserId);
    }
}
```

### Query Handler Tests

**Key:** No mocking needed — use real DB directly

```csharp
public class Get{Entity}ListQueryHandlerTests
{
    private readonly AppDbContext _context;

    public Get{Entity}ListQueryHandlerTests()
    {
        _context = TestDbContextFactory.CreateInMemoryDbContext();
    }

    private async Task SeedData()
    {
        var user = new ApplicationUser
        {
            Id = "user-1",
            UserName = "testuser",
            Email = "test@test.com",
            EmailConfirmed = true,
            FirstName = "Test",
            LastName = "User"
        };
        _context.Users.Add(user);

        var items = new List<{Entity}>
        {
            new() { Title = "Alpha", UserId = "user-1", IsActive = true },
            new() { Title = "Beta", UserId = "user-1", IsActive = true }
        };
        _context.{Entity}Plural.AddRange(items);
        await _context.SaveChangesAsync();
    }

    [Fact]
    public async Task Handle_ShouldReturnAllActiveItems() { ... }
    [Fact]
    public async Task Handle_ShouldFilterByProperty() { ... }
    [Fact]
    public async Task Handle_ShouldExcludeSoftDeleted() { ... }
    [Fact]
    public async Task Handle_ShouldReturnPaginationMetadata() { ... }
    [Fact]
    public async Task Handle_ShouldSortByTitleAscending() { ... }
}
```

### Validator Tests

```csharp
public class Create{Entity}CommandValidatorTests
{
    private readonly Create{Entity}CommandValidator _validator = new();

    [Fact]
    public void Should_HaveError_WhenDtoIsNull()
    {
        var command = new Create{Entity}Command { Create{Entity}Dto = null! };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Create{Entity}Dto);
    }

    [Fact]
    public void Should_HaveError_WhenTitleIsEmpty()
    {
        var command = new Create{Entity}Command
        {
            Create{Entity}Dto = new Create{Entity}Dto { Title = "" }
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Create{Entity}Dto!.Title);
    }

    [Fact]
    public void Should_NotHaveError_WhenAllPropertiesValid()
    {
        var command = new Create{Entity}Command
        {
            Create{Entity}Dto = new Create{Entity}Dto
            {
                Title = "Valid Title",
                Description = "Valid Description"
            }
        };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_HaveMultipleErrors_WhenMultipleFieldsInvalid()
    {
        var command = new Create{Entity}Command
        {
            Create{Entity}Dto = new Create{Entity}Dto { Title = "" }
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Create{Entity}Dto!.Title);
        result.ShouldHaveValidationErrorFor(x => x.Create{Entity}Dto!.Description);
    }
}
```

## Test Best Practices

1. **No `Mock<IAppDbContext>`** — use real `AppDbContext` via `TestDbContextFactory`
2. **No `Mock<DbSet<T>>`** — Moq can't mock extension methods
3. **Seed FK entities** — always add referenced entities (ApplicationUser) before the entity being tested
4. **Fresh context per test class** — `TestDbContextFactory.CreateInMemoryDbContext()` in constructor
5. **Descriptive test names** — `Handle_ShouldReturn404_WhenItemNotFound`
6. **Test both paths** — success + all failure codes (404, 403, 400)
7. **Turkish error messages** — assert exact Turkish strings from handlers
