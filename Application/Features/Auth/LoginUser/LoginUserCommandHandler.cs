using Application.Core;
using Application.Features.Auth.DTOs;
using Application.Interfaces;
using Domain;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Application.Features.Auth.LoginUser;

public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, Result<UserDto>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtService _jwtService;
    private readonly IConfiguration _configuration;

    public LoginUserCommandHandler(UserManager<ApplicationUser> userManager, IJwtService jwtService, IConfiguration configuration)
    {
        _userManager = userManager;
        _jwtService = jwtService;
        _configuration = configuration;
    }

    public async Task<Result<UserDto>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user == null)
        {
            return Result<UserDto>.Failure("Invalid email or password.", 401);
        }

        var result = await _userManager.CheckPasswordAsync(user, request.Password);

        if (!result)
        {
            return Result<UserDto>.Failure("Invalid email or password.", 401);
        }

        // Get Roles
        var roles = await _userManager.GetRolesAsync(user);

        // Generate Tokens
        var accessToken = _jwtService.GenerateAccessToken(user, roles);
        var refreshToken = _jwtService.GenerateRefreshToken();

        // Save Refresh Token to DB
        user.RefreshToken = refreshToken;
        
        var refreshTokenDays = _configuration.GetValue<int>("Jwt:RefreshTokenExpirationDays", 7);
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(refreshTokenDays);
        user.LastLoginDate = DateTime.UtcNow;

        var updateResult = await _userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
        {
            return Result<UserDto>.Failure("Failed to update user tokens.", 500);
        }

        return Result<UserDto>.Success("Login successful", new UserDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email!,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            Roles = roles.ToList(),
            PhoneNumber = user.PhoneNumber ?? string.Empty
        });
    }
}
