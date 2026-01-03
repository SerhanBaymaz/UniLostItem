using Application.Core;
using Application.Features.Auth.Common.DTOs;
using MediatR;

namespace Application.Features.Auth.Commands.Register;

public class RegisterCommand : IRequest<Result<UserDto>>
{
    public required RegisterDto RegisterDto { get; set; }
}