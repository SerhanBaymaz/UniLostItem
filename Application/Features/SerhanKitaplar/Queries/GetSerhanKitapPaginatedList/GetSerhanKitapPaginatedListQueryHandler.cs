using Application.Core;
using Application.Core.Pagination;
using Application.Features.SerhanKitaplar.Queries.Common.DTOs;
using AutoMapper;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Features.SerhanKitaplar.Queries.GetSerhanKitapPaginatedList;

public class GetSerhanKitapPaginatedListQueryHandler :
    IRequestHandler<GetSerhanKitapPaginatedListQuery, Result<PaginatedListDto<GetSerhanKitapDto>>>
{
    private readonly IAppDbContext _context;
    private readonly IMapper _mapper;

    public GetSerhanKitapPaginatedListQueryHandler(IAppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<PaginatedListDto<GetSerhanKitapDto>>> Handle(
        GetSerhanKitapPaginatedListQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.SerhanKitaplar.AsQueryable();

        // Sorting (with whitelist for security)
        query = ApplySorting(query, request);

        // Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Pagination
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = _mapper.Map<List<GetSerhanKitapDto>>(items);

        var paginatedList = new PaginatedListDto<GetSerhanKitapDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };

        return Result<PaginatedListDto<GetSerhanKitapDto>>.Success("Kitaplar başarıyla getirildi", paginatedList);
    }

    private static IQueryable<SerhanKitap> ApplySorting(IQueryable<SerhanKitap> query, GetSerhanKitapPaginatedListQuery request)
    {
        // WHITELIST approach - prevents SQL injection
        return request.SortBy?.ToLowerInvariant() switch
        {
            "kitapname" or "name" or "ad" => request.SortDescending
                ? query.OrderByDescending(x => x.KitapName)
                : query.OrderBy(x => x.KitapName),
            "kitapyazar" or "yazar" or "author" => request.SortDescending
                ? query.OrderByDescending(x => x.KitapYazar)
                : query.OrderBy(x => x.KitapYazar),
            "kitapsayfasayisi" or "sayfasayisi" or "pagecount" => request.SortDescending
                ? query.OrderByDescending(x => x.KitapSayfaSayisi)
                : query.OrderBy(x => x.KitapSayfaSayisi),
            _ => query.OrderBy(x => x.KitapName) // default sort
        };
    }
}
