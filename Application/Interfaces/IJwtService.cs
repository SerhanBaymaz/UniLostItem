using System.Security.Claims;
using Domain;

namespace Application.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(ApplicationUser user, IList<string> roles);
    string GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string? token);
}
