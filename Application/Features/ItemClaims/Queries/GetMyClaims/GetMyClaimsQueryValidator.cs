using Application.Core.Pagination;
using Application.Features.ItemClaims.Queries.Common.Enums;
using FluentValidation;

namespace Application.Features.ItemClaims.Queries.GetMyClaims;

public class GetMyClaimsQueryValidator : PagedAndSortedQueryValidator<GetMyClaimsQuery, ItemClaimSortField>
{
}
