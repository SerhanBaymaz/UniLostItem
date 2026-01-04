using Application.Core;
using Application.Features.Auth.Queries.GetCurrentUser;
using Application.Interfaces;
using Domain;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;

namespace Tests.Application_Tests.Features.Auth.Queries.GetCurrentUser;

public class GetCurrentUserQueryTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly GetCurrentUserQueryHandler _handler;

    public GetCurrentUserQueryTests()
    {
        _userManagerMock = MockUserManager();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _handler = new GetCurrentUserQueryHandler(_userManagerMock.Object, _currentUserServiceMock.Object);
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
    public async Task Handle_ShouldReturnCurrentUserDto_WhenUserIsAuthenticated()
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
        var roles = new List<string> { "User" };

        _currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);

        _userManagerMock.Setup(x => x.FindByIdAsync(userId))
            .ReturnsAsync(user);

        _userManagerMock.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(roles);

        var query = new GetCurrentUserQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(userId);
        result.Value.Email.Should().Be("test@test.com");
        result.Value.FirstName.Should().Be("Test");
        result.Value.LastName.Should().Be("User");
        result.Value.PhoneNumber.Should().Be("1234567890");
        result.Value.Roles.Should().BeEquivalentTo(roles);
    }

    [Fact]
    public async Task Handle_ShouldIncludeRoles_WhenUserHasMultipleRoles()
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
        var roles = new List<string> { "Admin", "Moderator", "User" };

        _currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);

        _userManagerMock.Setup(x => x.FindByIdAsync(userId))
            .ReturnsAsync(user);

        _userManagerMock.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(roles);

        var query = new GetCurrentUserQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Roles.Should().HaveCount(3);
        result.Value.Roles.Should().Contain("Admin");
        result.Value.Roles.Should().Contain("Moderator");
        result.Value.Roles.Should().Contain("User");
    }

    #endregion

    #region Authentication Failure Tests

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserIsNotAuthenticated()
    {
        // Arrange
        _currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(false);
        _currentUserServiceMock.Setup(x => x.UserId).Returns((string?)null);

        var query = new GetCurrentUserQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

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

        var query = new GetCurrentUserQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("User is not authenticated.");
        result.Code.Should().Be(401);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserIdIsEmpty()
    {
        // Arrange
        _currentUserServiceMock.Setup(x => x.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(x => x.UserId).Returns(string.Empty);

        var query = new GetCurrentUserQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

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

        var query = new GetCurrentUserQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("User not found.");
        result.Code.Should().Be(404);
    }

    #endregion
}
