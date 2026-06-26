using Application.Core;
using Application.Interfaces;
using Domain.Common.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Features.ItemClaims.Commands.AdminReviewClaim;

public class AdminReviewClaimCommandHandler(IAppDbContext context, ICurrentUserService currentUserService) : IRequestHandler<AdminReviewClaimCommand, Result<Unit>>
{
    private readonly IAppDbContext _context = context;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    public async Task<Result<Unit>> Handle(AdminReviewClaimCommand request, CancellationToken cancellationToken)
    {
        var claim = await _context.ItemClaims
            .Include(x => x.LostItem)
            .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);

        if (claim == null)
        {
            return Result<Unit>.Failure("Talep bulunamadı", 404);
        }

        if (claim.Status != ClaimStatus.Pending)
        {
            return Result<Unit>.Failure("Bu talep zaten değerlendirilmiş", 400);
        }

        if (request.AdminReviewClaimDto.IsApproved)
        {
            claim.Status = ClaimStatus.ApprovedByAdmin;
            claim.LostItem.Status = ItemStatus.Resolved;
        }
        else
        {
            claim.Status = ClaimStatus.RejectedByAdmin;
        }

        claim.ReviewedBy = _currentUserService.UserId;
        claim.ReviewedDate = DateTime.UtcNow;
        claim.AdminComment = request.AdminReviewClaimDto.Comment;
        claim.UpdatedDate = DateTime.UtcNow;
        claim.UpdatedBy = _currentUserService.UserId;

        var result = await _context.SaveChangesAsync(cancellationToken) > 0;

        if (!result)
        {
            return Result<Unit>.Failure("Admin değerlendirmesi kaydedilemedi", 400);
        }

        return Result<Unit>.Success("Talep başarıyla değerlendirildi", Unit.Value);
    }
}
