using Application.Core.Pagination;
using Application.Features.LostItems.Queries.Common.Enums;
using FluentValidation;

namespace Application.Features.LostItems.Queries.GetMyLostItems;

public class GetMyLostItemsValidator : PagedAndSortedQueryValidator<GetMyLostItemsQuery, LostItemSortField>
{
    public GetMyLostItemsValidator()
    {
    }
}
