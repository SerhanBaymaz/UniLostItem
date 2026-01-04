using API.Responses;
using Application.Features.Auth.Common.DTOs;
using Application.Features.Auth.Commands.Login;
using Application.Features.Auth.Commands.RefreshToken;
using Application.Features.Auth.Commands.Register;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/v1/auth")]
public class AuthController : BaseApiController
{
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<StandardApiResponse<UserDto>>> Register([FromBody] RegisterDto registerDto)
    {
        var command = new RegisterCommand { RegisterDto = registerDto };
        return HandleResult(await Mediator.Send(command));
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<StandardApiResponse<UserDto>>> Login([FromBody] LoginDto loginDto)
    {
        var command = new LoginCommand { LoginDto = loginDto };
        return HandleResult(await Mediator.Send(command));
    }

    [AllowAnonymous]
    [HttpPost("refresh-token")]
    public async Task<ActionResult<StandardApiResponse<UserDto>>> RefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
    {
        var command = new RefreshTokenCommand { RefreshTokenDto = refreshTokenDto };
        return HandleResult(await Mediator.Send(command));
    }
}
