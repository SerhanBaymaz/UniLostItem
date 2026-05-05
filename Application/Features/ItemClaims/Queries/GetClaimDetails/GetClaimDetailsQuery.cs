using Application.Core;
using Application.Features.ItemClaims.Queries.Common.DTOs;
using MediatR;

namespace Application.Features.ItemClaims.Queries.GetClaimDetails;

public record GetClaimDetailsQuery : IRequest<Result<GetItemClaimDto>>
{
    public required string Id { get; set; }
}
