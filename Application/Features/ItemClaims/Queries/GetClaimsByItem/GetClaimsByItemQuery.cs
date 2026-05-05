using Application.Core;
using Application.Core.Pagination;
using Application.Features.ItemClaims.Queries.Common.DTOs;
using Application.Features.ItemClaims.Queries.Common.Enums;
using Domain.Common.Enums;
using MediatR;

namespace Application.Features.ItemClaims.Queries.GetClaimsByItem;

public record GetClaimsByItemQuery : PagedAndSortedQueryBase<ItemClaimSortField>,
    IRequest<Result<PaginatedListDto<GetItemClaimDto>>>
{
    public required string LostItemId { get; init; }
    public ClaimStatus? Status { get; init; }
}
