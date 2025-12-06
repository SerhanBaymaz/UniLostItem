using Application.Core;
using MediatR;

namespace Application.Features.SerhanKitaplar.Commands.DeleteSerhanKitap;

public class DeleteSerhanKitapCommand : IRequest<Result<Unit>>
{
    public required string Id { get; set; }
}
