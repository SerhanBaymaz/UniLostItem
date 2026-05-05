using Application.Core.Pagination;
using Application.Features.ItemClaims.Queries.Common.Enums;
using FluentValidation;

namespace Application.Features.ItemClaims.Queries.GetPendingClaims;

public class GetPendingClaimsQueryValidator : PagedAndSortedQueryValidator<GetPendingClaimsQuery, ItemClaimSortField>
{
}
