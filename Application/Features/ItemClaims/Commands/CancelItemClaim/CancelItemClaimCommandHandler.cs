using Application.Core;
using Application.Interfaces;
using Domain.Common.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Features.ItemClaims.Commands.CancelItemClaim;

public class CancelItemClaimCommandHandler(IAppDbContext context, ICurrentUserService currentUserService) : IRequestHandler<CancelItemClaimCommand, Result<Unit>>
{
    private readonly IAppDbContext _context = context;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    public async Task<Result<Unit>> Handle(CancelItemClaimCommand request, CancellationToken cancellationToken)
    {
        var claim = await _context.ItemClaims
            .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);

        if (claim == null)
        {
            return Result<Unit>.Failure("Talep bulunamadı", 404);
        }

        if (claim.ClaimantId != _currentUserService.UserId)
        {
            return Result<Unit>.Failure("Bu talebi iptal etme yetkiniz yok", 403);
        }

        if (claim.Status != ClaimStatus.Pending)
        {
            return Result<Unit>.Failure("Sadece bekleyen talepler iptal edilebilir", 400);
        }

        claim.Status = ClaimStatus.Cancelled;
        claim.UpdatedDate = DateTime.UtcNow;
        claim.UpdatedBy = _currentUserService.UserId;

        var result = await _context.SaveChangesAsync(cancellationToken) > 0;

        if (!result)
        {
            return Result<Unit>.Failure("Talep iptal edilemedi", 400);
        }

        return Result<Unit>.Success("Talep başarıyla iptal edildi", Unit.Value);
    }
}
