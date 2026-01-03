using Application.Features.Auth.DTOs;
using Application.Features.Auth.RegisterUser;
using Application.Interfaces;
using Domain;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;

namespace Tests.Application_Tests.Features.Auth.RegisterUser;

public class RegisterUserCommandHandlerTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly RegisterUserCommandHandler _handler;

    public RegisterUserCommandHandlerTests()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        // UserManager constructor parametrelerini null! ile geçerek derleyici hatasını aşıyoruz
        // çünkü unit testte bu alt bağımlılıkları kullanmayacağız.
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        
        _jwtServiceMock = new Mock<IJwtService>();
        
        _handler = new RegisterUserCommandHandler(_userManagerMock.Object, _jwtServiceMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_Failure_When_Email_Exists()
    {
        // Arrange
        var command = new RegisterUserCommand { Email = "existing@test.com" };
        _userManagerMock.Setup(x => x.FindByEmailAsync(command.Email))
            .ReturnsAsync(new ApplicationUser()); // Kullanıcı var

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("Email is already taken");
    }

    [Fact]
    public async Task Handle_Should_Return_Success_When_Registration_Successful()
    {
        // Arrange
        var command = new RegisterUserCommand
        {
            Email = "new@test.com",
            Password = "Password1!",
            FirstName = "New",
            LastName = "User"
        };

        _userManagerMock.Setup(x => x.FindByEmailAsync(command.Email))
            .ReturnsAsync((ApplicationUser?)null); // Kullanıcı yok

        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), command.Password))
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
        result.Value!.Email.Should().Be(command.Email);
        result.Value.AccessToken.Should().Be("access_token");
        result.Value.RefreshToken.Should().Be("refresh_token");
        
        // Refresh token update çağrıldı mı?
        _userManagerMock.Verify(x => x.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Once);
    }
}