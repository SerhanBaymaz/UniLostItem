using Application.Features.Auth.Common.DTOs;
using Application.Features.Auth.Commands.Login;
using Application.Interfaces;
using Domain;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace Tests.Application_Tests.Features.Auth.Commands.Login;

public class LoginCommandHandlerTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly IConfiguration _configuration;
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
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

        _handler = new LoginCommandHandler(_userManagerMock.Object, _jwtServiceMock.Object, _configuration);
    }

    [Fact]
    public async Task Handle_Should_Return_Failure_When_User_Not_Found()
    {
        // Arrange
        var command = new LoginCommand
        {
            LoginDto = new LoginDto { Email = "nonexistent@test.com", Password = "Password1!" }
        };

        _userManagerMock.Setup(x => x.FindByEmailAsync(command.LoginDto.Email))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Invalid email or password.");
    }

    [Fact]
    public async Task Handle_Should_Return_Failure_When_Password_Incorrect()
    {
        // Arrange
        var command = new LoginCommand
        {
            LoginDto = new LoginDto { Email = "test@test.com", Password = "WrongPassword!" }
        };
        var user = new ApplicationUser { Email = command.LoginDto.Email };

        _userManagerMock.Setup(x => x.FindByEmailAsync(command.LoginDto.Email))
            .ReturnsAsync(user);

        _userManagerMock.Setup(x => x.CheckPasswordAsync(user, command.LoginDto.Password))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Invalid email or password.");
    }

    [Fact]
    public async Task Handle_Should_Return_Success_When_Credentials_Correct()
    {
        // Arrange
        var command = new LoginCommand
        {
            LoginDto = new LoginDto { Email = "test@test.com", Password = "CorrectPassword1!" }
        };

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = command.LoginDto.Email,
            FirstName = "Test",
            LastName = "User"
        };

        _userManagerMock.Setup(x => x.FindByEmailAsync(command.LoginDto.Email))
            .ReturnsAsync(user);

        _userManagerMock.Setup(x => x.CheckPasswordAsync(user, command.LoginDto.Password))
            .ReturnsAsync(true);

        _userManagerMock.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "BaseUser" });

        _jwtServiceMock.Setup(x => x.GenerateAccessToken(user, It.IsAny<IList<string>>()))
            .Returns("access_token");

        _jwtServiceMock.Setup(x => x.GenerateRefreshToken())
            .Returns("refresh_token");

        _userManagerMock.Setup(x => x.UpdateAsync(user))

            .ReturnsAsync(IdentityResult.Success);



        // Act

        var result = await _handler.Handle(command, CancellationToken.None);



        // Assert

        result.IsSuccess.Should().BeTrue();

        result.Value.Should().NotBeNull();

        result.Value!.AccessToken.Should().Be("access_token");

        result.Value.RefreshToken.Should().Be("refresh_token");



        _userManagerMock.Verify(x => x.UpdateAsync(user), Times.Once);

    }



    [Fact]

    public async Task Handle_Should_Return_Failure_When_UpdateUser_Fails()

    {

        // Arrange

        var command = new LoginCommand

        {

            LoginDto = new LoginDto { Email = "test@test.com", Password = "CorrectPassword1!" }

        };



        var user = new ApplicationUser { Email = command.LoginDto.Email };



        _userManagerMock.Setup(x => x.FindByEmailAsync(command.LoginDto.Email))

            .ReturnsAsync(user);



        _userManagerMock.Setup(x => x.CheckPasswordAsync(user, command.LoginDto.Password))

            .ReturnsAsync(true);



        _userManagerMock.Setup(x => x.GetRolesAsync(user))

            .ReturnsAsync(new List<string> { "BaseUser" });



        _jwtServiceMock.Setup(x => x.GenerateAccessToken(user, It.IsAny<IList<string>>()))

            .Returns("access_token");



        _jwtServiceMock.Setup(x => x.GenerateRefreshToken())

            .Returns("refresh_token");



        _userManagerMock.Setup(x => x.UpdateAsync(user))

            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Update failed" }));



        // Act

        var result = await _handler.Handle(command, CancellationToken.None);



        // Assert

        result.IsSuccess.Should().BeFalse();

        result.Message.Should().Be("Failed to update user tokens.");

    }

}

