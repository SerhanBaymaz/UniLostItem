using Application.Core;
using Application.Interfaces;
using MediatR;

namespace Application.Features.Images.Commands.DeleteImage;

public class DeleteImageCommandHandler : IRequestHandler<DeleteImageCommand, Result<Unit>>
{
    private readonly IImageStorageService _imageStorageService;

    public DeleteImageCommandHandler(IImageStorageService imageStorageService)
    {
        _imageStorageService = imageStorageService;
    }

    public async Task<Result<Unit>> Handle(DeleteImageCommand request, CancellationToken cancellationToken)
    {
        var success = await _imageStorageService.DeleteImageAsync(request.PublicId, cancellationToken);

        if (!success)
        {
            return Result<Unit>.Failure("Görsel silinemedi veya bulunamadı", 400);
        }

        return Result<Unit>.Success("Görsel başarıyla silindi", Unit.Value);
    }
}
