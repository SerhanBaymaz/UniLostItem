using System.Threading;
using System.Threading.Tasks;
using Application.Core;
using Application.Interfaces;
using Domain;
using MediatR;
using Persistence;

namespace Application.Features.SerhanKitaplar.Commands.EditSerhanKitap;

public class EditSerhanKitapCommandHandler : IRequestHandler<EditSerhanKitapCommand, Result<Unit>>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public EditSerhanKitapCommandHandler(IAppDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<Unit>> Handle(EditSerhanKitapCommand request, CancellationToken cancellationToken)
    {
        var serhanKitap = await _context.SerhanKitaplar.FindAsync(new object[] { request.Id }, cancellationToken);

        if (serhanKitap == null)
        {
            return Result<Unit>.Failure("SerhanKitap not found", 404);
        }

        serhanKitap.KitapName = request.EditSerhanKitapDto.KitapName;
        serhanKitap.KitapYazar = request.EditSerhanKitapDto.KitapYazar;
        serhanKitap.KitapSayfaSayisi = request.EditSerhanKitapDto.KitapSayfaSayisi;

        // Update audit fields
        serhanKitap.UpdatedDate = System.DateTime.UtcNow;
        serhanKitap.UpdatedBy = _currentUserService.UserId;

        var result = await _context.SaveChangesAsync(cancellationToken);

        if (result == 0)
        {
            return Result<Unit>.Success($"No changes were made to the {serhanKitap.KitapName}.", Unit.Value);
        }

        return Result<Unit>.Success("SerhanKitap updated successfully.", Unit.Value);
    }
}
