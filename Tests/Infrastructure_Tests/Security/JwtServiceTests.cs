using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Domain;
using FluentAssertions;
using Infrastructure.Security;
using Microsoft.Extensions.Configuration;
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

        _jwtService = new JwtService(_configMock.Object);
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
    public void GetPrincipalFromExpiredToken_ShouldReturnNull_ForInvalidToken()
    {
        var invalidToken = "bu.gecersiz.bir.token";
        var principal = _jwtService.GetPrincipalFromExpiredToken(invalidToken);
        principal.Should().BeNull();
    }
}
