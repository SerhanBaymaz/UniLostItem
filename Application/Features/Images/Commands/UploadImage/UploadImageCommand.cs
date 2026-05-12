using Application.Interfaces;
using MediatR;

namespace Application.Features.Images.Commands.UploadImage;

public class UploadImageCommand : IRequest<ImageUploadResult>
{
    public required Stream ImageStream { get; set; }
    public required string FileName { get; set; }
}
