using Application.Core;
using MediatR;

namespace Application.Features.ItemClaims.Commands.RespondToClaim;

public class RespondToClaimCommand : IRequest<Result<Unit>>
{
    public required string Id { get; set; }
    public required RespondToClaimDto RespondToClaimDto { get; set; }
}
