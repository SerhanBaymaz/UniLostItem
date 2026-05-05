using Application.Core;
using Application.Core.Pagination;
using Application.Features.ItemClaims.Queries.Common.DTOs;
using Application.Features.ItemClaims.Queries.Common.Enums;
using Domain.Common.Enums;
using MediatR;

namespace Application.Features.ItemClaims.Queries.GetMyClaims;

public record GetMyClaimsQuery : PagedAndSortedQueryBase<ItemClaimSortField>,
    IRequest<Result<PaginatedListDto<GetItemClaimDto>>>
{
    public ClaimStatus? Status { get; init; }
}
