using System;
using Application.Core;
using MediatR;
using Persistence;

namespace Application.SerhanKitaplar.Commands;

public class DeleteSerhanKitap
{
    public class Command : IRequest<Result<Unit>>
    {
        public required string Id { get; set; }
    }

    public class Handler(AppDbContext context) : IRequestHandler<Command, Result<Unit>>
    {
        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var serhanKitap = await context.SerhanKitaplar.FindAsync([request.Id], cancellationToken);

            if (serhanKitap == null)
            {
                return Result<Unit>.Failure("SerhanKitap not found", 404);
            }

            context.SerhanKitaplar.Remove(serhanKitap);

            var result = await context.SaveChangesAsync(cancellationToken) > 0;

            if (!result)
            {
                return Result<Unit>.Failure("Failed to delete the serhan kitap", 400);
            }

            return Result<Unit>.Success("Serhan kitap deleted successfully", Unit.Value);
        }
    }
}
