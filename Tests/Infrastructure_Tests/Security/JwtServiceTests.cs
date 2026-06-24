using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Domain;
using FluentAssertions;
using Infrastructure.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Tests.Infrastructure_Tests.Security;

public class JwtServiceTests
{
    private readonly Mock<IConfiguration> _configMock;
    private readonly JwtService _jwtService;
    private readonly string _secretKey;

    public JwtServiceTests()
    {
        _configMock = new Mock<IConfiguration>();

        _secretKey = "SuperSecretKeyMustBeLongEnoughForHmacSha512SecurityAlgorithm_AtLeast64BytesLong_BlaBlaBla123";
        _configMock.Setup(x => x["Jwt:SecretKey"]).Returns(_secretKey);
        _configMock.Setup(x => x["Jwt:Issuer"]).Returns("TestIssuer");
        _configMock.Setup(x => x["Jwt:Audience"]).Returns("TestAudience");

        var loggerMock = new Mock<ILogger<JwtService>>();
        _jwtService = new JwtService(_configMock.Object, loggerMock.Object);
    }

    [Fact]
    public void GenerateAccessToken_ShouldReturnValidJwtString()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = "testuser",
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User"
        };
        var roles = new List<string> { "Admin", "User" };

        // Act
        var token = _jwtService.GenerateAccessToken(user, roles);

        // Assert
        token.Should().NotBeNullOrEmpty();

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        // Standart Claim Kontrolleri
        // Jwt kütüphanesi versiyonuna göre claim isimleri değişebiliyor (nameid, email, unique_name vs.)
        jwtToken.Claims.Should().Contain(c => c.Type == "nameid" || c.Type == ClaimTypes.NameIdentifier);
        jwtToken.Claims.Should().Contain(c => c.Type == "email" || c.Type == ClaimTypes.Email);
        jwtToken.Claims.Should().Contain(c => c.Type == "unique_name" || c.Type == ClaimTypes.Name);

        // Custom Claim Kontrolleri
        jwtToken.Claims.Select(c => c.Type).Should().Contain("FirstName");
        jwtToken.Claims.First(c => c.Type == "FirstName").Value.Should().Be(user.FirstName);

        // Rolleri kontrol et
        var roleClaims = jwtToken.Claims.Where(c => c.Type == "role" || c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();
        roleClaims.Should().Contain("Admin");
        roleClaims.Should().Contain("User");
    }

    [Fact]
    public void GenerateRefreshToken_ShouldReturnBase64String()
    {
        var refreshToken = _jwtService.GenerateRefreshToken();
        refreshToken.Should().NotBeNullOrEmpty();
        var bytes = Convert.FromBase64String(refreshToken);
        bytes.Should().NotBeEmpty();
        bytes.Length.Should().Be(32);
    }

    [Fact]
    public void GetPrincipalFromExpiredToken_ShouldReturnClaims_EvenIfTokenExpired()
    {
        var user = new ApplicationUser { Id = "123", UserName = "expiredUser", Email = "ex@test.com" };
        var token = _jwtService.GenerateAccessToken(user, new List<string>());

        var principal = _jwtService.GetPrincipalFromExpiredToken(token);

        principal.Should().NotBeNull();
        // ClaimTypes.Name -> unique_name olarak dönebilir
        principal!.Identities.First().Claims.Any(c => c.Value == "expiredUser").Should().BeTrue();
    }

    [Fact]
    public void GetPrincipalFromExpiredToken_ShouldReturnNull_ForNullToken()
    {
        // Act
        var principal = _jwtService.GetPrincipalFromExpiredToken(null);

        // Assert
        principal.Should().BeNull();
    }

    [Fact]
    public void GetPrincipalFromExpiredToken_ShouldReturnNull_ForEmptyToken()
    {
        // Act
        var principal = _jwtService.GetPrincipalFromExpiredToken(string.Empty);

        // Assert
        principal.Should().BeNull();
    }

    [Fact]
    public void GetPrincipalFromExpiredToken_ShouldReturnNull_ForMalformedToken()
    {
        // Arrange — dotsuz, geçersiz format
        var invalidToken = "malformedtoken";

        // Act
        var principal = _jwtService.GetPrincipalFromExpiredToken(invalidToken);

        // Assert — ArgumentException catch'ine düşer
        principal.Should().BeNull();
    }

    [Fact]
    public void GetPrincipalFromExpiredToken_ShouldReturnNull_ForInvalidJwtFormat()
    {
        // Arrange — 3 parçalı ama geçersiz Base64 içerik
        var invalidToken = "bu.gecersiz.bir.token";

        // Act
        var principal = _jwtService.GetPrincipalFromExpiredToken(invalidToken);

        // Assert
        principal.Should().BeNull();
    }

    [Fact]
    public void GetPrincipalFromExpiredToken_ShouldReturnNull_ForTamperedToken()
    {
        // Arrange — farklı key ile imzalanmış (manipüle edilmiş) token
        var tamperedKey = "ThisIsADifferentSecretKeyThatIsNotTheOriginalOne_AtLeast64Bytes!";
        var tamperedConfigMock = new Mock<IConfiguration>();
        tamperedConfigMock.Setup(x => x["Jwt:SecretKey"]).Returns(tamperedKey);
        var tamperedLoggerMock = new Mock<ILogger<JwtService>>();
        var tamperedJwtService = new JwtService(tamperedConfigMock.Object, tamperedLoggerMock.Object);

        var user = new ApplicationUser { Id = "123", UserName = "test", Email = "test@test.com" };
        var token = tamperedJwtService.GenerateAccessToken(user, new List<string>());

        // Act — orijinal secretKey ile doğrulamaya çalış
        var principal = _jwtService.GetPrincipalFromExpiredToken(token);

        // Assert — SecurityTokenException catch'ine düşer (imza uyuşmazlığı)
        principal.Should().BeNull();
    }

    [Fact]
    public void GetPrincipalFromExpiredToken_ShouldReturnNull_ForTokenWithInvalidBase64Content()
    {
        // Arrange — 3 parçalı JWT formatında ama Base64 geçersiz karakterler içeriyor
        // FormatException → catch-all Exception catch'ine düşer
        var invalidToken = "!!!.!!!.!!!";

        // Act
        var principal = _jwtService.GetPrincipalFromExpiredToken(invalidToken);

        // Assert
        principal.Should().BeNull();
    }
}
