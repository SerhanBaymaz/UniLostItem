using API.Responses;
using Application.Core;
using Application.Features.Images.Commands.DeleteImage;
using Application.Features.Images.Commands.UploadImage;
using Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/v1/images")]
[ApiController]
public class ImagesController : BaseApiController
{
    [HttpPost("upload")]
    [Authorize]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<StandardApiResponse<ImageUploadResult>>> UploadImage(IFormFile image)
    {
        if (image == null || image.Length == 0)
        {
            return BadRequest(StandardApiResponse<ImageUploadResult>.ErrorResponse("Görsel dosyası gereklidir.", StatusCodes.Status400BadRequest));
        }

        var command = new UploadImageCommand
        {
            ImageStream = image.OpenReadStream(),
            FileName = image.FileName
        };

        var result = await Mediator.Send(command);

        return Ok(StandardApiResponse<ImageUploadResult>.SuccessResponse(result, "Görsel başarıyla yüklendi", StatusCodes.Status200OK));
    }

    [HttpDelete("{*publicId}")]
    [Authorize]
    public async Task<ActionResult<StandardApiResponse<Unit>>> DeleteImage(string publicId)
    {
        var command = new DeleteImageCommand { PublicId = publicId };
        return HandleResult(await Mediator.Send(command));
    }
}
