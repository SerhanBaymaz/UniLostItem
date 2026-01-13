using Application.Core;
using Application.Core.Pagination;
using Application.Features.SerhanKitaplar.Queries.Common.DTOs;
using MediatR;

namespace Application.Features.SerhanKitaplar.Queries.GetSerhanKitapPaginatedList;

public record GetSerhanKitapPaginatedListQuery : PagedAndSortedQueryBase,
    IRequest<Result<PaginatedListDto<GetSerhanKitapDto>>>
{
    // PageNumber, PageSize, SortBy, SortDescending are inherited from PagedAndSortedQueryBase

    // Filter properties
    public string? SearchTerm { get; init; }
    public string? KitapName { get; init; }
    public string? KitapYazar { get; init; }
    public int? MinPageCount { get; init; }
    public int? MaxPageCount { get; init; }
}
