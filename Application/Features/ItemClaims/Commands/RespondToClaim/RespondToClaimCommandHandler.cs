using Application.Core;
using Application.Interfaces;
using Domain.Common.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Features.ItemClaims.Commands.RespondToClaim;

public class RespondToClaimCommandHandler : IRequestHandler<RespondToClaimCommand, Result<Unit>>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public RespondToClaimCommandHandler(IAppDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<Unit>> Handle(RespondToClaimCommand request, CancellationToken cancellationToken)
    {
        var claim = await _context.ItemClaims
            .Include(x => x.LostItem)
            .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);

        if (claim == null)
        {
            return Result<Unit>.Failure("Talep bulunamadı", 404);
        }

        if (claim.LostItem.UserId != _currentUserService.UserId)
        {
            return Result<Unit>.Failure("Bu talebi değerlendirme yetkiniz yok", 403);
        }

        if (claim.Status != ClaimStatus.Pending)
        {
            return Result<Unit>.Failure("Bu talep zaten değerlendirilmiş", 400);
        }

        if (request.RespondToClaimDto.IsApproved)
        {
            claim.Status = ClaimStatus.ApprovedByOwner;
            claim.LostItem.Status = ItemStatus.Resolved;
        }
        else
        {
            claim.Status = ClaimStatus.RejectedByOwner;
        }

        claim.OwnerComment = request.RespondToClaimDto.Comment;
        claim.OwnerResponseDate = DateTime.UtcNow;
        claim.UpdatedDate = DateTime.UtcNow;
        claim.UpdatedBy = _currentUserService.UserId;

        var result = await _context.SaveChangesAsync(cancellationToken) > 0;

        if (!result)
        {
            return Result<Unit>.Failure("Talep yanıtlanamadı", 400);
        }

        return Result<Unit>.Success("Talep başarıyla yanıtlandı", Unit.Value);
    }
}
