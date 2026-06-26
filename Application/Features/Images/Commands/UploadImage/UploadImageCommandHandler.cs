using Application.Interfaces;
using MediatR;

namespace Application.Features.Images.Commands.UploadImage;

public class UploadImageCommandHandler(IImageStorageService imageStorageService) : IRequestHandler<UploadImageCommand, ImageUploadResult>
{
    private readonly IImageStorageService _imageStorageService = imageStorageService;

    public async Task<ImageUploadResult> Handle(UploadImageCommand request, CancellationToken cancellationToken)
    {
        return await _imageStorageService.UploadImageAsync(request.ImageStream, request.FileName, cancellationToken);
    }
}
