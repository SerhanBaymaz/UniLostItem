using System.Threading;
using System.Threading.Tasks;
using Application.Core;
using Domain;
using MediatR;
using Persistence;

namespace Application.Features.SerhanKitaplar.Commands.EditSerhanKitap;

public class EditSerhanKitapCommandHandler : IRequestHandler<EditSerhanKitapCommand, Result<Unit>>
{
    private readonly IAppDbContext _context;

    public EditSerhanKitapCommandHandler(IAppDbContext context)
    {
        _context = context;
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

        var result = await _context.SaveChangesAsync(cancellationToken);

        if (result == 0)
        {
            return Result<Unit>.Success($"No changes were made to the {serhanKitap.KitapName}.", Unit.Value);
        }

        return Result<Unit>.Success("SerhanKitap updated successfully.", Unit.Value);
    }
}
