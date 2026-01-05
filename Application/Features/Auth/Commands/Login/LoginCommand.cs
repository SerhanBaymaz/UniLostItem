using Application.Core;
using Application.Features.Auth.Common.DTOs;
using MediatR;

namespace Application.Features.Auth.Commands.Login;

public class LoginCommand : IRequest<Result<UserDto>>
{
    public required LoginDto LoginDto { get; set; }
}