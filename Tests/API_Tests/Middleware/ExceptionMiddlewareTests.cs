using System.Text.Json;
using API.Middleware;
using API.Responses;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Tests.API_Tests.Middleware;

public class ExceptionMiddlewareTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly Mock<ILogger<ExceptionMiddleware>> _loggerMock;
    private readonly Mock<IHostEnvironment> _envMock;
    private readonly ExceptionMiddleware _middleware;

    public ExceptionMiddlewareTests()
    {
        _loggerMock = new Mock<ILogger<ExceptionMiddleware>>();
        _envMock = new Mock<IHostEnvironment>();
        _middleware = new ExceptionMiddleware(_loggerMock.Object, _envMock.Object);
    }

    [Fact]
    public async Task InvokeAsync_WhenRequestIsSuccessful_ShouldNotModifyResponse()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.Response.StatusCode = 200;

        RequestDelegate next = (ctx) =>
        {
            return Task.CompletedTask;
        };

        // Act
        await _middleware.InvokeAsync(context, next);

        // Assert
        context.Response.StatusCode.Should().Be(200);
        context.Response.Body.Length.Should().Be(0);
    }

    [Fact]
    public async Task InvokeAsync_WhenValidationExceptionThrown_ShouldReturnBadRequestAndValidationErrors()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.Request.Path = "/api/test";

        var failures = new List<ValidationFailure>
        {
            new ValidationFailure("Field1", "Error 1"),
            new ValidationFailure("Field2", "Error 2")
        };
        var validationException = new ValidationException(failures);

        RequestDelegate next = (ctx) => throw validationException;

        // Act
        await _middleware.InvokeAsync(context, next);

        // Assert
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        var response = JsonSerializer.Deserialize<StandardApiResponse<object>>(body, JsonOptions);

        context.Response.StatusCode.Should().Be(400);
        response.Should().NotBeNull();
        response!.Success.Should().BeFalse();
        response.StatusCode.Should().Be(400);
        response.Message.Should().Be("Validation failed");

        response.Error.Should().NotBeNull();
        response.Error!.Extensions.Should().ContainKey("errors");

        var errorsElement = (JsonElement)response.Error.Extensions["errors"]!;
        var errors = JsonSerializer.Deserialize<Dictionary<string, string[]>>(errorsElement.GetRawText(), JsonOptions);

        errors.Should().ContainKey("Field1").WhoseValue.Should().Contain("Error 1");
        errors.Should().ContainKey("Field2").WhoseValue.Should().Contain("Error 2");
    }

    [Fact]
    public async Task InvokeAsync_WhenGenericExceptionThrown_ShouldReturnInternalServerError()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.Request.Path = "/api/error";
        var exceptionMessage = "Something went wrong";
        var exception = new Exception(exceptionMessage);

        RequestDelegate next = (ctx) => throw exception;

        // Act
        await _middleware.InvokeAsync(context, next);

        // Assert
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        var response = JsonSerializer.Deserialize<StandardApiResponse<object>>(body, JsonOptions);

        context.Response.StatusCode.Should().Be(500);
        response.Should().NotBeNull();
        response!.Success.Should().BeFalse();
        response.StatusCode.Should().Be(500);
        response.Message.Should().Be("Internal Server Error");

        response.Error.Should().NotBeNull();
        response.Error!.Detail.Should().Be(exceptionMessage);
        response.Error.Type.Should().Be("InternalServerError");
    }

    [Fact]
    public async Task InvokeAsync_WhenGenericExceptionThrownInDevelopment_ShouldIncludeStackTrace()
    {
        // Arrange
        _envMock.Setup(e => e.EnvironmentName).Returns("Development");

        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var exception = new Exception("Dev Error");

        RequestDelegate next = (ctx) => throw exception;

        // Act
        await _middleware.InvokeAsync(context, next);

        // Assert
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        var response = JsonSerializer.Deserialize<StandardApiResponse<object>>(body, JsonOptions);

        response.Should().NotBeNull();
        response!.Error.Should().NotBeNull();
        response.Error!.Extensions.Should().ContainKey("stackTrace");
    }

    [Fact]
    public async Task InvokeAsync_WhenStatusCodeIs415_ShouldWriteStandardApiResponse()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        RequestDelegate next = (ctx) =>
        {
            ctx.Response.StatusCode = 415;
            return Task.CompletedTask;
        };

        // Act
        await _middleware.InvokeAsync(context, next);

        // Assert
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();

        var response = JsonSerializer.Deserialize<StandardApiResponse<object>>(body, JsonOptions);

        response.Should().NotBeNull();
        response!.Success.Should().BeFalse();
        response.StatusCode.Should().Be(415);
        response.Message.Should().Be("Unsupported Media Type");
    }

    [Fact]
    public async Task InvokeAsync_WhenStatusCodeIs404_ShouldWriteStandardApiResponse()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        RequestDelegate next = (ctx) =>
        {
            ctx.Response.StatusCode = 404;
            return Task.CompletedTask;
        };

        // Act
        await _middleware.InvokeAsync(context, next);

        // Assert
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();

        var response = JsonSerializer.Deserialize<StandardApiResponse<object>>(body, JsonOptions);

        response.Should().NotBeNull();
        response!.Success.Should().BeFalse();
        response.StatusCode.Should().Be(404);
        response.Message.Should().Be("Resource Not Found");
    }

    [Fact]
    public async Task InvokeAsync_WhenValidationExceptionHasMultipleErrorsForSameField_ShouldAggregateErrors()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.Request.Path = "/api/test";

        var failures = new List<ValidationFailure>
        {
            new ValidationFailure("Field1", "Error 1"),
            new ValidationFailure("Field1", "Error 2")
        };
        var validationException = new ValidationException(failures);

        RequestDelegate next = (ctx) => throw validationException;

        // Act
        await _middleware.InvokeAsync(context, next);

        // Assert
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        var response = JsonSerializer.Deserialize<StandardApiResponse<object>>(body, JsonOptions);

        response.Should().NotBeNull();
        response!.Error.Should().NotBeNull();
        response.Error!.Extensions.Should().ContainKey("errors");

        var errorsElement = (JsonElement)response.Error.Extensions["errors"]!;
        var errors = JsonSerializer.Deserialize<Dictionary<string, string[]>>(errorsElement.GetRawText(), JsonOptions);

        errors.Should().ContainKey("Field1");
        errors!["Field1"].Should().HaveCount(2);
        errors["Field1"].Should().Contain("Error 1");
        errors["Field1"].Should().Contain("Error 2");
    }
}
