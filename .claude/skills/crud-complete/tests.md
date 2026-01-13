# Test Templates

This file contains unit test templates for all CRUD operations using xUnit, Moq, and FluentAssertions.

## Table of Contents

1. [Test Setup](#test-setup)
2. [Create Command Tests](#create-command-tests)
3. [Edit Command Tests](#edit-command-tests)
4. [Delete Command Tests](#delete-command-tests)
5. [Get List Query Tests](#get-list-query-tests)
6. [Get Details Query Tests](#get-details-query-tests)
7. [Validator Tests](#validator-tests)

---

## Test Setup

### Common Test Base

All tests use:
- **xUnit** as test framework
- **Moq** for mocking
- **FluentAssertions** for assertions
- **Microsoft.EntityFrameworkCore.InMemory** for database

### Usings

```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Features.{EntityName}Plural.Commands.Create{EntityName};
using Application.Core;
using Domain;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Persistence;
using Xunit;
```

---

## Create Command Tests

### Handler Tests

**File:** `Tests/Application_Tests/Features/{EntityName}Plural/Commands/Create{EntityName}/Create{EntityName}CommandHandlerTests.cs`

```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Features.{EntityName}Plural.Commands.Create{EntityName};
using Application.Core;
using AutoMapper;
using Domain;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Persistence;
using Xunit;

namespace Tests.Application_Tests.Features.{EntityName}Plural.Commands.Create{EntityName};

public class Create{EntityName}CommandHandlerTests : IDisposable
{
    private readonly IAppDbContext _context;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Create{EntityName}CommandHandler _handler;
    private readonly {EntityName} _testEntity;

    public Create{EntityName}CommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _mapperMock = new Mock<IMapper>();
        _handler = new Create{EntityName}CommandHandler(_context, _mapperMock.Object);

        _testEntity = new {EntityName}
        {
            Id = Guid.NewGuid().ToString(),
            Property1 = "Test Property1",
            Property2 = "Test Property2",
            Property3 = 100
        };
    }

    [Fact]
    public async Task Handle_WhenValidCommand_ShouldReturnSuccessWithId()
    {
        // Arrange
        var dto = new Create{EntityName}Dto
        {
            Property1 = "Test Property1",
            Property2 = "Test Property2",
            Property3 = 100
        };

        var command = new Create{EntityName}Command { Create{EntityName}Dto = dto };

        _mapperMock.Setup(m => m.Map<{EntityName}>(dto)).Returns(_testEntity);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(_testEntity.Id);
        result.Message.Should().Be("{EntityName} created successfully");

        _context.{EntityName}Plural.Should().ContainSingle();
        var savedEntity = await _context.{EntityName}Plural.FirstOrDefaultAsync();
        savedEntity.Should().NotBeNull();
        savedEntity!.Id.Should().Be(_testEntity.Id);
    }

    [Fact]
    public async Task Handle_WhenDbContextSaveChangesFails_ShouldReturnFailure()
    {
        // Arrange
        var dto = new Create{EntityName}Dto
        {
            Property1 = "Test Property1",
            Property2 = "Test Property2",
            Property3 = 100
        };

        var command = new Create{EntityName}Command { Create{EntityName}Dto = dto };

        _mapperMock.Setup(m => m.Map<{EntityName}>(dto)).Returns(_testEntity);

        // Simulate save failure by disposing context
        await _context.DisposeAsync();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Code.Should().Be(400);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
```

---

## Edit Command Tests

### Handler Tests

**File:** `Tests/Application_Tests/Features/{EntityName}Plural/Commands/Edit{EntityName}/Edit{EntityName}CommandHandlerTests.cs`

```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Features.{EntityName}Plural.Commands.Edit{EntityName};
using Application.Core;
using AutoMapper;
using Domain;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Persistence;
using Xunit;

namespace Tests.Application_Tests.Features.{EntityName}Plural.Commands.Edit{EntityName};

public class Edit{EntityName}CommandHandlerTests : IDisposable
{
    private readonly IAppDbContext _context;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Edit{EntityName}CommandHandler _handler;
    private readonly {EntityName} _existingEntity;

    public Edit{EntityName}CommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _mapperMock = new Mock<IMapper>();
        _handler = new Edit{EntityName}CommandHandler(_context, _mapperMock.Object);

        _existingEntity = new {EntityName}
        {
            Id = Guid.NewGuid().ToString(),
            Property1 = "Original Property1",
            Property2 = "Original Property2",
            Property3 = 50
        };

        _context.{EntityName}Plural.Add(_existingEntity);
        _context.SaveChanges();
    }

    [Fact]
    public async Task Handle_WhenEntityExists_ShouldUpdateAndReturnSuccess()
    {
        // Arrange
        var dto = new Edit{EntityName}Dto
        {
            Property1 = "Updated Property1",
            Property2 = "Updated Property2",
            Property3 = 200
        };

        var command = new Edit{EntityName}Command { Id = _existingEntity.Id, Edit{EntityName}Dto = dto };

        _mapperMock.Setup(m => m.Map(dto, _existingEntity))
            .Callback(() =>
            {
                _existingEntity.Property1 = dto.Property1;
                _existingEntity.Property2 = dto.Property2;
                _existingEntity.Property3 = dto.Property3;
            });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Be("{EntityName} updated successfully");

        var updatedEntity = await _context.{EntityName}Plural.FirstOrDefaultAsync(x => x.Id == _existingEntity.Id);
        updatedEntity.Should().NotBeNull();
        updatedEntity!.Property1.Should().Be(dto.Property1);
        updatedEntity.Property2.Should().Be(dto.Property2);
        updatedEntity.Property3.Should().Be(dto.Property3);
    }

    [Fact]
    public async Task Handle_WhenEntityNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var dto = new Edit{EntityName}Dto
        {
            Property1 = "Updated Property1",
            Property2 = "Updated Property2",
            Property3 = 200
        };

        var nonExistentId = Guid.NewGuid().ToString();
        var command = new Edit{EntityName}Command { Id = nonExistentId, Edit{EntityName}Dto = dto };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Code.Should().Be(404);
        result.Message.Should().Be("{EntityName} not found");
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
```

---

## Delete Command Tests

### Handler Tests

**File:** `Tests/Application_Tests/Features/{EntityName}Plural/Commands/Delete{EntityName}/Delete{EntityName}CommandHandlerTests.cs`

```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Features.{EntityName}Plural.Commands.Delete{EntityName};
using Application.Core;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Xunit;

namespace Tests.Application_Tests.Features.{EntityName}Plural.Commands.Delete{EntityName};

public class Delete{EntityName}CommandHandlerTests : IDisposable
{
    private readonly IAppDbContext _context;
    private readonly Delete{EntityName}CommandHandler _handler;
    private readonly {EntityName} _existingEntity;

    public Delete{EntityName}CommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _handler = new Delete{EntityName}CommandHandler(_context);

        _existingEntity = new {EntityName}
        {
            Id = Guid.NewGuid().ToString(),
            Property1 = "Test Property1",
            Property2 = "Test Property2",
            Property3 = 100
        };

        _context.{EntityName}Plural.Add(_existingEntity);
        _context.SaveChanges();
    }

    [Fact]
    public async Task Handle_WhenEntityExists_ShouldDeleteAndReturnSuccess()
    {
        // Arrange
        var command = new Delete{EntityName}Command { Id = _existingEntity.Id };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Be("{EntityName} deleted successfully");

        var deletedEntity = await _context.{EntityName}Plural.FirstOrDefaultAsync(x => x.Id == _existingEntity.Id);
        deletedEntity.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenEntityNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid().ToString();
        var command = new Delete{EntityName}Command { Id = nonExistentId };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Code.Should().Be(404);
        result.Message.Should().Be("{EntityName} not found");
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
```

---

## Get List Query Tests

### Handler Tests

**File:** `Tests/Application_Tests/Features/{EntityName}Plural/Queries/Get{EntityName}List/Get{EntityName}ListQueryHandlerTests.cs`

```csharp
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Features.{EntityName}Plural.Queries.Get{EntityName}List;
using Application.Core;
using AutoMapper;
using Domain;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Persistence;
using Xunit;

namespace Tests.Application_Tests.Features.{EntityName}Plural.Queries.Get{EntityName}List;

public class Get{EntityName}ListQueryHandlerTests : IDisposable
{
    private readonly IAppDbContext _context;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Get{EntityName}ListQueryHandler _handler;

    public Get{EntityName}ListQueryHandlerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _mapperMock = new Mock<IMapper>();
        _handler = new Get{EntityName}ListQueryHandler(_context, _mapperMock.Object);

        // Seed test data
        var entities = Enumerable.Range(1, 5).Select(i => new {EntityName}
        {
            Id = Guid.NewGuid().ToString(),
            Property1 = $"Property1_{i}",
            Property2 = $"Property2_{i}",
            Property3 = i * 10
        }).ToList();

        _context.{EntityName}Plural.AddRange(entities);
        _context.SaveChanges();
    }

    [Fact]
    public async Task Handle_WhenEntitiesExist_ShouldReturnListOfDtos()
    {
        // Arrange
        var query = new Get{EntityName}ListQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        result.Value.Should().HaveCount(5);
    }

    [Fact]
    public async Task Handle_WhenNoEntitiesExist_ShouldReturnEmptyList()
    {
        // Arrange
        _context.{EntityName}Plural.RemoveRange(_context.{EntityName}Plural);
        await _context.SaveChangesAsync();

        var query = new Get{EntityName}ListQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
```

---

## Get Details Query Tests

### Handler Tests

**File:** `Tests/Application_Tests/Features/{EntityName}Plural/Queries/Get{EntityName}Details/Get{EntityName}DetailsQueryHandlerTests.cs`

```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Features.{EntityName}Plural.Queries.Get{EntityName}Details;
using Application.Features.{EntityName}Plural.Queries.Common.DTOs;
using Application.Core;
using AutoMapper;
using Domain;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Persistence;
using Xunit;

namespace Tests.Application_Tests.Features.{EntityName}Plural.Queries.Get{EntityName}Details;

public class Get{EntityName}DetailsQueryHandlerTests : IDisposable
{
    private readonly IAppDbContext _context;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Get{EntityName}DetailsQueryHandler _handler;
    private readonly {EntityName} _testEntity;
    private readonly Get{EntityName}Dto _testDto;

    public Get{EntityName}DetailsQueryHandlerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _mapperMock = new Mock<IMapper>();
        _handler = new Get{EntityName}DetailsQueryHandler(_context, _mapperMock.Object);

        _testEntity = new {EntityName}
        {
            Id = Guid.NewGuid().ToString(),
            Property1 = "Test Property1",
            Property2 = "Test Property2",
            Property3 = 100
        };

        _testDto = new Get{EntityName}Dto
        {
            Id = _testEntity.Id,
            Property1 = _testEntity.Property1,
            Property2 = _testEntity.Property2,
            Property3 = _testEntity.Property3
        };

        _context.{EntityName}Plural.Add(_testEntity);
        _context.SaveChanges();

        _mapperMock.Setup(m => m.Map<Get{EntityName}Dto>(_testEntity)).Returns(_testDto);
    }

    [Fact]
    public async Task Handle_WhenEntityExists_ShouldReturnDto()
    {
        // Arrange
        var query = new Get{EntityName}DetailsQuery { Id = _testEntity.Id };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(_testEntity.Id);
        result.Value.Property1.Should().Be(_testEntity.Property1);
    }

    [Fact]
    public async Task Handle_WhenEntityNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid().ToString();
        var query = new Get{EntityName}DetailsQuery { Id = nonExistentId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Code.Should().Be(404);
        result.Message.Should().Be("{EntityName} not found");
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
```

---

## Validator Tests

### Create Command Validator Tests

**File:** `Tests/Application_Tests/Features/{EntityName}Plural/Commands/Create{EntityName}/Create{EntityName}CommandValidatorTests.cs`

```csharp
using Application.Features.{EntityName}Plural.Commands.Create{EntityName};
using FluentValidation.TestHelper;
using Xunit;

namespace Tests.Application_Tests.Features.{EntityName}Plural.Commands.Create{EntityName};

public class Create{EntityName}CommandValidatorTests
{
    private readonly Create{EntityName}CommandValidator _validator;

    public Create{EntityName}CommandValidatorTests()
    {
        _validator = new Create{EntityName}CommandValidator();
    }

    [Fact]
    public void Should_HaveError_WhenDtoIsNull()
    {
        // Arrange
        var command = new Create{EntityName}Command { Create{EntityName}Dto = null! };

        // Act & Assert
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Create{EntityName}Dto);
    }

    [Fact]
    public void Should_HaveError_WhenProperty1IsEmpty()
    {
        // Arrange
        var command = new Create{EntityName}Command
        {
            Create{EntityName}Dto = new Create{EntityName}Dto
            {
                Property1 = "",
                Property2 = "Test",
                Property3 = 100
            }
        };

        // Act & Assert
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Create{EntityName}Dto.Property1);
    }

    [Fact]
    public void Should_NotHaveError_WhenAllPropertiesAreValid()
    {
        // Arrange
        var command = new Create{EntityName}Command
        {
            Create{EntityName}Dto = new Create{EntityName}Dto
            {
                Property1 = "Valid Property1",
                Property2 = "Valid Property2",
                Property3 = 100
            }
        };

        // Act & Assert
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
```

### Edit Command Validator Tests

**File:** `Tests/Application_Tests/Features/{EntityName}Plural/Commands/Edit{EntityName}/Edit{EntityName}CommandValidatorTests.cs`

```csharp
using Application.Features.{EntityName}Plural.Commands.Edit{EntityName};
using FluentValidation.TestHelper;
using Xunit;

namespace Tests.Application_Tests.Features.{EntityName}Plural.Commands.Edit{EntityName};

public class Edit{EntityName}CommandValidatorTests
{
    private readonly Edit{EntityName}CommandValidator _validator;

    public Edit{EntityName}CommandValidatorTests()
    {
        _validator = new Edit{EntityName}CommandValidator();
    }

    [Fact]
    public void Should_HaveError_WhenIdIsEmpty()
    {
        // Arrange
        var command = new Edit{EntityName}Command
        {
            Id = "",
            Edit{EntityName}Dto = new Edit{EntityName}Dto()
        };

        // Act & Assert
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public void Should_HaveError_WhenDtoIsNull()
    {
        // Arrange
        var command = new Edit{EntityName}Command
        {
            Id = "valid-id",
            Edit{EntityName}Dto = null!
        };

        // Act & Assert
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Edit{EntityName}Dto);
    }

    [Fact]
    public void Should_NotHaveError_WhenAllPropertiesAreValid()
    {
        // Arrange
        var command = new Edit{EntityName}Command
        {
            Id = "valid-id",
            Edit{EntityName}Dto = new Edit{EntityName}Dto
            {
                Property1 = "Valid Property1",
                Property2 = "Valid Property2",
                Property3 = 100
            }
        };

        // Act & Assert
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
```

---

# Test Best Practices

1. **Dispose context properly**: Always implement `IDisposable` and dispose the in-memory context
2. **Use unique database names**: Each test should use a unique in-memory database
3. **Arrange-Act-Assert**: Follow AAA pattern for clear test structure
4. **Use descriptive test names**: Test names should describe what is being tested
5. **Test both success and failure paths**: Cover both happy path and error scenarios
6. **Mock external dependencies**: Use Moq for services like AutoMapper
7. **Use FluentAssertions**: Provides readable assertion syntax
8. **Run tests in isolation**: Each test should be independent of others

---

# Running Tests

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test
dotnet test --filter "FullyQualifiedName~Create{EntityName}CommandHandlerTests"

# Verbose output
dotnet test --verbosity detailed
```
