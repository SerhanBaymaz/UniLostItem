using Application.Core;
using Application.Features.LostItems.Queries.Common.DTOs;
using MediatR;

namespace Application.Features.LostItems.Queries.GetLostItemDetails;

public class GetLostItemDetailsQuery : IRequest<Result<GetLostItemDetailDto>>
{
    public required string Id { get; set; }
}
