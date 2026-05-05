using Application.Core;
using MediatR;

namespace Application.Features.ItemClaims.Commands.CreateItemClaim;

public class CreateItemClaimCommand : IRequest<Result<string>>
{
    public required CreateItemClaimDto CreateItemClaimDto { get; set; }
}
