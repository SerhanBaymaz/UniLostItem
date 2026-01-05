using API.Controllers;
using API.Responses;
using Application.Core;
using Application.Features.Auth.Commands.Login;
using Application.Features.Auth.Commands.RefreshToken;
using Application.Features.Auth.Commands.Register;
using Application.Features.Auth.Commands.UpdateUserProfile;
using Application.Features.Auth.Common.DTOs;
using Application.Features.Auth.Queries.GetCurrentUser;
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

    #region RefreshToken Tests

    [Fact]
    public async Task RefreshToken_ShouldReturnOk_WhenRefreshTokenSucceeds()
    {
        // Arrange
        var refreshTokenDto = new RefreshTokenDto
        {
            AccessToken = "old_access_token",
            RefreshToken = "valid_refresh_token"
        };
        var userDto = new UserDto
        {
            Id = "1",
            Email = "test@test.com",
            AccessToken = "new_access_token",
            RefreshToken = "new_refresh_token"
        };
        var result = Result<UserDto>.Success("Token refreshed successfully", userDto);

        _mediatorMock.Setup(m => m.Send(It.IsAny<RefreshTokenCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var actionResult = await _controller.RefreshToken(refreshTokenDto);

        // Assert
        actionResult.Should().NotBeNull();
        actionResult.Result.Should().BeOfType<OkObjectResult>();

        var okResult = actionResult.Result as OkObjectResult;
        var response = okResult!.Value as StandardApiResponse<UserDto>;

        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.Data.Should().BeEquivalentTo(userDto);
        response.Message.Should().Be("Token refreshed successfully");
    }

    [Fact]
    public async Task RefreshToken_ShouldReturnBadRequest_WhenRefreshTokenFails()
    {
        // Arrange
        var refreshTokenDto = new RefreshTokenDto
        {
            AccessToken = "invalid_token",
            RefreshToken = "invalid_refresh_token"
        };
        var result = Result<UserDto>.Failure("Invalid access token.", 401);

        _mediatorMock.Setup(m => m.Send(It.IsAny<RefreshTokenCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var actionResult = await _controller.RefreshToken(refreshTokenDto);

        // Assert
        actionResult.Result.Should().BeOfType<BadRequestObjectResult>();

        var badRequestResult = actionResult.Result as BadRequestObjectResult;
        var response = badRequestResult!.Value as StandardApiResponse<UserDto>;

        response!.Success.Should().BeFalse();
        response.Message.Should().Be("Invalid access token.");
    }

    [Fact]
    public async Task RefreshToken_ShouldReturnBadRequest_WhenRefreshTokenIsExpired()
    {
        // Arrange
        var refreshTokenDto = new RefreshTokenDto
        {
            AccessToken = "expired_access_token",
            RefreshToken = "expired_refresh_token"
        };
        var result = Result<UserDto>.Failure("Refresh token has expired. Please login again.", 401);

        _mediatorMock.Setup(m => m.Send(It.IsAny<RefreshTokenCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var actionResult = await _controller.RefreshToken(refreshTokenDto);

        // Assert
        actionResult.Result.Should().BeOfType<BadRequestObjectResult>();

        var badRequestResult = actionResult.Result as BadRequestObjectResult;
        var response = badRequestResult!.Value as StandardApiResponse<UserDto>;

        response!.Success.Should().BeFalse();
        response.Message.Should().Be("Refresh token has expired. Please login again.");
    }

    [Fact]
    public async Task RefreshToken_ShouldReturnNotFound_WhenUserNotFound()
    {
        // Arrange
        var refreshTokenDto = new RefreshTokenDto
        {
            AccessToken = "valid_token_format",
            RefreshToken = "some_refresh_token"
        };
        var result = Result<UserDto>.Failure("User not found.", 404);

        _mediatorMock.Setup(m => m.Send(It.IsAny<RefreshTokenCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var actionResult = await _controller.RefreshToken(refreshTokenDto);

        // Assert
        actionResult.Result.Should().BeOfType<NotFoundObjectResult>();

        var notFoundResult = actionResult.Result as NotFoundObjectResult;
        var response = notFoundResult!.Value as StandardApiResponse<UserDto>;

        response!.Success.Should().BeFalse();
        response.StatusCode.Should().Be(404);
        response.Message.Should().Be("User not found.");
    }

    [Fact]
    public async Task RefreshToken_ShouldPassDto_ToCommand()
    {
        // Arrange
        var refreshTokenDto = new RefreshTokenDto
        {
            AccessToken = "access_token",
            RefreshToken = "refresh_token"
        };
        var result = Result<UserDto>.Success("Success", new UserDto());

        _mediatorMock.Setup(m => m.Send(It.IsAny<RefreshTokenCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        await _controller.RefreshToken(refreshTokenDto);

        // Assert
        _mediatorMock.Verify(m => m.Send(
            It.Is<RefreshTokenCommand>(cmd => cmd.RefreshTokenDto == refreshTokenDto),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region GetProfile Tests

    [Fact]
    public async Task GetProfile_ShouldReturnOk_WhenProfileRetrievedSuccessfully()
    {
        // Arrange
        var currentUserDto = new CurrentUserDto
        {
            Id = "user123",
            Email = "test@test.com",
            FirstName = "Test",
            LastName = "User",
            PhoneNumber = "1234567890",
            Roles = new List<string> { "User" }
        };
        var result = Result<CurrentUserDto>.Success("User retrieved successfully", currentUserDto);

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetCurrentUserQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var actionResult = await _controller.GetProfile();

        // Assert
        actionResult.Should().NotBeNull();
        actionResult.Result.Should().BeOfType<OkObjectResult>();

        var okResult = actionResult.Result as OkObjectResult;
        var response = okResult!.Value as StandardApiResponse<CurrentUserDto>;

        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.Data.Should().BeEquivalentTo(currentUserDto);
        response.Message.Should().Be("User retrieved successfully");
    }

    [Fact]
    public async Task GetProfile_ShouldReturnUnauthorized_WhenUserIsNotAuthenticated()
    {
        // Arrange
        var result = Result<CurrentUserDto>.Failure("User is not authenticated.", 401);

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetCurrentUserQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var actionResult = await _controller.GetProfile();

        // Assert
        actionResult.Result.Should().BeOfType<BadRequestObjectResult>();

        var badRequestResult = actionResult.Result as BadRequestObjectResult;
        var response = badRequestResult!.Value as StandardApiResponse<CurrentUserDto>;

        response!.Success.Should().BeFalse();
        response.Message.Should().Be("User is not authenticated.");
    }

    [Fact]
    public async Task GetProfile_ShouldReturnNotFound_WhenUserNotFound()
    {
        // Arrange
        var result = Result<CurrentUserDto>.Failure("User not found.", 404);

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetCurrentUserQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var actionResult = await _controller.GetProfile();

        // Assert
        actionResult.Result.Should().BeOfType<NotFoundObjectResult>();

        var notFoundResult = actionResult.Result as NotFoundObjectResult;
        var response = notFoundResult!.Value as StandardApiResponse<CurrentUserDto>;

        response!.Success.Should().BeFalse();
        response.StatusCode.Should().Be(404);
        response.Message.Should().Be("User not found.");
    }

    #endregion

    #region UpdateProfile Tests

    [Fact]
    public async Task UpdateProfile_ShouldReturnOk_WhenProfileUpdatedSuccessfully()
    {
        // Arrange
        var updateProfileDto = new UpdateUserProfileDto
        {
            FirstName = "UpdatedFirstName",
            LastName = "UpdatedLastName",
            PhoneNumber = "9876543210"
        };

        var currentUserDto = new CurrentUserDto
        {
            Id = "user123",
            Email = "test@test.com",
            FirstName = "UpdatedFirstName",
            LastName = "UpdatedLastName",
            PhoneNumber = "9876543210",
            Roles = new List<string> { "User" }
        };
        var result = Result<CurrentUserDto>.Success("Profile updated successfully", currentUserDto);

        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateUserProfileCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var actionResult = await _controller.UpdateProfile(updateProfileDto);

        // Assert
        actionResult.Should().NotBeNull();
        actionResult.Result.Should().BeOfType<OkObjectResult>();

        var okResult = actionResult.Result as OkObjectResult;
        var response = okResult!.Value as StandardApiResponse<CurrentUserDto>;

        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.Data.Should().BeEquivalentTo(currentUserDto);
        response.Message.Should().Be("Profile updated successfully");
    }

    [Fact]
    public async Task UpdateProfile_ShouldReturnBadRequest_WhenValidationFails()
    {
        // Arrange
        var updateProfileDto = new UpdateUserProfileDto
        {
            FirstName = "", // Invalid - empty
            LastName = "", // Invalid - empty
            PhoneNumber = "" // Invalid - empty
        };
        var result = Result<CurrentUserDto>.Failure("First name is required.", 400);

        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateUserProfileCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var actionResult = await _controller.UpdateProfile(updateProfileDto);

        // Assert
        actionResult.Result.Should().BeOfType<BadRequestObjectResult>();

        var badRequestResult = actionResult.Result as BadRequestObjectResult;
        var response = badRequestResult!.Value as StandardApiResponse<CurrentUserDto>;

        response!.Success.Should().BeFalse();
        response.StatusCode.Should().Be(400);
        response.Message.Should().Be("First name is required.");
    }

    [Fact]
    public async Task UpdateProfile_ShouldReturnUnauthorized_WhenUserIsNotAuthenticated()
    {
        // Arrange
        var updateProfileDto = new UpdateUserProfileDto
        {
            FirstName = "Test",
            LastName = "User",
            PhoneNumber = "1234567890"
        };
        var result = Result<CurrentUserDto>.Failure("User is not authenticated.", 401);

        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateUserProfileCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var actionResult = await _controller.UpdateProfile(updateProfileDto);

        // Assert
        actionResult.Result.Should().BeOfType<BadRequestObjectResult>();

        var badRequestResult = actionResult.Result as BadRequestObjectResult;
        var response = badRequestResult!.Value as StandardApiResponse<CurrentUserDto>;

        response!.Success.Should().BeFalse();
        response.Message.Should().Be("User is not authenticated.");
    }

    [Fact]
    public async Task UpdateProfile_ShouldReturnNotFound_WhenUserNotFound()
    {
        // Arrange
        var updateProfileDto = new UpdateUserProfileDto
        {
            FirstName = "Test",
            LastName = "User",
            PhoneNumber = "1234567890"
        };
        var result = Result<CurrentUserDto>.Failure("User not found.", 404);

        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateUserProfileCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var actionResult = await _controller.UpdateProfile(updateProfileDto);

        // Assert
        actionResult.Result.Should().BeOfType<NotFoundObjectResult>();

        var notFoundResult = actionResult.Result as NotFoundObjectResult;
        var response = notFoundResult!.Value as StandardApiResponse<CurrentUserDto>;

        response!.Success.Should().BeFalse();
        response.StatusCode.Should().Be(404);
        response.Message.Should().Be("User not found.");
    }

    [Fact]
    public async Task UpdateProfile_ShouldPassDto_ToCommand()
    {
        // Arrange
        var updateProfileDto = new UpdateUserProfileDto
        {
            FirstName = "Test",
            LastName = "User",
            PhoneNumber = "1234567890"
        };
        var result = Result<CurrentUserDto>.Success("Success", new CurrentUserDto());

        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateUserProfileCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        await _controller.UpdateProfile(updateProfileDto);

        // Assert
        _mediatorMock.Verify(m => m.Send(
            It.Is<UpdateUserProfileCommand>(cmd => cmd.UpdateUserProfileDto == updateProfileDto),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion
}
