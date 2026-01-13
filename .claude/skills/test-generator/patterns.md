# Test Patterns Reference

This file contains test templates for all common patterns in the codebase.

## Table of Contents

1. [Command Handler Tests](#command-handler-tests)
2. [Query Handler Tests](#query-handler-tests)
3. [Validator Tests](#validator-tests)
4. [Controller Tests](#controller-tests)
5. [Service Tests](#service-tests)
6. [Middleware Tests](#middleware-tests)
7. [Extension Method Tests](#extension-method-tests)

---

## Command Handler Tests

### Create Command Handler Template

**File:** `Tests/Application_Tests/Features/{Feature}/Commands/Create{Entity}/Create{Entity}CommandHandlerTests.cs`

```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Core;
using Application.Features.{Feature}.Commands.Create{Entity};
using AutoMapper;
using Domain;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Persistence;
using Tests.Helpers;

namespace Tests.Application_Tests.Features.{Feature}.Commands.Create{Entity};

public class Create{Entity}CommandHandlerTests
{
    private readonly Mock<IMapper> _mapperMock;
    private readonly IAppDbContext _context;
    private readonly Create{Entity}CommandHandler _handler;

    public Create{Entity}CommandHandlerTests()
    {
        _context = TestDbContextFactory.CreateInMemoryDbContext();
        _mapperMock = new Mock<IMapper>();
        _handler = new Create{Entity}CommandHandler(_context, _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreate{Entity}()
    {
        // Arrange
        var dto = new Create{Entity}Dto
        {
            Property1 = "Test Value",
            Property2 = 100
        };

        var command = new Create{Entity}Command { Create{Entity}Dto = dto };

        var entity = new {Entity}
        {
            Property1 = dto.Property1,
            Property2 = dto.Property2
        };

        _mapperMock.Setup(m => m.Map<{Entity}>(dto)).Returns(entity);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Be("{Entity} created successfully");
        result.Value.Should().NotBeNullOrEmpty();

        var savedEntity = await _context.{Entity}Plural.FirstOrDefaultAsync();
        savedEntity.Should().NotBeNull();
        savedEntity!.Property1.Should().Be("Test Value");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenSaveFails()
    {
        // Arrange
        var mockContext = new Mock<IAppDbContext>();
        var mockDbSet = new Mock<DbSet<{Entity}>>();

        mockContext.Setup(c => c.{Entity}Plural).Returns(mockDbSet.Object);
        mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(0); // Simulate save failure

        var handler = new Create{Entity}CommandHandler(mockContext.Object, _mapperMock.Object);

        var dto = new Create{Entity}Dto { Property1 = "Test" };
        var command = new Create{Entity}Command { Create{Entity}Dto = dto };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Code.Should().Be(400);
    }

    [Fact]
    public async Task Handle_ShouldCallMapperWithCorrectDto()
    {
        // Arrange
        var dto = new Create{Entity}Dto { Property1 = "Test" };
        var command = new Create{Entity}Command { Create{Entity}Dto = dto };

        var entity = new {Entity} { Property1 = dto.Property1 };
        _mapperMock.Setup(m => m.Map<{Entity}>(dto)).Returns(entity);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mapperMock.Verify(m => m.Map<{Entity}>(dto), Times.Once);
    }
}
```

### Edit Command Handler Template

**File:** `Tests/Application_Tests/Features/{Feature}/Commands/Edit{Entity}/Edit{Entity}CommandHandlerTests.cs`

```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Core;
using Application.Features.{Feature}.Commands.Edit{Entity};
using AutoMapper;
using Domain;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Persistence;
using Tests.Helpers;

namespace Tests.Application_Tests.Features.{Feature}.Commands.Edit{Entity};

public class Edit{Entity}CommandHandlerTests
{
    private readonly Mock<IMapper> _mapperMock;
    private readonly IAppDbContext _context;
    private readonly Edit{Entity}CommandHandler _handler;
    private readonly {Entity} _existingEntity;

    public Edit{Entity}CommandHandlerTests()
    {
        _context = TestDbContextFactory.CreateInMemoryDbContext();
        _mapperMock = new Mock<IMapper>();
        _handler = new Edit{Entity}CommandHandler(_context, _mapperMock.Object);

        _existingEntity = new {Entity}
        {
            Id = Guid.NewGuid().ToString(),
            Property1 = "Original",
            Property2 = 100
        };

        _context.{Entity}Plural.Add(_existingEntity);
        _context.SaveChanges();
    }

    [Fact]
    public async Task Handle_WhenEntityExists_ShouldUpdateAndReturnSuccess()
    {
        // Arrange
        var dto = new Edit{Entity}Dto
        {
            Property1 = "Updated",
            Property2 = 200
        };

        var command = new Edit{Entity}Command
        {
            Id = _existingEntity.Id,
            Edit{Entity}Dto = dto
        };

        _mapperMock.Setup(m => m.Map(dto, _existingEntity))
            .Callback(() =>
            {
                _existingEntity.Property1 = dto.Property1;
                _existingEntity.Property2 = dto.Property2;
            });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Be("{Entity} updated successfully");

        var updatedEntity = await _context.{Entity}Plural.FirstOrDefaultAsync(x => x.Id == _existingEntity.Id);
        updatedEntity.Should().NotBeNull();
        updatedEntity!.Property1.Should().Be("Updated");
        updatedEntity.Property2.Should().Be(200);
    }

    [Fact]
    public async Task Handle_WhenEntityNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var dto = new Edit{Entity}Dto { Property1 = "Test" };
        var nonExistentId = Guid.NewGuid().ToString();
        var command = new Edit{Entity}Command { Id = nonExistentId, Edit{Entity}Dto = dto };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Code.Should().Be(404);
        result.Message.Should().Be("{Entity} not found");
    }
}
```

### Delete Command Handler Template

**File:** `Tests/Application_Tests/Features/{Feature}/Commands/Delete{Entity}/Delete{Entity}CommandHandlerTests.cs`

```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Core;
using Application.Features.{Feature}.Commands.Delete{Entity};
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Tests.Helpers;

namespace Tests.Application_Tests.Features.{Feature}.Commands.Delete{Entity};

public class Delete{Entity}CommandHandlerTests
{
    private readonly IAppDbContext _context;
    private readonly Delete{Entity}CommandHandler _handler;
    private readonly {Entity} _existingEntity;

    public Delete{Entity}CommandHandlerTests()
    {
        _context = TestDbContextFactory.CreateInMemoryDbContext();
        _handler = new Delete{Entity}CommandHandler(_context);

        _existingEntity = new {Entity}
        {
            Id = Guid.NewGuid().ToString(),
            Property1 = "Test"
        };

        _context.{Entity}Plural.Add(_existingEntity);
        _context.SaveChanges();
    }

    [Fact]
    public async Task Handle_WhenEntityExists_ShouldDeleteAndReturnSuccess()
    {
        // Arrange
        var command = new Delete{Entity}Command { Id = _existingEntity.Id };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Be("{Entity} deleted successfully");

        var deletedEntity = await _context.{Entity}Plural.FirstOrDefaultAsync(x => x.Id == _existingEntity.Id);
        deletedEntity.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenEntityNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid().ToString();
        var command = new Delete{Entity}Command { Id = nonExistentId };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Code.Should().Be(404);
        result.Message.Should().Be("{Entity} not found");
    }
}
```

---

## Query Handler Tests

### Get List Query Handler Template

**File:** `Tests/Application_Tests/Features/{Feature}/Queries/Get{Entity}List/Get{Entity}ListQueryHandlerTests.cs`

```csharp
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Core;
using Application.Features.{Feature}.Queries.Get{Entity}List;
using AutoMapper;
using Domain;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Persistence;
using Tests.Helpers;

namespace Tests.Application_Tests.Features.{Feature}.Queries.Get{Entity}List;

public class Get{Entity}ListQueryHandlerTests
{
    private readonly IAppDbContext _context;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Get{Entity}ListQueryHandler _handler;

    public Get{Entity}ListQueryHandlerTests()
    {
        _context = TestDbContextFactory.CreateInMemoryDbContext();
        _mapperMock = new Mock<IMapper>();
        _handler = new Get{Entity}ListQueryHandler(_context, _mapperMock.Object);

        // Seed test data
        var entities = Enumerable.Range(1, 5).Select(i => new {Entity}
        {
            Id = Guid.NewGuid().ToString(),
            Property1 = $"Item {i}"
        }).ToList();

        _context.{Entity}Plural.AddRange(entities);
        _context.SaveChanges();
    }

    [Fact]
    public async Task Handle_WhenEntitiesExist_ShouldReturnListOfDtos()
    {
        // Arrange
        var query = new Get{Entity}ListQuery();

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
        _context.{Entity}Plural.RemoveRange(_context.{Entity}Plural);
        await _context.SaveChangesAsync();

        var query = new Get{Entity}ListQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }
}
```

### Get Details Query Handler Template

**File:** `Tests/Application_Tests/Features/{Feature}/Queries/Get{Entity}Details/Get{Entity}DetailsQueryHandlerTests.cs`

```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Core;
using Application.Features.{Feature}.Queries.Get{Entity}Details;
using AutoMapper;
using Domain;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Persistence;
using Tests.Helpers;

namespace Tests.Application_Tests.Features.{Feature}.Queries.Get{Entity}Details;

public class Get{Entity}DetailsQueryHandlerTests
{
    private readonly IAppDbContext _context;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Get{Entity}DetailsQueryHandler _handler;
    private readonly {Entity} _testEntity;

    public Get{Entity}DetailsQueryHandlerTests()
    {
        _context = TestDbContextFactory.CreateInMemoryDbContext();
        _mapperMock = new Mock<IMapper>();
        _handler = new Get{Entity}DetailsQueryHandler(_context, _mapperMock.Object);

        _testEntity = new {Entity}
        {
            Id = Guid.NewGuid().ToString(),
            Property1 = "Test Property"
        };

        _context.{Entity}Plural.Add(_testEntity);
        _context.SaveChanges();
    }

    [Fact]
    public async Task Handle_WhenEntityExists_ShouldReturnDto()
    {
        // Arrange
        var query = new Get{Entity}DetailsQuery { Id = _testEntity.Id };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(_testEntity.Id);
    }

    [Fact]
    public async Task Handle_WhenEntityNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid().ToString();
        var query = new Get{Entity}DetailsQuery { Id = nonExistentId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Code.Should().Be(404);
        result.Message.Should().Be("{Entity} not found");
    }
}
```

---

## Validator Tests

### Validator Template

**File:** `Tests/Application_Tests/Features/{Feature}/Commands/{Operation}/{Operation}ValidatorTests.cs`

```csharp
using Application.Features.{Feature}.Commands.{Operation};
using FluentAssertions;

namespace Tests.Application_Tests.Features.{Feature}.Commands.{Operation};

public class {Operation}ValidatorTests
{
    private readonly {Operation}Validator _validator;

    public {Operation}ValidatorTests()
    {
        _validator = new {Operation}Validator();
    }

    [Fact]
    public void Validate_WithValidCommand_ShouldPass()
    {
        // Arrange
        var command = new {Operation}
        {
            Property1 = "Valid Value",
            Property2 = 100
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WithNullDto_ShouldFail()
    {
        // Arrange
        var command = new {Operation} { Dto = null! };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public void Validate_WithEmptyProperty_ShouldFail()
    {
        // Arrange
        var command = new {Operation}
        {
            Property1 = "",
            Property2 = 100
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Property1");
    }

    [Fact]
    public void Validate_WithInvalidNumericValue_ShouldFail()
    {
        // Arrange
        var command = new {Operation}
        {
            Property1 = "Valid",
            Property2 = -1
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_WithMultipleErrors_ShouldReturnAllErrors()
    {
        // Arrange
        var command = new {Operation}
        {
            Property1 = "",
            Property2 = -1
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterOrEqualTo(2);
    }
}
```

---

## Controller Tests

### Controller Template

**File:** `Tests/API_Tests/Controllers/{ControllerName}Tests.cs`

```csharp
using API.Controllers;
using API.Responses;
using Application.Core;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Tests.API_Tests.Controllers;

public class {ControllerName}Tests : IDisposable
{
    private readonly {ControllerName} _controller;
    private readonly Mock<IMediator> _mediatorMock;

    public {ControllerName}Tests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new {ControllerName}();

        // Setup HttpContext
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = "/api/v1/test";
        httpContext.TraceIdentifier = "test-trace-id";

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(sp => sp.GetService(typeof(IMediator)))
            .Returns(_mediatorMock.Object);

        httpContext.RequestServices = serviceProviderMock.Object;
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
    }

    [Fact]
    public async Task Get_ShouldReturnOk_WhenDataExists()
    {
        // Arrange
        var result = Result<List<Dto>>.Success("Success", new List<Dto>());
        _mediatorMock.Setup(m => m.Send(It.IsAny<Query>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var actionResult = await _controller.Get();

        // Assert
        actionResult.Result.Should().BeOfType<OkObjectResult>();
        var okResult = actionResult.Result as OkObjectResult;
        var response = okResult!.Value as StandardApiResponse<List<Dto>>;
        response!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task Get_ShouldReturnNotFound_WhenNotFound()
    {
        // Arrange
        var result = Result<Dto>.Failure("Not found", 404);
        _mediatorMock.Setup(m => m.Send(It.IsAny<Query>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var actionResult = await _controller.GetById("id");

        // Assert
        actionResult.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Post_ShouldReturnOk_WhenCreationSucceeds()
    {
        // Arrange
        var result = Result<string>.Success("Created", "id");
        _mediatorMock.Setup(m => m.Send(It.IsAny<Command>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var actionResult = await _controller.Create(new Dto());

        // Assert
        actionResult.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Put_ShouldReturnNotFound_WhenEntityNotFound()
    {
        // Arrange
        var result = Result<Unit>.Failure("Not found", 404);
        _mediatorMock.Setup(m => m.Send(It.IsAny<Command>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var actionResult = await _controller.Update("id", new Dto());

        // Assert
        actionResult.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Delete_ShouldReturnOk_WhenDeletionSucceeds()
    {
        // Arrange
        var result = Result<Unit>.Success("Deleted", Unit.Value);
        _mediatorMock.Setup(m => m.Send(It.IsAny<Command>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var actionResult = await _controller.Delete("id");

        // Assert
        actionResult.Result.Should().BeOfType<OkObjectResult>();
    }

    public void Dispose()
    {
        _controller?.Dispose();
    }
}
```

---

## Service Tests

### Service Template

**File:** `Tests/Application_Tests/Services/{ServiceName}Tests.cs` or `Tests/Infrastructure_Tests/{Path}/{ServiceName}Tests.cs`

```csharp
using FluentAssertions;
using Moq;
using Xunit;

namespace Tests.{TestFolder}.Services;

public class {ServiceName}Tests
{
    private readonly Mock<IDependency> _dependencyMock;
    private readonly {ServiceName} _service;

    public {ServiceName}Tests()
    {
        _dependencyMock = new Mock<IDependency>();
        _service = new {ServiceName}(_dependencyMock.Object);
    }

    [Fact]
    public void MethodName_WithValidInput_ShouldReturnExpected()
    {
        // Arrange
        var input = "test";
        var expected = "result";
        _dependencyMock.Setup(d => d.SomeMethod(input)).Returns(expected);

        // Act
        var result = _service.MethodName(input);

        // Assert
        result.Should().Be(expected);
        _dependencyMock.Verify(d => d.SomeMethod(input), Times.Once);
    }

    [Fact]
    public void MethodName_WithInvalidInput_ShouldThrowException()
    {
        // Arrange
        var input = "";

        // Act
        var action = () => _service.MethodName(input);

        // Assert
        action.Should().Throw<ArgumentException>()
            .WithMessage("*cannot be empty*");
    }

    [Fact]
    public async Task AsyncMethod_WhenCalled_ShouldReturnSuccess()
    {
        // Arrange
        _dependencyMock.Setup(d => d.GetAsync()).ReturnsAsync("value");

        // Act
        var result = await _service.AsyncMethod();

        // Assert
        result.Should().Be("value");
    }
}
```

---

## Middleware Tests

### Middleware Template

**File:** `Tests/API_Tests/Middleware/{MiddlewareName}Tests.cs`

```csharp
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Tests.API_Tests.Middleware;

public class {MiddlewareName}Tests
{
    private readonly Mock<ILogger<{MiddlewareName}>> _loggerMock;
    private readonly {MiddlewareName} _middleware;
    private readonly DefaultHttpContext _context;

    public {MiddlewareName}Tests()
    {
        _loggerMock = new Mock<ILogger<{MiddlewareName}>>();
        _middleware = new {MiddlewareName}(next: (innerContext) => Task.CompletedTask, _loggerMock.Object);
        _context = new DefaultHttpContext();
    }

    [Fact]
    public async Task Invoke_WhenCalled_ShouldCallNext()
    {
        // Arrange
        var nextCalled = false;
        var middleware = new {MiddlewareName>(
            (innerContext) =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            },
            _loggerMock.Object
        );

        // Act
        await middleware.Invoke(_context);

        // Assert
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task Invoke_WhenExceptionThrown_ShouldHandleGracefully()
    {
        // Arrange
        var middleware = new {MiddlewareName>(
            (innerContext) => throw new Exception("Test exception"),
            _loggerMock.Object
        );

        // Act
        var act = async () => await middleware.Invoke(_context);

        // Assert
        await act.Should().NotThrowAsync<Exception>();
        _context.Response.StatusCode.Should().Be(500);
    }
}
```

---

## Extension Method Tests

### Extension Method Template

**File:** `Tests/API_Tests/Extensions/{ExtensionName}Tests.cs` or `Tests/Application_Tests/Extensions/{ExtensionName}Tests.cs`

```csharp
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Tests.{TestFolder}.Extensions;

public class {ExtensionName}Tests
{
    [Fact]
    public void ExtensionMethod_WithNullBuilder_ShouldThrowArgumentNullException()
    {
        // Arrange
        IApplicationBuilder builder = null!;

        // Act
        var action = () => builder.ExtensionMethod();

        // Assert
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ExtensionMethod_WithValidBuilder_ShouldReturnBuilder()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new ApplicationBuilder(services.BuildServiceProvider());

        // Act
        var result = builder.ExtensionMethod();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeSameAs(builder);
    }

    [Fact]
    public void ExtensionMethod_WhenCalled_ShouldRegisterServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new ApplicationBuilder(services.BuildServiceProvider());

        // Act
        builder.ExtensionMethod();

        // Assert
        services.Should().ContainSingle(d => d.ServiceType == typeof(IService));
    }
}
```

---

## Test Best Practices

1. **AAA Pattern**: Always use Arrange-Act-Assert structure
2. **Descriptive Names**: Test names should follow `MethodName_Senario_ExpectedOutcome`
3. **Mock External Dependencies**: Use Moq for DbContext, AutoMapper, MediatR, etc.
4. **Unique Database Names**: Each test class should use unique in-memory database
5. **Dispose Resources**: Implement IDisposable for tests with database contexts
6. **Test Both Paths**: Always test both success and failure scenarios
7. **Verify Mock Calls**: Use Verify to ensure methods are called correctly
8. **Use FluentAssertions**: Provides readable assertion syntax
9. **Avoid Test Interdependence**: Each test should be independent
10. **One Assert Per Test**: Prefer focused tests with single assertions
