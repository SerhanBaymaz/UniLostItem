using System;
using System.Threading;
using System.Threading.Tasks;
using Domain;
using Microsoft.EntityFrameworkCore;

namespace Persistence;

public interface IAppDbContext : IAsyncDisposable
{
    DbSet<LostItem> LostItems { get; set; }
    DbSet<ItemClaim> ItemClaims { get; set; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
