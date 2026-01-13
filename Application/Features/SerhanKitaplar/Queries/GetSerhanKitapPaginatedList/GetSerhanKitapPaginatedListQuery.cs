using Application.Core;
using Application.Core.Pagination;
using Application.Features.SerhanKitaplar.Queries.Common.DTOs;
using MediatR;

namespace Application.Features.SerhanKitaplar.Queries.GetSerhanKitapPaginatedList;

public record GetSerhanKitapPaginatedListQuery : PagedAndSortedQueryBase,
    IRequest<Result<PaginatedListDto<GetSerhanKitapDto>>>
{
    // PageNumber, PageSize, SortBy, SortDescending are inherited from PagedAndSortedQueryBase
}
