using Application.Interfaces;
using FluentAssertions;
using Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Security.Claims;

namespace Tests.Application_Tests.Services;

public class CurrentUserServiceTests
{
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly CurrentUserService _currentUserService;

    public CurrentUserServiceTests()
    {
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _currentUserService = new CurrentUserService(_httpContextAccessorMock.Object);
    }

    #region IsAuthenticated Tests

    [Fact]
    public void IsAuthenticated_ShouldReturnTrue_WhenUserIsAuthenticated()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "user123"),
            new Claim(ClaimTypes.Email, "test@test.com")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext();
        httpContext.User = principal;

        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = _currentUserService.IsAuthenticated;

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsAuthenticated_ShouldReturnFalse_WhenUserIsNotAuthenticated()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(); // No identity

        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = _currentUserService.IsAuthenticated;

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsAuthenticated_ShouldReturnFalse_WhenHttpContextIsNull()
    {
        // Arrange
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext?)null);

        // Act
        var result = _currentUserService.IsAuthenticated;

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region UserId Tests

    [Fact]
    public void UserId_ShouldReturnCorrectUserId_WhenUserIsAuthenticated()
    {
        // Arrange
        var expectedUserId = "user123";
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, expectedUserId),
            new Claim(ClaimTypes.Email, "test@test.com")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext();
        httpContext.User = principal;

        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = _currentUserService.UserId;

        // Assert
        result.Should().Be(expectedUserId);
    }

    [Fact]
    public void UserId_ShouldReturnNull_WhenNameIdentifierClaimIsMissing()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Email, "test@test.com")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext();
        httpContext.User = principal;

        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = _currentUserService.UserId;

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void UserId_ShouldReturnNull_WhenHttpContextIsNull()
    {
        // Arrange
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext?)null);

        // Act
        var result = _currentUserService.UserId;

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region UserId - Additional Edge Cases

    [Fact]
    public void UserId_ShouldReturnNull_WhenUserIsNull()
    {
        // Arrange
        var httpContext = new DefaultHttpContext
        {
            User = null!
        };

        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = _currentUserService.UserId;

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void UserId_ShouldReturnNull_WhenIdentityIsNull()
    {
        // Arrange
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal() // No identities
        };

        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = _currentUserService.UserId;

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void UserId_ShouldReturnFirstClaim_WhenMultipleNameIdentifierClaimsExist()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "first-user-id"),
            new Claim(ClaimTypes.NameIdentifier, "second-user-id"),
            new Claim(ClaimTypes.Email, "test@test.com")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext
        {
            User = principal
        };

        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = _currentUserService.UserId;

        // Assert
        result.Should().Be("first-user-id");
    }

    [Fact]
    public void UserId_ShouldReturnEmptyString_WhenClaimValueIsEmpty()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, string.Empty),
            new Claim(ClaimTypes.Email, "test@test.com")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext
        {
            User = principal
        };

        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = _currentUserService.UserId;

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region Email - Additional Edge Cases

    [Fact]
    public void Email_ShouldReturnNull_WhenUserIsNull()
    {
        // Arrange
        var httpContext = new DefaultHttpContext
        {
            User = null!
        };

        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = _currentUserService.Email;

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Email_ShouldReturnNull_WhenIdentityIsNull()
    {
        // Arrange
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal() // No identities
        };

        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = _currentUserService.Email;

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Email_ShouldReturnFirstClaim_WhenMultipleEmailClaimsExist()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "user123"),
            new Claim(ClaimTypes.Email, "first@test.com"),
            new Claim(ClaimTypes.Email, "second@test.com")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext
        {
            User = principal
        };

        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = _currentUserService.Email;

        // Assert
        result.Should().Be("first@test.com");
    }

    [Fact]
    public void Email_ShouldReturnEmptyString_WhenClaimValueIsEmpty()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "user123"),
            new Claim(ClaimTypes.Email, string.Empty)
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext
        {
            User = principal
        };

        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = _currentUserService.Email;

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region IsAuthenticated - Additional Edge Cases

    [Fact]
    public void IsAuthenticated_ShouldReturnFalse_WhenUserIsNull()
    {
        // Arrange
        var httpContext = new DefaultHttpContext
        {
            User = null!
        };

        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = _currentUserService.IsAuthenticated;

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsAuthenticated_ShouldReturnFalse_WhenIdentityIsNull()
    {
        // Arrange
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal() // No identities
        };

        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = _currentUserService.IsAuthenticated;

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsAuthenticated_ShouldReturnFalse_WhenIdentityIsNotAuthenticated()
    {
        // Arrange
        var identity = new ClaimsIdentity(); // No authentication type
        var principal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext
        {
            User = principal
        };

        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = _currentUserService.IsAuthenticated;

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Email Tests

    [Fact]
    public void Email_ShouldReturnCorrectEmail_WhenUserIsAuthenticated()
    {
        // Arrange
        var expectedEmail = "test@test.com";
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "user123"),
            new Claim(ClaimTypes.Email, expectedEmail)
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext();
        httpContext.User = principal;

        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = _currentUserService.Email;

        // Assert
        result.Should().Be(expectedEmail);
    }

    [Fact]
    public void Email_ShouldReturnNull_WhenEmailClaimIsMissing()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "user123")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext();
        httpContext.User = principal;

        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = _currentUserService.Email;

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Email_ShouldReturnNull_WhenHttpContextIsNull()
    {
        // Arrange
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext?)null);

        // Act
        var result = _currentUserService.Email;

        // Assert
        result.Should().BeNull();
    }

    #endregion
}