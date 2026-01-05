using API.Helpers;
using API.Responses;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;

namespace Tests.API_Tests.Helpers;

public class ModelStateResponseFactoryTests
{
    private static readonly string[] ExpectedField2Errors = ["Error 2", "Error 3"];

    [Fact]
    public void Create_WithModelStateErrors_ReturnsBadRequestWithStandardErrorResponse()
    {
        // Arrange
        var modelState = new ModelStateDictionary();
        modelState.AddModelError("Field1", "Error 1");
        modelState.AddModelError("Field2", "Error 2");
        modelState.AddModelError("Field2", "Error 3");

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = "/api/test";
        httpContext.TraceIdentifier = "trace-id";

        var actionContext = new ActionContext(
            httpContext,
            new RouteData(),
            new ActionDescriptor(),
            modelState
        );

        // Act
        var result = ModelStateResponseFactory.Create(actionContext);

        // Assert
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var response = badRequestResult.Value.Should().BeOfType<StandardApiResponse<object>>().Subject;

        response.Success.Should().BeFalse();
        response.StatusCode.Should().Be(400);
        response.Message.Should().Be("Validation failed");
        response.Error.Should().NotBeNull();
        response.Error!.Status.Should().Be(400);
        response.Error.Title.Should().Be("Validation error");
        response.Error.Instance.Should().Be("/api/test");

        response.Error.Extensions.Should().ContainKey("traceId")
            .WhoseValue.Should().Be("trace-id");

        response.Error.Extensions.Should().ContainKey("errors");
        var errors = response.Error.Extensions["errors"] as Dictionary<string, string[]>;
        errors.Should().NotBeNull();
        errors.Should().HaveCount(2);
        errors!["Field1"].Should().ContainSingle().Which.Should().Be("Error 1");
        errors["Field2"].Should().HaveCount(2).And.Contain(ExpectedField2Errors);
    }

    [Fact]
    public void Create_WithEmptyModelState_ReturnsBadRequestWithEmptyErrors()
    {
        // Arrange
        var modelState = new ModelStateDictionary();
        var httpContext = new DefaultHttpContext();
        var actionContext = new ActionContext(
            httpContext,
            new RouteData(),
            new ActionDescriptor(),
            modelState
        );

        // Act
        var result = ModelStateResponseFactory.Create(actionContext);

        // Assert
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var response = badRequestResult.Value.Should().BeOfType<StandardApiResponse<object>>().Subject;

        response.Success.Should().BeFalse();
        response.Error.Should().NotBeNull();
        response.Error!.Extensions.Should().ContainKey("errors");
        var errors = response.Error.Extensions["errors"] as Dictionary<string, string[]>;
        errors.Should().NotBeNull();
        errors.Should().BeEmpty();
    }
}
