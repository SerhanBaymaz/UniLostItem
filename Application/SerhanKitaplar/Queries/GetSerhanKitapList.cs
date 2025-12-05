using System;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.SerhanKitaplar.Queries;

public class GetSerhanKitapList
{
    public class Query : IRequest<List<SerhanKitap>> { }

    public class Handler(AppDbContext context) : IRequestHandler<Query, List<SerhanKitap>>
    {
        public async Task<List<SerhanKitap>> Handle(Query request, CancellationToken cancellationToken)
        {
            return await context.SerhanKitaplar.ToListAsync(cancellationToken);
        }
    }
}
