using Application.Core;
using MediatR;

namespace Application.Features.SerhanKitaplar.Commands.EditSerhanKitap;

public class EditSerhanKitapCommand : IRequest<Result<Unit>>
{
    public required string Id { get; set; }
    public required EditSerhanKitapDto EditSerhanKitapDto { get; set; }
}
