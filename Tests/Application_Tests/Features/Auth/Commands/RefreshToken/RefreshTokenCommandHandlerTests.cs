using System.Security.Claims;
using Application.Features.Auth.Commands.RefreshToken;
using Application.Interfaces;
using Domain;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace Tests.Application_Tests.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommandHandlerTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly IConfiguration _configuration;
    private readonly RefreshTokenCommandHandler _handler;

    public RefreshTokenCommandHandlerTests()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        _jwtServiceMock = new Mock<IJwtService>();

        var myConfiguration = new Dictionary<string, string>
        {
            {"Jwt:RefreshTokenExpirationDays", "7"}
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(myConfiguration!)
            .Build();

        _handler = new RefreshTokenCommandHandler(_userManagerMock.Object, _jwtServiceMock.Object, _configuration);
    }

    [Fact]
    public async Task Handle_Should_Return_Failure_When_AccessToken_Is_Invalid()
    {
        // Arrange
        var command = new RefreshTokenCommand
        {
            RefreshTokenDto = new RefreshTokenDto { AccessToken = "invalid_token", RefreshToken = "refresh_token" }
        };

        _jwtServiceMock.Setup(x => x.GetPrincipalFromExpiredToken(command.RefreshTokenDto.AccessToken))
            .Returns((ClaimsPrincipal?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Invalid access token.");
    }

    [Fact]
    public async Task Handle_Should_Return_Failure_When_Token_Claims_Are_Invalid()
    {
        // Arrange
        var command = new RefreshTokenCommand
        {
            RefreshTokenDto = new RefreshTokenDto { AccessToken = "token_without_claims", RefreshToken = "refresh_token" }
        };

        var claimsPrincipal = new ClaimsPrincipal();

        _jwtServiceMock.Setup(x => x.GetPrincipalFromExpiredToken(command.RefreshTokenDto.AccessToken))
            .Returns(claimsPrincipal);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Invalid token claims.");
    }

    [Fact]
    public async Task Handle_Should_Return_Failure_When_User_Not_Found()
    {
        // Arrange
        var command = new RefreshTokenCommand
        {
            RefreshTokenDto = new RefreshTokenDto { AccessToken = "valid_token", RefreshToken = "refresh_token" }
        };

        var claimsIdentity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "user-id-123")
        });

        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        _jwtServiceMock.Setup(x => x.GetPrincipalFromExpiredToken(command.RefreshTokenDto.AccessToken))
            .Returns(claimsPrincipal);

        _userManagerMock.Setup(x => x.FindByIdAsync("user-id-123"))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("User not found.");
    }

    [Fact]
    public async Task Handle_Should_Return_Failure_When_RefreshToken_Does_Not_Match()
    {
        // Arrange
        var command = new RefreshTokenCommand
        {
            RefreshTokenDto = new RefreshTokenDto { AccessToken = "valid_token", RefreshToken = "wrong_refresh_token" }
        };

        var claimsIdentity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "user-id-123")
        });

        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        var user = new ApplicationUser
        {
            Id = "user-id-123",
            Email = "test@test.com",
            RefreshToken = "correct_refresh_token",
            RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(1)
        };

        _jwtServiceMock.Setup(x => x.GetPrincipalFromExpiredToken(command.RefreshTokenDto.AccessToken))
            .Returns(claimsPrincipal);

        _userManagerMock.Setup(x => x.FindByIdAsync("user-id-123"))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Invalid refresh token.");
    }

    [Fact]
    public async Task Handle_Should_Return_Failure_When_RefreshToken_Is_Expired()
    {
        // Arrange
        var command = new RefreshTokenCommand
        {
            RefreshTokenDto = new RefreshTokenDto { AccessToken = "valid_token", RefreshToken = "expired_refresh_token" }
        };

        var claimsIdentity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "user-id-123")
        });

        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        var user = new ApplicationUser
        {
            Id = "user-id-123",
            Email = "test@test.com",
            RefreshToken = "expired_refresh_token",
            RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(-1) // Expired
        };

        _jwtServiceMock.Setup(x => x.GetPrincipalFromExpiredToken(command.RefreshTokenDto.AccessToken))
            .Returns(claimsPrincipal);

        _userManagerMock.Setup(x => x.FindByIdAsync("user-id-123"))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Refresh token has expired. Please login again.");
    }

    [Fact]
    public async Task Handle_Should_Return_Success_When_Tokens_Are_Valid()
    {
        // Arrange
        var command = new RefreshTokenCommand
        {
            RefreshTokenDto = new RefreshTokenDto { AccessToken = "valid_token", RefreshToken = "valid_refresh_token" }
        };

        var claimsIdentity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "user-id-123")
        });

        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        var user = new ApplicationUser
        {
            Id = "user-id-123",
            Email = "test@test.com",
            FirstName = "Test",
            LastName = "User",
            RefreshToken = "valid_refresh_token",
            RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(1)
        };

        _jwtServiceMock.Setup(x => x.GetPrincipalFromExpiredToken(command.RefreshTokenDto.AccessToken))
            .Returns(claimsPrincipal);

        _userManagerMock.Setup(x => x.FindByIdAsync("user-id-123"))
            .ReturnsAsync(user);

        _userManagerMock.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "BaseUser" });

        _jwtServiceMock.Setup(x => x.GenerateAccessToken(user, It.IsAny<IList<string>>()))
            .Returns("new_access_token");

        _jwtServiceMock.Setup(x => x.GenerateRefreshToken())
            .Returns("new_refresh_token");

        _userManagerMock.Setup(x => x.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.AccessToken.Should().Be("new_access_token");
        result.Value.RefreshToken.Should().Be("new_refresh_token");
        user.RefreshToken.Should().Be("new_refresh_token");

        _userManagerMock.Verify(x => x.UpdateAsync(user), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Return_Failure_When_UpdateUser_Fails()
    {
        // Arrange
        var command = new RefreshTokenCommand
        {
            RefreshTokenDto = new RefreshTokenDto { AccessToken = "valid_token", RefreshToken = "valid_refresh_token" }
        };

        var claimsIdentity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "user-id-123")
        });

        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        var user = new ApplicationUser
        {
            Id = "user-id-123",
            Email = "test@test.com",
            FirstName = "Test",
            LastName = "User",
            RefreshToken = "valid_refresh_token",
            RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(1)
        };

        _jwtServiceMock.Setup(x => x.GetPrincipalFromExpiredToken(command.RefreshTokenDto.AccessToken))
            .Returns(claimsPrincipal);

        _userManagerMock.Setup(x => x.FindByIdAsync("user-id-123"))
            .ReturnsAsync(user);

        _userManagerMock.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "BaseUser" });

        _jwtServiceMock.Setup(x => x.GenerateAccessToken(user, It.IsAny<IList<string>>()))
            .Returns("new_access_token");

        _jwtServiceMock.Setup(x => x.GenerateRefreshToken())
            .Returns("new_refresh_token");

        _userManagerMock.Setup(x => x.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Update failed" }));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Failed to update user tokens.");
    }
}
