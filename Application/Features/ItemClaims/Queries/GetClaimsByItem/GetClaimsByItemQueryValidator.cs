using Application.Core.Pagination;
using Application.Features.ItemClaims.Queries.Common.Enums;
using FluentValidation;

namespace Application.Features.ItemClaims.Queries.GetClaimsByItem;

public class GetClaimsByItemQueryValidator : PagedAndSortedQueryValidator<GetClaimsByItemQuery, ItemClaimSortField>
{
    public GetClaimsByItemQueryValidator()
    {
        RuleFor(x => x.LostItemId)
            .NotEmpty().WithMessage("İlan ID gereklidir");
    }
}
