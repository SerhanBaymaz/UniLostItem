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

public class BaseApiControllerTests
{
    private class TestController : BaseApiController
    {
        public new IMediator Mediator => base.Mediator;

        public ActionResult<StandardApiResponse<T>> TestHandleResult<T>(Result<T> result)
        {
            return HandleResult(result);
        }

        public string TestGetTraceId()
        {
            return GetTraceId();
        }
    }

    private readonly TestController _controller;
    private readonly Mock<IMediator> _mediatorMock;

    public BaseApiControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new TestController();

        // Setup HttpContext
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = "/api/test";
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
    public void Mediator_ShouldReturnMediator_WhenServiceAvailable()
    {
        // Act
        var mediator = _controller.Mediator;

        // Assert
        mediator.Should().NotBeNull();
        mediator.Should().Be(_mediatorMock.Object);
    }

    [Fact]
    public void Mediator_ShouldThrowException_WhenServiceUnavailable()
    {
        // Arrange
        var controller = new TestController();
        var httpContext = new DefaultHttpContext();
        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(sp => sp.GetService(typeof(IMediator)))!
            .Returns((IMediator)null!);
        httpContext.RequestServices = serviceProviderMock.Object;
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

        // Act
        Action act = () => { var _ = controller.Mediator; };

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("IMediator service is unavailable");
    }

    [Fact]
    public void GetTraceId_ShouldReturnTraceIdentifier()
    {
        // Act
        var traceId = _controller.TestGetTraceId();

        // Assert
        traceId.Should().NotBeNullOrEmpty();
        traceId.Should().Be("test-trace-id");
    }

    [Fact]
    public void HandleResult_ShouldReturnOk_WhenResultIsSuccess()
    {
        // Arrange
        var successResult = Result<string>.Success("Operation successful", "Test Value");

        // Act
        var actionResult = _controller.TestHandleResult(successResult);

        // Assert
        actionResult.Should().NotBeNull();
        actionResult.Result.Should().BeOfType<OkObjectResult>();

        var okResult = actionResult.Result as OkObjectResult;
        okResult!.Value.Should().BeOfType<StandardApiResponse<string>>();

        var response = okResult.Value as StandardApiResponse<string>;
        response!.Success.Should().BeTrue();
        response.Data.Should().Be("Test Value");
        response.Message.Should().Be("Operation successful");
        response.StatusCode.Should().Be(200);
    }

    [Fact]
    public void HandleResult_ShouldReturnNotFound_WhenResultIsFailureWith404()
    {
        // Arrange
        var notFoundResult = Result<string>.Failure("Resource not found", 404);

        // Act
        var actionResult = _controller.TestHandleResult(notFoundResult);

        // Assert
        actionResult.Should().NotBeNull();
        actionResult.Result.Should().BeOfType<NotFoundObjectResult>();

        var notFoundObjResult = actionResult.Result as NotFoundObjectResult;
        notFoundObjResult!.Value.Should().BeOfType<StandardApiResponse<string>>();

        var response = notFoundObjResult.Value as StandardApiResponse<string>;
        response!.Success.Should().BeFalse();
        response.Message.Should().Be("Resource not found");
        response.StatusCode.Should().Be(404);
        response.Error.Should().NotBeNull();
        response.Error!.Type.Should().Be("NotFound");
        response.Error.Title.Should().Be("Resource not found");
    }

    [Fact]
    public void HandleResult_ShouldReturnBadRequest_WhenResultIsFailureWithoutCode()
    {
        // Arrange
        var failureResult = Result<string>.Failure("Validation error", 400);

        // Act
        var actionResult = _controller.TestHandleResult(failureResult);

        // Assert
        actionResult.Should().NotBeNull();
        actionResult.Result.Should().BeOfType<BadRequestObjectResult>();

        var badRequestResult = actionResult.Result as BadRequestObjectResult;
        badRequestResult!.Value.Should().BeOfType<StandardApiResponse<string>>();

        var response = badRequestResult.Value as StandardApiResponse<string>;
        response!.Success.Should().BeFalse();
        response.Message.Should().Be("Validation error");
        response.StatusCode.Should().Be(400);
        response.Error.Should().NotBeNull();
        response.Error!.Type.Should().Be("BadRequest");
    }

    [Fact]
    public void HandleResult_ShouldIncludeTraceId_InErrorResponses()
    {
        // Arrange
        var failureResult = Result<string>.Failure("Error occurred", 404);

        // Act
        var actionResult = _controller.TestHandleResult(failureResult);

        // Assert
        actionResult.Should().NotBeNull();
        var notFoundResult = actionResult.Result as NotFoundObjectResult;
        var response = notFoundResult!.Value as StandardApiResponse<string>;

        response!.Error.Should().NotBeNull();
        response.Error!.Extensions.Should().ContainKey("traceId");
        response.Error!.Extensions["traceId"].Should().Be("test-trace-id");
    }

    [Fact]
    public void HandleResult_ShouldIncludePath_InErrorResponses()
    {
        // Arrange
        var failureResult = Result<string>.Failure("Error occurred", 400);

        // Act
        var actionResult = _controller.TestHandleResult(failureResult);

        // Assert
        actionResult.Should().NotBeNull();
        var badRequestResult = actionResult.Result as BadRequestObjectResult;
        var response = badRequestResult!.Value as StandardApiResponse<string>;

        response!.Error.Should().NotBeNull();
        response.Error!.Instance.Should().Be("/api/test");
    }

    [Fact]
    public void HandleResult_ShouldUseDefaultMessage_WhenResultMessageIsNull()
    {
        // Arrange
        var successResult = Result<string>.Success(null!, "Data");

        // Act
        var actionResult = _controller.TestHandleResult(successResult);

        // Assert
        var okResult = actionResult.Result as OkObjectResult;
        var response = okResult!.Value as StandardApiResponse<string>;

        response!.Message.Should().Be("Request completed successfully");
    }

    [Fact]
    public void HandleResult_ShouldHandleComplexTypes()
    {
        // Arrange
        var complexData = new List<int> { 1, 2, 3 };
        var successResult = Result<List<int>>.Success("List retrieved", complexData);

        // Act
        var actionResult = _controller.TestHandleResult(successResult);

        // Assert
        var okResult = actionResult.Result as OkObjectResult;
        var response = okResult!.Value as StandardApiResponse<List<int>>;

        response!.Success.Should().BeTrue();
        response.Data.Should().BeEquivalentTo(complexData);
        response.Message.Should().Be("List retrieved");
    }
}
