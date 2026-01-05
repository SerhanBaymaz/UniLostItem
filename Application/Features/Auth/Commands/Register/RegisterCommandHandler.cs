using Application.Core;
using Application.Features.Auth.Common.DTOs;
using Application.Interfaces;
using Domain;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Auth.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<UserDto>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtService _jwtService;

    public RegisterCommandHandler(UserManager<ApplicationUser> userManager, IJwtService jwtService)
    {
        _userManager = userManager;
        _jwtService = jwtService;
    }

    public async Task<Result<UserDto>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        // 1. Email kontrolü
        if (await _userManager.FindByEmailAsync(request.RegisterDto.Email) != null)
        {
            return Result<UserDto>.Failure("Email is already taken", 400);
        }

        // 2. Kullanıcı oluşturma
        var user = new ApplicationUser
        {
            Email = request.RegisterDto.Email,
            UserName = request.RegisterDto.Email, // Email'i UserName olarak kullanıyoruz
            FirstName = request.RegisterDto.FirstName,
            LastName = request.RegisterDto.LastName,
            PhoneNumber = request.RegisterDto.PhoneNumber,
            CreatedDate = DateTime.UtcNow,
            IsActive = true
        };

        var result = await _userManager.CreateAsync(user, request.RegisterDto.Password);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return Result<UserDto>.Failure($"Registration failed: {errors}", 400);
        }

        // 3. Varsayılan rol atama
        var roleResult = await _userManager.AddToRoleAsync(user, "BaseUser");
        if (!roleResult.Succeeded)
        {
            return Result<UserDto>.Failure("Failed to assign role", 500);
        }

        // 4. Token üretme
        var roles = await _userManager.GetRolesAsync(user);

        var accessToken = _jwtService.GenerateAccessToken(user, roles);
        var refreshToken = _jwtService.GenerateRefreshToken();

        // Refresh token'ı veritabanına kaydet
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _userManager.UpdateAsync(user);

        // 5. DTO Dönüş
        var userDto = new UserDto
        {
            Id = user.Id,
            Email = user.Email!,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            Roles = roles.ToList()
        };

        return Result<UserDto>.Success("User registered successfully", userDto);
    }
}
