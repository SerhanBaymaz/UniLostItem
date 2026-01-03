using Application.Core;
using Application.Features.Auth.DTOs;
using MediatR;

namespace Application.Features.Auth.LoginUser;

public record LoginUserCommand(string Email, string Password) : IRequest<Result<UserDto>>;
