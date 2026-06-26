using System.Security.Claims;
using Application.Core;
using Application.Features.Auth.Common.DTOs;
using Application.Interfaces;
using Domain;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommandHandler(
    UserManager<ApplicationUser> userManager,
    IJwtService jwtService,
    IConfiguration configuration) : IRequestHandler<RefreshTokenCommand, Result<UserDto>>
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IJwtService _jwtService = jwtService;
    private readonly IConfiguration _configuration = configuration;

    public async Task<Result<UserDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        // Step 1: Validate the old access token (ignoring expiration)
        var principal = _jwtService.GetPrincipalFromExpiredToken(request.RefreshTokenDto.AccessToken);

        if (principal == null)
        {
            return Result<UserDto>.Failure("Invalid access token.", 401);
        }

        // Step 2: Get user ID from claims
        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim))
        {
            return Result<UserDto>.Failure("Invalid token claims.", 401);
        }

        // Step 3: Get user from database
        var user = await _userManager.FindByIdAsync(userIdClaim);

        if (user == null)
        {
            return Result<UserDto>.Failure("User not found.", 404);
        }

        // Step 4: Validate refresh token against database and expiry
        if (user.RefreshToken != request.RefreshTokenDto.RefreshToken)
        {
            return Result<UserDto>.Failure("Invalid refresh token.", 401);
        }

        if (user.RefreshTokenExpiryTime < DateTime.UtcNow)
        {
            return Result<UserDto>.Failure("Refresh token has expired. Please login again.", 401);
        }

        // Step 5: Generate new tokens
        var roles = await _userManager.GetRolesAsync(user);
        var newAccessToken = _jwtService.GenerateAccessToken(user, roles);
        var newRefreshToken = _jwtService.GenerateRefreshToken();

        // Step 6: Update database with new refresh token
        user.RefreshToken = newRefreshToken;

        var refreshTokenDays = _configuration.GetValue<int>("Jwt:RefreshTokenExpirationDays", 7);
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(refreshTokenDays);

        var updateResult = await _userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
        {
            return Result<UserDto>.Failure("Failed to update user tokens.", 500);
        }

        return Result<UserDto>.Success("Token refreshed successfully", new UserDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email!,
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            Roles = roles.ToList(),
            PhoneNumber = user.PhoneNumber ?? string.Empty
        });
    }
}
