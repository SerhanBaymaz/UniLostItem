using Application.Core;
using Application.Features.ItemClaims.Queries.Common.DTOs;
using Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Features.ItemClaims.Queries.GetClaimDetails;

public class GetClaimDetailsQueryHandler(IAppDbContext context, ICurrentUserService currentUserService) : IRequestHandler<GetClaimDetailsQuery, Result<GetItemClaimDto>>
{
    private readonly IAppDbContext _context = context;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    public async Task<Result<GetItemClaimDto>> Handle(
        GetClaimDetailsQuery request,
        CancellationToken cancellationToken)
    {
        var claim = await _context.ItemClaims
            .Where(x => x.Id == request.Id)
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
            .FirstOrDefaultAsync(cancellationToken);

        if (claim == null)
        {
            return Result<GetItemClaimDto>.Failure("Talep bulunamadı", 404);
        }

        var lostItem = await _context.LostItems
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == claim.LostItemId, cancellationToken);

        if (lostItem == null)
        {
            return Result<GetItemClaimDto>.Failure("İlan bulunamadı", 404);
        }

        var isClaimant = claim.ClaimantId == _currentUserService.UserId;
        var isItemOwner = lostItem.UserId == _currentUserService.UserId;

        if (!isClaimant && !isItemOwner)
        {
            return Result<GetItemClaimDto>.Failure("Bu talebi görüntüleme yetkiniz yok", 403);
        }

        return Result<GetItemClaimDto>.Success("Talep başarıyla getirildi", claim);
    }
}
