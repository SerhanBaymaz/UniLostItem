using Application.Core;
using Application.Features.Auth.Queries.GetCurrentUser;
using MediatR;

namespace Application.Features.Auth.Commands.UpdateUserProfile;

public class UpdateUserProfileCommand : IRequest<Result<CurrentUserDto>>
{
    public UpdateUserProfileDto UpdateUserProfileDto { get; set; } = new();
}
