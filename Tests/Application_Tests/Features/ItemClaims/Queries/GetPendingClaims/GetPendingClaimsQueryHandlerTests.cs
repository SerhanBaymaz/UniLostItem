using Application.Core;
using Application.Features.ItemClaims.Queries.GetPendingClaims;
using Domain;
using Domain.Common.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Tests.Helpers;

namespace Tests.Application_Tests.Features.ItemClaims.Queries.GetPendingClaims;

public class GetPendingClaimsQueryHandlerTests
{
    private const string OwnerUserId = "owner-id";
    private const string ClaimantUserId = "claimant-id";

    private async Task<AppDbContext> SeedContext()
    {
        var context = TestDbContextFactory.CreateInMemoryDbContext();
        context.Users.Add(new ApplicationUser
        {
            Id = OwnerUserId,
            UserName = "owner",
            Email = "owner@test.com",
            EmailConfirmed = true,
            FirstName = "Owner",
            LastName = "User"
        });
        context.Users.Add(new ApplicationUser
        {
            Id = ClaimantUserId,
            UserName = "claimant",
            Email = "claimant@test.com",
            EmailConfirmed = true,
            FirstName = "Claimant",
            LastName = "User"
        });
        context.LostItems.Add(new LostItem
        {
            Id = "item-1",
            Title = "Test Item",
            Description = "Test",
            Category = ItemCategory.Electronics,
            ItemType = ItemType.Lost,
            Status = ItemStatus.Active,
            IncidentDate = DateTime.UtcNow,
            LocationLabel = "Test",
            Latitude = 41.0,
            Longitude = 29.0,
            UserId = OwnerUserId,
            IsActive = true
        });
        context.ItemClaims.Add(new ItemClaim
        {
            Description = "Pending claim",
            LostItemId = "item-1",
            ClaimantId = ClaimantUserId,
            Status = ClaimStatus.Pending,
            ExpiresAt = DateTime.UtcNow.AddDays(2),
            ExtensionCount = 0,
            IsActive = true
        });
        context.ItemClaims.Add(new ItemClaim
        {
            Description = "Approved claim",
            LostItemId = "item-1",
            ClaimantId = ClaimantUserId,
            Status = ClaimStatus.ApprovedByOwner,
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            ExtensionCount = 0,
            IsActive = true
        });
        context.ItemClaims.Add(new ItemClaim
        {
            Description = "Cancelled claim",
            LostItemId = "item-1",
            ClaimantId = ClaimantUserId,
            Status = ClaimStatus.Cancelled,
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            ExtensionCount = 0,
            IsActive = true
        });
        await context.SaveChangesAsync();
        return context;
    }

    [Fact]
    public async Task Handle_ShouldReturnOnlyPendingClaims()
    {
        var context = await SeedContext();
        var handler = new GetPendingClaimsQueryHandler(context);

        var result = await handler.Handle(new GetPendingClaimsQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(1);
        result.Value.Items[0].Status.Should().Be(ClaimStatus.Pending);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmpty_WhenNoPendingClaims()
    {
        var context = TestDbContextFactory.CreateInMemoryDbContext();
        context.Users.Add(new ApplicationUser
        {
            Id = OwnerUserId,
            UserName = "owner",
            Email = "owner@test.com",
            EmailConfirmed = true,
            FirstName = "Owner",
            LastName = "User"
        });
        await context.SaveChangesAsync();

        var handler = new GetPendingClaimsQueryHandler(context);

        var result = await handler.Handle(new GetPendingClaimsQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ShouldFilterBySearchTerm()
    {
        var context = await SeedContext();
        var handler = new GetPendingClaimsQueryHandler(context);

        var result = await handler.Handle(
            new GetPendingClaimsQuery { SearchTerm = "Pending claim" }, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(1);
        result.Value.Items[0].Description.Should().Be("Pending claim");
    }

    [Fact]
    public async Task Handle_ShouldReturnPaginationMetadata()
    {
        var context = TestDbContextFactory.CreateInMemoryDbContext();
        context.Users.Add(new ApplicationUser
        {
            Id = OwnerUserId,
            UserName = "owner",
            Email = "owner@test.com",
            EmailConfirmed = true,
            FirstName = "Owner",
            LastName = "User"
        });
        context.Users.Add(new ApplicationUser
        {
            Id = ClaimantUserId,
            UserName = "claimant",
            Email = "claimant@test.com",
            EmailConfirmed = true,
            FirstName = "Claimant",
            LastName = "User"
        });
        context.LostItems.Add(new LostItem
        {
            Id = "item-1",
            Title = "Test Item",
            Description = "Test",
            Category = ItemCategory.Electronics,
            ItemType = ItemType.Lost,
            Status = ItemStatus.Active,
            IncidentDate = DateTime.UtcNow,
            LocationLabel = "Test",
            Latitude = 41.0,
            Longitude = 29.0,
            UserId = OwnerUserId,
            IsActive = true
        });
        context.ItemClaims.Add(new ItemClaim
        {
            Description = "Pending A",
            LostItemId = "item-1",
            ClaimantId = ClaimantUserId,
            Status = ClaimStatus.Pending,
            ExpiresAt = DateTime.UtcNow.AddDays(2),
            ExtensionCount = 0,
            IsActive = true
        });
        context.ItemClaims.Add(new ItemClaim
        {
            Description = "Pending B",
            LostItemId = "item-1",
            ClaimantId = ClaimantUserId,
            Status = ClaimStatus.Pending,
            ExpiresAt = DateTime.UtcNow.AddDays(2),
            ExtensionCount = 0,
            IsActive = true
        });
        await context.SaveChangesAsync();

        var handler = new GetPendingClaimsQueryHandler(context);

        var result = await handler.Handle(
            new GetPendingClaimsQuery { PageNumber = 1, PageSize = 1 }, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.TotalCount.Should().Be(2);
        result.Value.TotalPages.Should().Be(2);
        result.Value.HasNext.Should().BeTrue();
        result.Value.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_ShouldExcludeSoftDeleted()
    {
        var context = await SeedContext();
        var claim = await context.ItemClaims.FirstAsync(x => x.Status == ClaimStatus.Pending);
        claim.IsDeleted = true;
        await context.SaveChangesAsync();

        var handler = new GetPendingClaimsQueryHandler(context);

        var result = await handler.Handle(new GetPendingClaimsQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().BeEmpty();
    }
}
