using Application.Core;
using MediatR;

namespace Application.Features.ItemClaims.Commands.AdminReviewClaim;

public class AdminReviewClaimCommand : IRequest<Result<Unit>>
{
    public required string Id { get; set; }
    public required AdminReviewClaimDto AdminReviewClaimDto { get; set; }
}
