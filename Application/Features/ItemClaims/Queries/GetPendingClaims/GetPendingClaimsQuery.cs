using Application.Core;
using Application.Core.Pagination;
using Application.Features.ItemClaims.Queries.Common.DTOs;
using Application.Features.ItemClaims.Queries.Common.Enums;
using MediatR;

namespace Application.Features.ItemClaims.Queries.GetPendingClaims;

public record GetPendingClaimsQuery : PagedAndSortedQueryBase<ItemClaimSortField>,
    IRequest<Result<PaginatedListDto<GetItemClaimDto>>>
{
    public string? SearchTerm { get; init; }
}
