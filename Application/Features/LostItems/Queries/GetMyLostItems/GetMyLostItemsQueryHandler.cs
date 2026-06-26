using Application.Core;
using Application.Core.Pagination;
using Application.Features.LostItems.Queries.Common.DTOs;
using Application.Features.LostItems.Queries.Common.Enums;
using Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Features.LostItems.Queries.GetMyLostItems;

public class GetMyLostItemsQueryHandler(IAppDbContext context, ICurrentUserService currentUserService) :
    IRequestHandler<GetMyLostItemsQuery, Result<PaginatedListDto<GetLostItemDto>>>
{
    private readonly IAppDbContext _context = context;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    public async Task<Result<PaginatedListDto<GetLostItemDto>>> Handle(
        GetMyLostItemsQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.LostItems
            .Where(x => !x.IsDeleted && x.UserId == _currentUserService.UserId);

        query = ApplyFiltering(query, request);
        query = ApplySorting(query, request);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Select(x => new GetLostItemDto
            {
                Id = x.Id,
                Title = x.Title,
                Description = x.Description,
                Category = x.Category,
                ItemType = x.ItemType,
                Status = x.Status,
                IncidentDate = x.IncidentDate,
                ImageUrl = x.ImageUrl,
                LocationLabel = x.LocationLabel,
                Latitude = x.Latitude,
                Longitude = x.Longitude,
                UserId = x.UserId,
                UserFullName = x.User.FirstName + " " + x.User.LastName,
                ClaimCount = x.Claims.Count,
                CreatedDate = x.CreatedDate
            })
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var paginatedList = new PaginatedListDto<GetLostItemDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };

        return Result<PaginatedListDto<GetLostItemDto>>.Success("Kayıtlarım başarıyla getirildi", paginatedList);
    }

    private static IQueryable<Domain.LostItem> ApplySorting(IQueryable<Domain.LostItem> query, GetMyLostItemsQuery request)
    {
        return request.SortBy switch
        {
            LostItemSortField.Title => request.SortDescending
                ? query.OrderByDescending(x => x.Title)
                : query.OrderBy(x => x.Title),
            LostItemSortField.Category => request.SortDescending
                ? query.OrderByDescending(x => x.Category)
                : query.OrderBy(x => x.Category),
            LostItemSortField.IncidentDate => request.SortDescending
                ? query.OrderByDescending(x => x.IncidentDate)
                : query.OrderBy(x => x.IncidentDate),
            LostItemSortField.CreatedDate => request.SortDescending
                ? query.OrderByDescending(x => x.CreatedDate)
                : query.OrderBy(x => x.CreatedDate),
            _ => query.OrderByDescending(x => x.CreatedDate)
        };
    }

    private static IQueryable<Domain.LostItem> ApplyFiltering(IQueryable<Domain.LostItem> query, GetMyLostItemsQuery request)
    {
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(x =>
                x.Title.ToLower().Contains(request.SearchTerm.ToLower()) ||
                x.Description.ToLower().Contains(request.SearchTerm.ToLower()));
        }

        if (request.ItemType.HasValue)
        {
            query = query.Where(x => x.ItemType == request.ItemType.Value);
        }

        if (request.Category.HasValue)
        {
            query = query.Where(x => x.Category == request.Category.Value);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(x => x.Status == request.Status.Value);
        }

        return query;
    }
}
