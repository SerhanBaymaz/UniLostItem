using Application.Core;
using Application.Interfaces;
using Domain;
using Domain.Common.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Features.ItemClaims.Commands.CreateItemClaim;

public class CreateItemClaimCommandHandler : IRequestHandler<CreateItemClaimCommand, Result<string>>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CreateItemClaimCommandHandler(IAppDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<string>> Handle(CreateItemClaimCommand request, CancellationToken cancellationToken)
    {
        var lostItem = await _context.LostItems
            .FirstOrDefaultAsync(x => x.Id == request.CreateItemClaimDto.LostItemId && !x.IsDeleted && x.IsActive,
                cancellationToken);

        if (lostItem == null)
        {
            return Result<string>.Failure("İlan bulunamadı", 404);
        }

        if (lostItem.UserId == _currentUserService.UserId)
        {
            return Result<string>.Failure("Kendi ilanınız için talep oluşturamazsınız", 400);
        }

        var hasPendingClaim = await _context.ItemClaims
            .AnyAsync(x => x.LostItemId == request.CreateItemClaimDto.LostItemId
                        && x.ClaimantId == _currentUserService.UserId
                        && x.Status == ClaimStatus.Pending,
                cancellationToken);

        if (hasPendingClaim)
        {
            return Result<string>.Failure("Bu ilan için zaten bekleyen bir talebiniz var", 400);
        }

        var claim = new ItemClaim
        {
            Description = request.CreateItemClaimDto.Description,
            LostItemId = request.CreateItemClaimDto.LostItemId,
            ClaimantId = _currentUserService.UserId!,
            ExpiresAt = DateTime.UtcNow.AddDays(2),
            ExtensionCount = 0,
            Status = ClaimStatus.Pending,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = _currentUserService.UserId
        };

        _context.ItemClaims.Add(claim);

        var result = await _context.SaveChangesAsync(cancellationToken) > 0;

        if (!result)
        {
            return Result<string>.Failure("Talep oluşturulamadı", 400);
        }

        return Result<string>.Success("Talep başarıyla oluşturuldu", claim.Id);
    }
}
