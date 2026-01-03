using API.Responses;
using Application.Features.Auth.DTOs;
using Application.Features.Auth.RegisterUser;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/v1/auth")]
public class AuthController : BaseApiController
{
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<StandardApiResponse<UserDto>>> Register([FromBody] RegisterUserCommand command)
    {
        return HandleResult(await Mediator.Send(command));
    }
}
