using API.Responses;
using FluentAssertions;

namespace Tests.API_Tests.Responses;

public class StandardApiResponseTests
{
    private static readonly string[] Field1Errors = ["Error 1"];

    [Fact]
    public void SuccessResponse_WithData_ReturnsSuccessfulResponse()
    {
        // Arrange
        var data = new { Id = 1, Name = "Test" };
        var message = "Operation successful";
        var statusCode = 201;
        var metadata = new Dictionary<string, object?> { { "page", 1 } };

        // Act
        var response = StandardApiResponse<object>.SuccessResponse(data, message, statusCode, metadata);

        // Assert
        response.Success.Should().BeTrue();
        response.StatusCode.Should().Be(statusCode);
        response.Message.Should().Be(message);
        response.Data.Should().Be(data);
        response.Metadata.Should().BeEquivalentTo(metadata);
        response.Error.Should().BeNull();
        response.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void SuccessResponse_WithDefaults_ReturnsSuccessfulResponseWithDefaults()
    {
        // Arrange
        var data = "Simple Data";

        // Act
        var response = StandardApiResponse<string>.SuccessResponse(data);

        // Assert
        response.Success.Should().BeTrue();
        response.StatusCode.Should().Be(200);
        response.Message.Should().Be("Request completed successfully");
        response.Data.Should().Be(data);
        response.Metadata.Should().BeNull();
        response.Error.Should().BeNull();
    }

    [Fact]
    public void ErrorResponse_WithProblemDetails_ReturnsErrorResponse()
    {
        // Arrange
        var problemDetails = new AppProblemDetails(
            status: 404,
            title: "Not Found",
            detail: "Resource not found",
            type: "NotFound",
            instance: "/api/resource/1"
        );
        var message = "Custom Error Message";
        var traceId = "trace-123";

        // Act
        var response = StandardApiResponse<object>.ErrorResponse(problemDetails, message, traceId);

        // Assert
        response.Success.Should().BeFalse();
        response.StatusCode.Should().Be(404);
        response.Message.Should().Be(message);
        response.Data.Should().BeNull();
        response.Error.Should().Be(problemDetails);
        response.Error!.Extensions.Should().ContainKey("traceId").WhoseValue.Should().Be(traceId);
    }

    [Fact]
    public void ErrorResponse_WithProblemDetailsAndDefaults_ReturnsErrorResponseWithDefaults()
    {
        // Arrange
        var problemDetails = new AppProblemDetails
        {
            Status = 500,
            Title = "Server Error"
        };

        // Act
        var response = StandardApiResponse<object>.ErrorResponse(problemDetails);

        // Assert
        response.Success.Should().BeFalse();
        response.StatusCode.Should().Be(500);
        response.Message.Should().Be("Server Error");
        response.Error.Should().Be(problemDetails);
    }

    [Fact]
    public void ErrorResponse_WithSimpleMessage_ReturnsErrorResponse()
    {
        // Arrange
        var message = "Bad Request";
        var statusCode = 400;
        var type = "Validation";
        var detail = "Invalid input";
        var instance = "/api/submit";
        var extensions = new Dictionary<string, object?> { { "custom", "value" } };
        var traceId = "trace-456";

        // Act
        var response = StandardApiResponse<object>.ErrorResponse(
            message, statusCode, type, detail, instance, extensions, traceId);

        // Assert
        response.Success.Should().BeFalse();
        response.StatusCode.Should().Be(statusCode);
        response.Message.Should().Be(message);
        response.Data.Should().BeNull();

        response.Error.Should().NotBeNull();
        response.Error!.Status.Should().Be(statusCode);
        response.Error.Title.Should().Be(message);
        response.Error.Detail.Should().Be(detail);
        response.Error.Type.Should().Be(type);
        response.Error.Instance.Should().Be(instance);

        response.Error.Extensions.Should().ContainKey("custom").WhoseValue.Should().Be("value");
        response.Error.Extensions.Should().ContainKey("traceId").WhoseValue.Should().Be(traceId);
    }

    [Fact]
    public void ValidationErrorResponse_ReturnsValidationErrorResponse()
    {
        // Arrange
        var validationErrors = new Dictionary<string, string[]>
        {
            { "Field1", Field1Errors }
        };
        var instance = "/api/validate";
        var stackTrace = new[] { "Line 1", "Line 2" };
        var traceId = "trace-789";

        // Act
        var response = StandardApiResponse<object>.ValidationErrorResponse(
            validationErrors, instance, stackTrace, traceId);

        // Assert
        response.Success.Should().BeFalse();
        response.StatusCode.Should().Be(400);
        response.Message.Should().Be("Validation failed");

        response.Error.Should().NotBeNull();
        response.Error!.Status.Should().Be(400);
        response.Error.Title.Should().Be("Validation error");
        response.Error.Type.Should().Be("ValidationFailure");
        response.Error.Instance.Should().Be(instance);

        response.Error.Extensions.Should().ContainKey("errors");
        var errors = response.Error.Extensions["errors"] as Dictionary<string, string[]>;
        errors.Should().BeEquivalentTo(validationErrors);

        response.Error.Extensions.Should().ContainKey("stackTrace");
        var trace = response.Error.Extensions["stackTrace"] as string[];
        trace.Should().BeEquivalentTo(stackTrace);

        response.Error.Extensions.Should().ContainKey("traceId").WhoseValue.Should().Be(traceId);
    }
}
