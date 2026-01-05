using Application.Core;
using Application.Features.Auth.Common.DTOs;
using MediatR;

namespace Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommand : IRequest<Result<UserDto>>
{
    public required RefreshTokenDto RefreshTokenDto { get; set; }
}
