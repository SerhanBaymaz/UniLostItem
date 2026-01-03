using Application.Core;
using Application.Features.Auth.DTOs;
using MediatR;

namespace Application.Features.Auth.RegisterUser;

public class RegisterUserCommand : IRequest<Result<UserDto>>
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
}
