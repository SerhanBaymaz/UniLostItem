using Application.Core;
using Application.Interfaces;
using Domain.Common.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Features.ItemClaims.Commands.ExtendClaimDeadline;

public class ExtendClaimDeadlineCommandHandler(IAppDbContext context, ICurrentUserService currentUserService) : IRequestHandler<ExtendClaimDeadlineCommand, Result<Unit>>
{
    private readonly IAppDbContext _context = context;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    public async Task<Result<Unit>> Handle(ExtendClaimDeadlineCommand request, CancellationToken cancellationToken)
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
            return Result<Unit>.Failure("Bu talebin süresini uzatma yetkiniz yok", 403);
        }

        if (claim.Status != ClaimStatus.Pending)
        {
            return Result<Unit>.Failure("Sadece bekleyen taleplerin süresi uzatılabilir", 400);
        }

        if (claim.ExtensionCount >= 2)
        {
            return Result<Unit>.Failure("Talep süresi en fazla 2 kez uzatılabilir", 400);
        }

        claim.ExpiresAt = claim.ExpiresAt.AddDays(2);
        claim.ExtensionCount++;
        claim.UpdatedDate = DateTime.UtcNow;
        claim.UpdatedBy = _currentUserService.UserId;

        var result = await _context.SaveChangesAsync(cancellationToken) > 0;

        if (!result)
        {
            return Result<Unit>.Failure("Talep süresi uzatılamadı", 400);
        }

        return Result<Unit>.Success("Talep süresi başarıyla uzatıldı", Unit.Value);
    }
}
