using Application.Core;
using Application.Features.ItemClaims.Queries.GetMyClaims;
using Application.Interfaces;
using Domain;
using Domain.Common.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Persistence;
using Tests.Helpers;

namespace Tests.Application_Tests.Features.ItemClaims.Queries.GetMyClaims;

public class GetMyClaimsQueryHandlerTests
{
    private const string OwnerUserId = "owner-id";
    private const string ClaimantUserId = "claimant-id";
    private const string OtherClaimantId = "other-claimant-id";

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
        context.Users.Add(new ApplicationUser
        {
            Id = OtherClaimantId,
            UserName = "otherclaimant",
            Email = "other@test.com",
            EmailConfirmed = true,
            FirstName = "Other",
            LastName = "Claimant"
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
            Description = "My claim",
            LostItemId = "item-1",
            ClaimantId = ClaimantUserId,
            Status = ClaimStatus.Pending,
            ExpiresAt = DateTime.UtcNow.AddDays(2),
            ExtensionCount = 0,
            IsActive = true
        });
        context.ItemClaims.Add(new ItemClaim
        {
            Description = "Other claim",
            LostItemId = "item-1",
            ClaimantId = OtherClaimantId,
            Status = ClaimStatus.Pending,
            ExpiresAt = DateTime.UtcNow.AddDays(2),
            ExtensionCount = 0,
            IsActive = true
        });
        await context.SaveChangesAsync();
        return context;
    }

    [Fact]
    public async Task Handle_ShouldReturnOnlyCurrentUserClaims()
    {
        var context = await SeedContext();

        var currentUserMock = new Mock<ICurrentUserService>();
        currentUserMock.Setup(x => x.UserId).Returns(ClaimantUserId);

        var handler = new GetMyClaimsQueryHandler(context, currentUserMock.Object);
        var query = new GetMyClaimsQuery();

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(1);
        result.Value.Items[0].ClaimantId.Should().Be(ClaimantUserId);
    }

    [Fact]
    public async Task Handle_ShouldFilterByStatus()
    {
        var context = await SeedContext();
        var claim = await context.ItemClaims.FirstAsync(x => x.ClaimantId == ClaimantUserId);
        claim.Status = ClaimStatus.ApprovedByOwner;
        await context.SaveChangesAsync();

        var currentUserMock = new Mock<ICurrentUserService>();
        currentUserMock.Setup(x => x.UserId).Returns(ClaimantUserId);

        var handler = new GetMyClaimsQueryHandler(context, currentUserMock.Object);
        var query = new GetMyClaimsQuery { Status = ClaimStatus.Pending };

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldReturnPaginationMetadata()
    {
        var context = await SeedContext();
        context.ItemClaims.Add(new ItemClaim
        {
            Description = "Second my claim",
            LostItemId = "item-1",
            ClaimantId = ClaimantUserId,
            Status = ClaimStatus.Cancelled,
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            ExtensionCount = 0,
            IsActive = true
        });
        await context.SaveChangesAsync();

        var currentUserMock = new Mock<ICurrentUserService>();
        currentUserMock.Setup(x => x.UserId).Returns(ClaimantUserId);

        var handler = new GetMyClaimsQueryHandler(context, currentUserMock.Object);
        var query = new GetMyClaimsQuery { PageNumber = 1, PageSize = 1 };

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.TotalCount.Should().Be(2);
        result.Value.TotalPages.Should().Be(2);
        result.Value.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmpty_WhenNoClaims()
    {
        var context = TestDbContextFactory.CreateInMemoryDbContext();
        context.Users.Add(new ApplicationUser
        {
            Id = ClaimantUserId,
            UserName = "claimant",
            Email = "claimant@test.com",
            EmailConfirmed = true,
            FirstName = "Claimant",
            LastName = "User"
        });
        await context.SaveChangesAsync();

        var currentUserMock = new Mock<ICurrentUserService>();
        currentUserMock.Setup(x => x.UserId).Returns(ClaimantUserId);

        var handler = new GetMyClaimsQueryHandler(context, currentUserMock.Object);
        var query = new GetMyClaimsQuery();

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);
    }
}
