using Application.Core;
using MediatR;

namespace Application.Features.ItemClaims.Commands.CancelItemClaim;

public class CancelItemClaimCommand : IRequest<Result<Unit>>
{
    public required string Id { get; set; }
}
