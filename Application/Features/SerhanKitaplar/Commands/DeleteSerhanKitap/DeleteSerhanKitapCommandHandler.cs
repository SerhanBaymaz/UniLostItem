using System.Threading;
using System.Threading.Tasks;
using Application.Core;
using Application.Interfaces;
using MediatR;
using Persistence;

namespace Application.Features.SerhanKitaplar.Commands.DeleteSerhanKitap;

public class DeleteSerhanKitapCommandHandler : IRequestHandler<DeleteSerhanKitapCommand, Result<Unit>>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public DeleteSerhanKitapCommandHandler(IAppDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<Unit>> Handle(DeleteSerhanKitapCommand request, CancellationToken cancellationToken)
    {
        var serhanKitap = await _context.SerhanKitaplar.FindAsync(new object[] { request.Id }, cancellationToken);

        if (serhanKitap == null)
        {
            return Result<Unit>.Failure("SerhanKitap not found", 404);
        }

        // Soft delete instead of physical removal
        serhanKitap.IsDeleted = true;
        serhanKitap.UpdatedDate = System.DateTime.UtcNow;
        serhanKitap.UpdatedBy = _currentUserService.UserId;

        var result = await _context.SaveChangesAsync(cancellationToken) > 0;

        if (!result)
        {
            return Result<Unit>.Failure("Failed to delete the serhan kitap", 400);
        }

        return Result<Unit>.Success("Serhan kitap deleted successfully", Unit.Value);
    }
}
