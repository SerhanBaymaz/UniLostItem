using Application.Core;
using Application.Features.ItemClaims.Queries.GetClaimsByItem;
using Domain;
using Domain.Common.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Tests.Helpers;

namespace Tests.Application_Tests.Features.ItemClaims.Queries.GetClaimsByItem;

public class GetClaimsByItemQueryHandlerTests
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
            Description = "Test Desc",
            Category = ItemCategory.Electronics,
            ItemType = ItemType.Lost,
            Status = ItemStatus.Active,
            IncidentDate = DateTime.UtcNow,
            LocationLabel = "Test",
            Latitude = 41.0,
            Longitude = 29.0,
            ContactInfo = "owner@test.com",
            UserId = OwnerUserId,
            IsActive = true
        });
        context.ItemClaims.Add(new ItemClaim
        {
            Description = "First claim",
            LostItemId = "item-1",
            ClaimantId = ClaimantUserId,
            Status = ClaimStatus.Pending,
            ExpiresAt = DateTime.UtcNow.AddDays(2),
            ExtensionCount = 0,
            IsActive = true
        });
        context.ItemClaims.Add(new ItemClaim
        {
            Description = "Second claim",
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
    public async Task Handle_ShouldReturnAllClaims_WhenAnyoneRequests()
    {
        var context = await SeedContext();
        var itemId = (await context.LostItems.FirstAsync()).Id;

        var handler = new GetClaimsByItemQueryHandler(context);
        var query = new GetClaimsByItemQuery { LostItemId = itemId };

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_ShouldFilterByStatus()
    {
        var context = await SeedContext();
        var itemId = (await context.LostItems.FirstAsync()).Id;

        var handler = new GetClaimsByItemQueryHandler(context);
        var query = new GetClaimsByItemQuery { LostItemId = itemId, Status = ClaimStatus.Pending };

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(1);
        result.Value.Items[0].Status.Should().Be(ClaimStatus.Pending);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenItemDoesNotExist()
    {
        var context = TestDbContextFactory.CreateInMemoryDbContext();

        var handler = new GetClaimsByItemQueryHandler(context);
        var query = new GetClaimsByItemQuery { LostItemId = "non-existent" };

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Code.Should().Be(404);
        result.Message.Should().Be("İlan bulunamadı");
    }

    [Fact]
    public async Task Handle_ShouldReturnPaginationMetadata()
    {
        var context = await SeedContext();
        var itemId = (await context.LostItems.FirstAsync()).Id;

        var handler = new GetClaimsByItemQueryHandler(context);
        var query = new GetClaimsByItemQuery { LostItemId = itemId, PageNumber = 1, PageSize = 1 };

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.TotalCount.Should().Be(2);
        result.Value.TotalPages.Should().Be(2);
        result.Value.HasNext.Should().BeTrue();
        result.Value.Items.Should().HaveCount(1);
    }
}
