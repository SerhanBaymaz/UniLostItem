using Application.Core;
using Application.Core.Pagination;
using Application.Features.ItemClaims.Queries.Common.DTOs;
using Application.Features.ItemClaims.Queries.Common.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Features.ItemClaims.Queries.GetClaimsByItem;

public class GetClaimsByItemQueryHandler(IAppDbContext context) :
    IRequestHandler<GetClaimsByItemQuery, Result<PaginatedListDto<GetItemClaimDto>>>
{
    private readonly IAppDbContext _context = context;

    public async Task<Result<PaginatedListDto<GetItemClaimDto>>> Handle(
        GetClaimsByItemQuery request,
        CancellationToken cancellationToken)
    {
        var lostItem = await _context.LostItems
            .FirstOrDefaultAsync(x => x.Id == request.LostItemId && !x.IsDeleted, cancellationToken);

        if (lostItem == null)
        {
            return Result<PaginatedListDto<GetItemClaimDto>>.Failure("İlan bulunamadı", 404);
        }

        var query = _context.ItemClaims
            .Where(x => x.LostItemId == request.LostItemId && !x.IsDeleted);

        if (request.Status.HasValue)
        {
            query = query.Where(x => x.Status == request.Status.Value);
        }

        query = ApplySorting(query, request);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Select(x => new GetItemClaimDto
            {
                Id = x.Id,
                Description = x.Description,
                Status = x.Status,
                ExpiresAt = x.ExpiresAt,
                ExtensionCount = x.ExtensionCount,
                LostItemId = x.LostItemId,
                LostItemTitle = x.LostItem.Title,
                ClaimantId = x.ClaimantId,
                ClaimantFullName = x.Claimant.FirstName + " " + x.Claimant.LastName,
                OwnerComment = x.OwnerComment,
                OwnerResponseDate = x.OwnerResponseDate,
                ReviewedBy = x.ReviewedBy,
                ReviewedDate = x.ReviewedDate,
                AdminComment = x.AdminComment,
                CreatedDate = x.CreatedDate
            })
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var paginatedList = new PaginatedListDto<GetItemClaimDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };

        return Result<PaginatedListDto<GetItemClaimDto>>.Success("Talepler başarıyla getirildi", paginatedList);
    }

    private static IQueryable<Domain.ItemClaim> ApplySorting(IQueryable<Domain.ItemClaim> query, GetClaimsByItemQuery request)
    {
        return request.SortBy switch
        {
            ItemClaimSortField.CreatedDate => request.SortDescending
                ? query.OrderByDescending(x => x.CreatedDate)
                : query.OrderBy(x => x.CreatedDate),
            ItemClaimSortField.Status => request.SortDescending
                ? query.OrderByDescending(x => x.Status)
                : query.OrderBy(x => x.Status),
            ItemClaimSortField.ExpiresAt => request.SortDescending
                ? query.OrderByDescending(x => x.ExpiresAt)
                : query.OrderBy(x => x.ExpiresAt),
            _ => query.OrderByDescending(x => x.CreatedDate)
        };
    }
}
