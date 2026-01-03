using API.Controllers;
using API.Responses;
using Application.Core;
using Application.Features.Auth.Common.DTOs;
using Application.Features.Auth.Commands.Login;
using Application.Features.Auth.Commands.Register;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Tests.API_Tests.Controllers;

public class AuthControllerTests
{
    private readonly AuthController _controller;
    private readonly Mock<IMediator> _mediatorMock;

    public AuthControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new AuthController();

        // Setup HttpContext
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = "/api/v1/auth";
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

    #region Register Tests

    [Fact]
    public async Task Register_ShouldReturnOk_WhenRegistrationSucceeds()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Email = "new@test.com",
            Password = "Password1!",
            FirstName = "Test",
            LastName = "User"
        };
        var userDto = new UserDto { Id = "1", Email = registerDto.Email };
        var result = Result<UserDto>.Success("User registered successfully", userDto);

        _mediatorMock.Setup(m => m.Send(It.IsAny<RegisterCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var actionResult = await _controller.Register(registerDto);

        // Assert
        actionResult.Should().NotBeNull();
        actionResult.Result.Should().BeOfType<OkObjectResult>();

        var okResult = actionResult.Result as OkObjectResult;
        var response = okResult!.Value as StandardApiResponse<UserDto>;

        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.Data.Should().BeEquivalentTo(userDto);
        response.Message.Should().Be("User registered successfully");
    }

    [Fact]
    public async Task Register_ShouldReturnBadRequest_WhenRegistrationFails()
    {
        // Arrange
        var registerDto = new RegisterDto { Email = "fail@test.com", Password = "weak" };
        var result = Result<UserDto>.Failure("Registration failed", 400);

        _mediatorMock.Setup(m => m.Send(It.IsAny<RegisterCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var actionResult = await _controller.Register(registerDto);

        // Assert
        actionResult.Result.Should().BeOfType<BadRequestObjectResult>();

        var badRequestResult = actionResult.Result as BadRequestObjectResult;
        var response = badRequestResult!.Value as StandardApiResponse<UserDto>;

        response!.Success.Should().BeFalse();
        response.StatusCode.Should().Be(400);
        response.Message.Should().Be("Registration failed");
    }

    [Fact]
    public async Task Register_ShouldPassDto_ToCommand()
    {
        // Arrange
        var registerDto = new RegisterDto { Email = "test@test.com" };
        var result = Result<UserDto>.Success("Success", new UserDto());

        _mediatorMock.Setup(m => m.Send(It.IsAny<RegisterCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        await _controller.Register(registerDto);

        // Assert
        _mediatorMock.Verify(m => m.Send(
            It.Is<RegisterCommand>(cmd => cmd.RegisterDto == registerDto),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region Login Tests

    [Fact]
    public async Task Login_ShouldReturnOk_WhenLoginSucceeds()
    {
        // Arrange
        var loginDto = new LoginDto { Email = "test@test.com", Password = "Password1!" };
        var userDto = new UserDto { Id = "1", Email = loginDto.Email, AccessToken = "token" };
        var result = Result<UserDto>.Success("Login successful", userDto);

        _mediatorMock.Setup(m => m.Send(It.IsAny<LoginCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var actionResult = await _controller.Login(loginDto);

        // Assert
        actionResult.Should().NotBeNull();
        actionResult.Result.Should().BeOfType<OkObjectResult>();

        var okResult = actionResult.Result as OkObjectResult;
        var response = okResult!.Value as StandardApiResponse<UserDto>;

        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.Data.Should().BeEquivalentTo(userDto);
        response.Message.Should().Be("Login successful");
    }

    [Fact]
    public async Task Login_ShouldReturnBadRequest_WhenLoginFails()
    {
        // Note: BaseApiController returns BadRequest for failures by default (or NotFound for 404).
        // If LoginHandler returns failure, BaseApiController wraps it in BadRequest (400) 
        // unless code is 404.
        
        // Arrange
        var loginDto = new LoginDto { Email = "fail@test.com", Password = "wrong" };
        var result = Result<UserDto>.Failure("Invalid email or password.", 401); 
        // Note: BaseApiController treats failure as BadRequest (400) by default for now, 
        // or we need to update BaseApiController to handle 401 Unauthorized if needed.
        // Let's check BaseApiController logic. It returns BadRequest for failure unless 404.
        // So even if result code is 401, BaseApiController might return 400 BadRequest with the message.
        
        _mediatorMock.Setup(m => m.Send(It.IsAny<LoginCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var actionResult = await _controller.Login(loginDto);

        // Assert
        actionResult.Result.Should().BeOfType<BadRequestObjectResult>();
        
        var badRequestResult = actionResult.Result as BadRequestObjectResult;
        var response = badRequestResult!.Value as StandardApiResponse<UserDto>;
        
        response!.Success.Should().BeFalse();
        response.Message.Should().Be("Invalid email or password.");
    }

    [Fact]
    public async Task Login_ShouldPassDto_ToCommand()
    {
        // Arrange
        var loginDto = new LoginDto { Email = "test@test.com" };
        var result = Result<UserDto>.Success("Success", new UserDto());

        _mediatorMock.Setup(m => m.Send(It.IsAny<LoginCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        await _controller.Login(loginDto);

        // Assert
        _mediatorMock.Verify(m => m.Send(
            It.Is<LoginCommand>(cmd => cmd.LoginDto == loginDto),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion
}
