using Application.Core;
using Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Features.LostItems.Commands.UpdateLostItem;

public class UpdateLostItemCommandHandler : IRequestHandler<UpdateLostItemCommand, Result<Unit>>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UpdateLostItemCommandHandler(IAppDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<Unit>> Handle(UpdateLostItemCommand request, CancellationToken cancellationToken)
    {
        var lostItem = await _context.LostItems
            .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);

        if (lostItem == null)
        {
            return Result<Unit>.Failure("Kayıt bulunamadı", 404);
        }

        if (lostItem.UserId != _currentUserService.UserId)
        {
            return Result<Unit>.Failure("Bu kaydı güncelleme yetkiniz yok", 403);
        }

        lostItem.Title = request.UpdateLostItemDto.Title;
        lostItem.Description = request.UpdateLostItemDto.Description;
        lostItem.Category = request.UpdateLostItemDto.Category;
        lostItem.IncidentDate = request.UpdateLostItemDto.IncidentDate;
        lostItem.ImageUrl = request.UpdateLostItemDto.ImageUrl;
        lostItem.ContactInfo = request.UpdateLostItemDto.ContactInfo;
        lostItem.LocationLabel = request.UpdateLostItemDto.LocationLabel;
        lostItem.Latitude = request.UpdateLostItemDto.Latitude;
        lostItem.Longitude = request.UpdateLostItemDto.Longitude;

        lostItem.UpdatedDate = DateTime.UtcNow;
        lostItem.UpdatedBy = _currentUserService.UserId;

        var result = await _context.SaveChangesAsync(cancellationToken);

        if (result == 0)
        {
            return Result<Unit>.Success("Kayıtta değişiklik yapılmadı", Unit.Value);
        }

        return Result<Unit>.Success("Kayıt başarıyla güncellendi", Unit.Value);
    }
}
