using Application.Core;
using Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Features.LostItems.Commands.DeleteLostItem;

public class DeleteLostItemCommandHandler : IRequestHandler<DeleteLostItemCommand, Result<Unit>>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public DeleteLostItemCommandHandler(IAppDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<Unit>> Handle(DeleteLostItemCommand request, CancellationToken cancellationToken)
    {
        var lostItem = await _context.LostItems
            .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);

        if (lostItem == null)
        {
            return Result<Unit>.Failure("Kayıt bulunamadı", 404);
        }

        if (lostItem.UserId != _currentUserService.UserId)
        {
            return Result<Unit>.Failure("Bu kaydı silme yetkiniz yok", 403);
        }

        lostItem.IsDeleted = true;
        lostItem.UpdatedDate = DateTime.UtcNow;
        lostItem.UpdatedBy = _currentUserService.UserId;

        var result = await _context.SaveChangesAsync(cancellationToken) > 0;

        if (!result)
        {
            return Result<Unit>.Failure("Kayıt silinemedi", 400);
        }

        return Result<Unit>.Success("Kayıt başarıyla silindi", Unit.Value);
    }
}
