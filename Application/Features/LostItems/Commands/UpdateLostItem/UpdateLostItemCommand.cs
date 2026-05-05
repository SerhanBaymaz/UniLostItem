using Application.Core;
using MediatR;

namespace Application.Features.LostItems.Commands.UpdateLostItem;

public class UpdateLostItemCommand : IRequest<Result<Unit>>
{
    public required string Id { get; set; }
    public required UpdateLostItemDto UpdateLostItemDto { get; set; }
}
