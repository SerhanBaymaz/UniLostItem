using Application.Features.Auth.Common.DTOs;
using Application.Features.Auth.Commands.Register;
using Application.Interfaces;
using Domain;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;

namespace Tests.Application_Tests.Features.Auth.Commands.Register;

public class RegisterCommandHandlerTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly RegisterCommandHandler _handler;

    public RegisterCommandHandlerTests()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        
        _jwtServiceMock = new Mock<IJwtService>();
        
        _handler = new RegisterCommandHandler(_userManagerMock.Object, _jwtServiceMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_Failure_When_Email_Exists()
    {
        // Arrange
        var command = new RegisterCommand 
        { 
            RegisterDto = new RegisterDto { Email = "existing@test.com" }
        };
        _userManagerMock.Setup(x => x.FindByEmailAsync(command.RegisterDto.Email))
            .ReturnsAsync(new ApplicationUser()); // Kullanıcı var

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("Email is already taken");
    }

    [Fact]
    public async Task Handle_Should_Return_Failure_When_CreateUser_Fails()
    {
        // Arrange
        var command = new RegisterCommand
        {
            RegisterDto = new RegisterDto { Email = "fail@test.com", Password = "Password1!" }
        };

        _userManagerMock.Setup(x => x.FindByEmailAsync(command.RegisterDto.Email))
            .ReturnsAsync((ApplicationUser?)null);

        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), command.RegisterDto.Password))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Password too weak" }));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("Registration failed");
        result.Message.Should().Contain("Password too weak");
    }

    [Fact]
    public async Task Handle_Should_Return_Failure_When_RoleAssignment_Fails()
    {
        // Arrange
        var command = new RegisterCommand
        {
            RegisterDto = new RegisterDto { Email = "rolefail@test.com", Password = "Password1!" }
        };

        _userManagerMock.Setup(x => x.FindByEmailAsync(command.RegisterDto.Email))
            .ReturnsAsync((ApplicationUser?)null);

        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), command.RegisterDto.Password))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "BaseUser"))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Role not found" }));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("Failed to assign role");
    }

    [Fact]
    public async Task Handle_Should_Return_Success_When_Registration_Successful()
    {
        // Arrange
        var command = new RegisterCommand
        {
            RegisterDto = new RegisterDto 
            {
                Email = "new@test.com",
                Password = "Password1!",
                FirstName = "New",
                LastName = "User"
            }
        };

        _userManagerMock.Setup(x => x.FindByEmailAsync(command.RegisterDto.Email))
            .ReturnsAsync((ApplicationUser?)null); // Kullanıcı yok

        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), command.RegisterDto.Password))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "BaseUser"))
            .ReturnsAsync(IdentityResult.Success);
            
        _userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(new List<string> { "BaseUser" });

        _jwtServiceMock.Setup(x => x.GenerateAccessToken(It.IsAny<ApplicationUser>(), It.IsAny<IList<string>>()))
            .Returns("access_token");

        _jwtServiceMock.Setup(x => x.GenerateRefreshToken())
            .Returns("refresh_token");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Email.Should().Be(command.RegisterDto.Email);
        result.Value.AccessToken.Should().Be("access_token");
        result.Value.RefreshToken.Should().Be("refresh_token");
        
        // Refresh token update çağrıldı mı?
        _userManagerMock.Verify(x => x.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Once);
    }
}
