using Application.Core;
using Application.Features.Auth.Commands.UpdateUserProfile;
using Application.Features.Auth.Queries.GetCurrentUser;
using Application.Interfaces;
using Domain;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;

namespace Tests.Application_Tests.Features.Auth.Commands.UpdateUserProfile;

public class UpdateUserProfileCommandTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly UpdateUserProfileCommandHandler _handler;

    public UpdateUserProfileCommandTests()
    {
        _userManagerMock = MockUserManager();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _handler = new UpdateUserProfileCommandHandler(_userManagerMock.Object, _currentUserServiceMock.Object);
    }

    private static Mock<UserManager<ApplicationUser>> MockUserManager()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        return new Mock<UserManager<ApplicationUser>>(
            store.Object,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!);
    }

    #region Success Tests

    [Fact]
    public async Task Handle_ShouldUpdateUserProfile_WhenDataIsValid()
    {
        // Arrange
        var userId = "user123";
        var user = new ApplicationUser
        {
            Id = userId,
            Email = "test@test.com",
            FirstName = "OldFirstName",
            LastName = "OldLastName",
            PhoneNumber = "0000000000"
        };

        var updateDto = new UpdateUserProfileDto
        {
            FirstName = "NewFirstName",
            LastName = "NewLastName",
            PhoneNumber = "1234567890"
        };

        var command = new UpdateUserProfileCommand { UpdateUserProfileDto = updateDto };

        _currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);

        _userManagerMock.Setup(x => x.FindByIdAsync(userId))
            .ReturnsAsync(user);

        _userManagerMock.Setup(x => x.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "User" });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.FirstName.Should().Be("NewFirstName");
        result.Value.LastName.Should().Be("NewLastName");
        result.Value.PhoneNumber.Should().Be("1234567890");
        result.Message.Should().Be("Profile updated successfully");

        // Verify user was updated
        user.FirstName.Should().Be("NewFirstName");
        user.LastName.Should().Be("NewLastName");
        user.PhoneNumber.Should().Be("1234567890");
    }

    [Fact]
    public async Task Handle_ShouldReturnUpdatedUserWithRoles_WhenUpdateSucceeds()
    {
        // Arrange
        var userId = "admin123";
        var user = new ApplicationUser
        {
            Id = userId,
            Email = "admin@test.com",
            FirstName = "Admin",
            LastName = "User",
            PhoneNumber = "9876543210"
        };
        var roles = new List<string> { "Admin", "Moderator" };

        var updateDto = new UpdateUserProfileDto
        {
            FirstName = "SuperAdmin",
            LastName = "SuperUser",
            PhoneNumber = "5555555555"
        };

        var command = new UpdateUserProfileCommand { UpdateUserProfileDto = updateDto };

        _currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);

        _userManagerMock.Setup(x => x.FindByIdAsync(userId))
            .ReturnsAsync(user);

        _userManagerMock.Setup(x => x.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(roles);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Roles.Should().HaveCount(2);
        result.Value.Roles.Should().Contain("Admin");
        result.Value.Roles.Should().Contain("Moderator");
    }

    #endregion

    #region Authentication Failure Tests

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserIsNotAuthenticated()
    {
        // Arrange
        _currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(false);
        _currentUserServiceMock.Setup(x => x.UserId).Returns((string?)null);

        var command = new UpdateUserProfileCommand
        {
            UpdateUserProfileDto = new UpdateUserProfileDto()
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("User is not authenticated.");
        result.Code.Should().Be(401);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserIdIsNull()
    {
        // Arrange
        _currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(x => x.UserId).Returns((string?)null);

        var command = new UpdateUserProfileCommand
        {
            UpdateUserProfileDto = new UpdateUserProfileDto()
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("User is not authenticated.");
        result.Code.Should().Be(401);
    }

    #endregion

    #region User Not Found Tests

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserNotFound()
    {
        // Arrange
        var userId = "nonexistent123";

        _currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);

        _userManagerMock.Setup(x => x.FindByIdAsync(userId))
            .ReturnsAsync((ApplicationUser?)null);

        var command = new UpdateUserProfileCommand
        {
            UpdateUserProfileDto = new UpdateUserProfileDto()
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("User not found.");
        result.Code.Should().Be(404);
    }

    #endregion

    #region Update Failure Tests

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUpdateFails()
    {
        // Arrange
        var userId = "user123";
        var user = new ApplicationUser
        {
            Id = userId,
            Email = "test@test.com",
            FirstName = "Test",
            LastName = "User",
            PhoneNumber = "1234567890"
        };

        var updateDto = new UpdateUserProfileDto
        {
            FirstName = "NewFirstName",
            LastName = "NewLastName",
            PhoneNumber = "9876543210"
        };

        var command = new UpdateUserProfileCommand { UpdateUserProfileDto = updateDto };

        _currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);

        _userManagerMock.Setup(x => x.FindByIdAsync(userId))
            .ReturnsAsync(user);

        _userManagerMock.Setup(x => x.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Update failed due to constraint" }));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("Failed to update profile");
        result.Code.Should().Be(400);
    }

    #endregion
}
