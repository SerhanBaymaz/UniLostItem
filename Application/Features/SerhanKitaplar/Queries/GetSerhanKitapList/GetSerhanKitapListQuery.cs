using Application.Core;
using Application.Core.Pagination;
using Application.Features.SerhanKitaplar.Queries.Common.DTOs;
using Application.Features.SerhanKitaplar.Queries.Common.Enums;
using Domain;
using MediatR;

namespace Application.Features.SerhanKitaplar.Queries.GetSerhanKitapList;

public record GetSerhanKitapListQuery : PagedAndSortedQueryBase<KitapSortField>,
    IRequest<Result<PaginatedListDto<GetSerhanKitapDto>>>
{
    // PageNumber, PageSize, SortBy, SortDescending are inherited from PagedAndSortedQueryBase<KitapSortField>

    // Filter properties
    public string? SearchTerm { get; init; }
    public string? KitapName { get; init; }
    public string? KitapYazar { get; init; }
    public int? MinPageCount { get; init; }
    public int? MaxPageCount { get; init; }
}
