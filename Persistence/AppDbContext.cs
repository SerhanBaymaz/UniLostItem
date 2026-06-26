using Domain;
using Domain.Common.Enums;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Persistence;

public class AppDbContext(DbContextOptions options) : IdentityDbContext<ApplicationUser>(options), IAppDbContext
{
    public DbSet<LostItem> LostItems { get; set; } = null!;
    public DbSet<ItemClaim> ItemClaims { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(e => e.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.LastName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.ProfileImageUrl).HasMaxLength(500);
        });

        builder.Entity<LostItem>(entity =>
        {
            entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(2000).IsRequired();
            entity.Property(e => e.LocationLabel).HasMaxLength(300).IsRequired();
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
            entity.Property(e => e.ContactInfo).HasMaxLength(300);

            entity.HasOne(e => e.User)
                  .WithMany(u => u.LostItems)
                  .HasForeignKey(e => e.UserId)
                  .IsRequired()
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.ItemType);
            entity.HasIndex(e => e.Category);
        });

        builder.Entity<ItemClaim>(entity =>
        {
            entity.Property(e => e.Description).HasMaxLength(1000).IsRequired();
            entity.Property(e => e.OwnerComment).HasMaxLength(500);
            entity.Property(e => e.AdminComment).HasMaxLength(500);

            entity.HasOne(e => e.LostItem)
                  .WithMany(l => l.Claims)
                  .HasForeignKey(e => e.LostItemId)
                  .IsRequired()
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Claimant)
                  .WithMany(u => u.Claims)
                  .HasForeignKey(e => e.ClaimantId)
                  .IsRequired()
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.LostItemId);
            entity.HasIndex(e => e.ClaimantId);
            entity.HasIndex(e => e.Status);
        });
    }
}
