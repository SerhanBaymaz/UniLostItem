# Test Patterns — UniLostItem

Test templates matching the actual patterns from `LostItems` and `ItemClaims` tests.

## Table of Contents

1. [Create Handler Tests](#create-handler-tests)
2. [Update/Delete Handler Tests (with ownership)](#updatedelete-handler-tests)
3. [Query Handler Tests](#query-handler-tests)
4. [Validator Tests](#validator-tests)
5. [Workflow Handler Tests (status transitions)](#workflow-handler-tests)

---

## Create Handler Tests

**Pattern:** SeedUser helper, Mock IMapper + ICurrentUserService, real AppDbContext

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
        _context.Users.Add(new ApplicationUser
        {
            Id = "user-1",
            UserName = "testuser",
            Email = "test@test.com",
            EmailConfirmed = true,
            FirstName = "Test",
            LastName = "User"
        });
        await _context.SaveChangesAsync();
    }

    [Fact]
    public async Task Handle_ShouldCreate_WhenValid()
    {
        await SeedUser();
        var dto = new Create{Entity}Dto { /* valid properties */ };
        var command = new Create{Entity}Command { Create{Entity}Dto = dto };
        _mapperMock.Setup(m => m.Map<{Entity}>(dto)).Returns(new {Entity} { /* ... */ });

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Handle_ShouldSetUserId_FromService()
    {
        await SeedUser();
        // ... create and handle ...

        var saved = await _context.{Entity}Plural.FirstOrDefaultAsync();
        saved!.UserId.Should().Be("user-1");
    }

    [Fact]
    public async Task Handle_ShouldCallMapper()
    {
        await SeedUser();
        var dto = new Create{Entity}Dto { /* ... */ };
        var command = new Create{Entity}Command { Create{Entity}Dto = dto };
        _mapperMock.Setup(m => m.Map<{Entity}>(dto)).Returns(new {Entity} { /* ... */ });

        await _handler.Handle(command, CancellationToken.None);

        _mapperMock.Verify(m => m.Map<{Entity}>(dto), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenSaveFails()
    {
        await SeedUser();
        var dto = new Create{Entity}Dto { /* ... */ };
        var command = new Create{Entity}Command { Create{Entity}Dto = dto };
        _mapperMock.Setup(m => m.Map<{Entity}>(dto)).Returns(new {Entity} { /* ... */ });

        await _context.DisposeAsync(); // break the context

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Code.Should().Be(400);
    }
}
```

---

## Update/Delete Handler Tests

**Pattern:** `CreateContextWithItem()` helper that seeds both user and entity, tests 404/403/success

```csharp
using Application.Core;
using Application.Features.{Feature}.Commands.Update{Entity};
using Application.Interfaces;
using Domain;
using FluentAssertions;
using Moq;
using Persistence;
using Tests.Helpers;

namespace Tests.Application_Tests.Features.{Feature}.Commands.Update{Entity};

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
    public async Task Handle_ShouldUpdate_WhenUserIsOwner()
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

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenNoChanges()
    {
        var context = await CreateContextWithItem(OwnerUserId);
        var item = await context.{Entity}Plural.FindAsync("item-1");
        var currentUserMock = new Mock<ICurrentUserService>();
        currentUserMock.Setup(x => x.UserId).Returns(OwnerUserId);
        var handler = new Update{Entity}CommandHandler(context, currentUserMock.Object);

        var command = new Update{Entity}Command
        {
            Id = "item-1",
            Update{Entity}Dto = new Update{Entity}Dto { Property1 = item!.Property1 }
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }
}
```

**Delete handler follows same pattern but tests soft delete (`IsDeleted = true`) instead of property changes.**

---

## Query Handler Tests

**Pattern:** Seed data helper, real AppDbContext (no mocks), test filters/sort/pagination/soft-delete

```csharp
using Application.Core;
using Application.Features.{Feature}.Queries.Get{Entity}List;
using Domain;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Tests.Helpers;

namespace Tests.Application_Tests.Features.{Feature}.Queries.Get{Entity}List;

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
            new() { Title = "Alpha Item", UserId = "user-1", IsActive = true },
            new() { Title = "Beta Item", UserId = "user-1", IsActive = true }
        };
        _context.{Entity}Plural.AddRange(items);
        await _context.SaveChangesAsync();
    }

    [Fact]
    public async Task Handle_ShouldReturnAllActiveItems()
    {
        await SeedData();
        var handler = new Get{Entity}ListQueryHandler(_context);
        var result = await handler.Handle(new Get{Entity}ListQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_ShouldExcludeSoftDeleted()
    {
        await SeedData();
        var item = await _context.{Entity}Plural.FirstAsync();
        item.IsDeleted = true;
        await _context.SaveChangesAsync();

        var handler = new Get{Entity}ListQueryHandler(_context);
        var result = await handler.Handle(new Get{Entity}ListQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_ShouldReturnPaginationMetadata()
    {
        await SeedData();
        var handler = new Get{Entity}ListQueryHandler(_context);
        var query = new Get{Entity}ListQuery { PageNumber = 1, PageSize = 1 };
        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.TotalCount.Should().Be(2);
        result.Value.TotalPages.Should().Be(2);
        result.Value.HasNext.Should().BeTrue();
        result.Value.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_ShouldSortByTitleAscending()
    {
        await SeedData();
        var handler = new Get{Entity}ListQueryHandler(_context);
        var query = new Get{Entity}ListQuery { SortBy = {Entity}SortField.Title, SortDescending = false };
        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Items[0].Title.Should().Be("Alpha Item");
        result.Value!.Items[1].Title.Should().Be("Beta Item");
    }
}
```

---

## Validator Tests

**Pattern:** Use `TestValidate` from FluentValidation, no mocking needed

```csharp
using Application.Features.{Feature}.Commands.Create{Entity};
using FluentAssertions;

namespace Tests.Application_Tests.Features.{Feature}.Commands.Create{Entity};

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
    public void Should_HaveError_WhenRequiredFieldIsEmpty()
    {
        var command = new Create{Entity}Command
        {
            Create{Entity}Dto = new Create{Entity}Dto { Title = "" }
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Create{Entity}Dto!.Title);
    }

    [Fact]
    public void Should_HaveError_WhenFieldExceedsMaxLength()
    {
        var command = new Create{Entity}Command
        {
            Create{Entity}Dto = new Create{Entity}Dto { Title = new string('x', 201) }
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Create{Entity}Dto!.Title);
    }

    [Fact]
    public void Should_NotHaveError_WhenAllFieldsValid()
    {
        var command = new Create{Entity}Command
        {
            Create{Entity}Dto = new Create{Entity}Dto { Title = "Valid", Description = "Valid" }
        };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_HaveMultipleErrors_WhenMultipleFieldsInvalid()
    {
        var command = new Create{Entity}Command
        {
            Create{Entity}Dto = new Create{Entity}Dto { Title = "", Description = "" }
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Create{Entity}Dto!.Title);
        result.ShouldHaveValidationErrorFor(x => x.Create{Entity}Dto!.Description);
    }
}
```

---

## Workflow Handler Tests

For status-transition handlers like `CancelItemClaim`, `RespondToClaim`, `ExtendClaimDeadline`, `AdminReviewClaim:

**Pattern:** Seed item + claim, test each status guard and transition

```csharp
public class CancelItemClaimCommandHandlerTests
{
    private async Task<AppDbContext> CreateContextWithClaim(
        string claimantId = "claimant-id",
        string ownerId = "owner-id")
    {
        var context = TestDbContextFactory.CreateInMemoryDbContext();
        context.Users.Add(new ApplicationUser { Id = ownerId, /* ... */ });
        context.Users.Add(new ApplicationUser { Id = claimantId, /* ... */ });
        context.LostItems.Add(new LostItem { Id = "item-1", UserId = ownerId, IsActive = true });
        context.ItemClaims.Add(new ItemClaim
        {
            Id = "claim-1",
            LostItemId = "item-1",
            ClaimantId = claimantId,
            Status = ClaimStatus.Pending,
            ExpiresAt = DateTime.UtcNow.AddDays(2),
            IsActive = true
        });
        await context.SaveChangesAsync();
        return context;
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenClaimDoesNotExist() { /* 404 */ }
    [Fact]
    public async Task Handle_ShouldReturnForbidden_WhenNotClaimant() { /* 403 */ }
    [Fact]
    public async Task Handle_ShouldReturnBadRequest_WhenNotPending() { /* 400 */ }
    [Fact]
    public async Task Handle_ShouldCancel_WhenPending() { /* Status = Cancelled */ }
}
```
