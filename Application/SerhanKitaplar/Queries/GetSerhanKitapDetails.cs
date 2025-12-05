using System;
using Application.Core;
using Domain;
using MediatR;
using Persistence;

namespace Application.SerhanKitaplar.Queries;

public class GetSerhanKitapDetails
{
    public class Query : IRequest<Result<SerhanKitap>>
    {
        public required string Id { get; set; }
    }

    public class Handler(AppDbContext context) : IRequestHandler<Query, Result<SerhanKitap>>
    {
        public async Task<Result<SerhanKitap>> Handle(Query request, CancellationToken cancellationToken)
        {
            var serhanKitap = await context.SerhanKitaplar.FindAsync([request.Id], cancellationToken);

            if (serhanKitap == null)
            {
                return Result<SerhanKitap>.Failure("SerhanKitap not found", 404);
            }

            return Result<SerhanKitap>.Success("SerhanKitap retrieved successfully", serhanKitap!);
        }
    }
}
