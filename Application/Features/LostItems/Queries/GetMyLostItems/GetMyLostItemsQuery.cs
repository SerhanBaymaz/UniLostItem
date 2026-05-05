using Application.Core;
using Application.Core.Pagination;
using Application.Features.LostItems.Queries.Common.DTOs;
using Application.Features.LostItems.Queries.Common.Enums;
using Domain.Common.Enums;
using MediatR;

namespace Application.Features.LostItems.Queries.GetMyLostItems;

public record GetMyLostItemsQuery : PagedAndSortedQueryBase<LostItemSortField>,
    IRequest<Result<PaginatedListDto<GetLostItemDto>>>
{
    public string? SearchTerm { get; init; }
    public ItemType? ItemType { get; init; }
    public ItemCategory? Category { get; init; }
    public ItemStatus? Status { get; init; }
}
