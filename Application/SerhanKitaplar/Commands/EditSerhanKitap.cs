using System;
using Application.Core;
using Domain;
using MediatR;
using Persistence;

namespace Application.SerhanKitaplar.Commands;

public class EditSerhanKitap
{
    public class Command : IRequest<Result<Unit>>
    {
        public required SerhanKitap SerhanKitap { get; set; }
    }

    public class Handler(AppDbContext context) : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var serhanKitap = await context.SerhanKitaplar.FindAsync([request.SerhanKitap.Id], cancellationToken);

            if (serhanKitap == null)
            {
                return Result<Unit>.Failure("SerhanKitap not found", 404);
            }

            serhanKitap.KitapName = request.SerhanKitap.KitapName;
            serhanKitap.KitapYazar = request.SerhanKitap.KitapYazar;
            serhanKitap.KitapSayfaSayisi = request.SerhanKitap.KitapSayfaSayisi;

            var result = await context.SaveChangesAsync(cancellationToken);

            if (result == 0)
            {
                // No changes were made in the database, return success with message
                return Result<Unit>.Success($"No changes were made to the {serhanKitap.KitapName}.", Unit.Value);
            }

            return Result<Unit>.Success("SerhanKitap updated successfully.", Unit.Value);
        }
    }
}
