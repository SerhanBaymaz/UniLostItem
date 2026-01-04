using Application.Core;
using Application.Features.Auth.Queries.GetCurrentUser;
using Application.Interfaces;
using Domain;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Auth.Commands.UpdateUserProfile;

public class UpdateUserProfileCommandHandler : IRequestHandler<UpdateUserProfileCommand, Result<CurrentUserDto>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ICurrentUserService _currentUserService;

    public UpdateUserProfileCommandHandler(UserManager<ApplicationUser> userManager, ICurrentUserService currentUserService)
    {
        _userManager = userManager;
        _currentUserService = currentUserService;
    }

    public async Task<Result<CurrentUserDto>> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated || string.IsNullOrEmpty(_currentUserService.UserId))
        {
            return Result<CurrentUserDto>.Failure("User is not authenticated.", 401);
        }

        var user = await _userManager.FindByIdAsync(_currentUserService.UserId);

        if (user == null)
        {
            return Result<CurrentUserDto>.Failure("User not found.", 404);
        }

        user.FirstName = request.UpdateUserProfileDto.FirstName;
        user.LastName = request.UpdateUserProfileDto.LastName;
        user.PhoneNumber = request.UpdateUserProfileDto.PhoneNumber;

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return Result<CurrentUserDto>.Failure($"Failed to update profile: {errors}", 400);
        }

        var roles = await _userManager.GetRolesAsync(user);

        return Result<CurrentUserDto>.Success("Profile updated successfully", new CurrentUserDto
        {
            Id = user.Id,
            Email = user.Email!,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber ?? string.Empty,
            Roles = roles.ToList()
        });
    }
}