using Application.Core;
using MediatR;

namespace Application.Features.Images.Commands.DeleteImage;

public class DeleteImageCommand : IRequest<Result<Unit>>
{
    public required string PublicId { get; set; }
}
