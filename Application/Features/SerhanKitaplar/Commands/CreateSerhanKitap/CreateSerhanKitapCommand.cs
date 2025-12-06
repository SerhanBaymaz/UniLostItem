using Application.Core;
using MediatR;

namespace Application.Features.SerhanKitaplar.Commands.CreateSerhanKitap;

public class CreateSerhanKitapCommand : IRequest<Result<string>>
{
    public required CreateSerhanKitapDto CreateSerhanKitapDto { get; set; }
}
