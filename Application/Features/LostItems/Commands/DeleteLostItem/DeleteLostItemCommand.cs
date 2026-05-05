using Application.Core;
using MediatR;

namespace Application.Features.LostItems.Commands.DeleteLostItem;

public class DeleteLostItemCommand : IRequest<Result<Unit>>
{
    public required string Id { get; set; }
}
