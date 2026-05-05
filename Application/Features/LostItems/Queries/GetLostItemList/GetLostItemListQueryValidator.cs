using Application.Core.Pagination;
using Application.Features.LostItems.Queries.Common.Enums;
using FluentValidation;

namespace Application.Features.LostItems.Queries.GetLostItemList;

public class GetLostItemListValidator : PagedAndSortedQueryValidator<GetLostItemListQuery, LostItemSortField>
{
    public GetLostItemListValidator()
    {
    }
}
