using Application.Core;
using MediatR;

namespace Application.Features.LostItems.Commands.CreateLostItem;

public class CreateLostItemCommand : IRequest<Result<string>>
{
    public required CreateLostItemDto CreateLostItemDto { get; set; }
}
