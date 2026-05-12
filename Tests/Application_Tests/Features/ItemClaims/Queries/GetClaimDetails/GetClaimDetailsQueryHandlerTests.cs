using Application.Core;
using Application.Features.ItemClaims.Queries.GetClaimDetails;
using Application.Interfaces;
using Domain;
using Domain.Common.Enums;
using FluentAssertions;
using Moq;
using Persistence;
using Tests.Helpers;

namespace Tests.Application_Tests.Features.ItemClaims.Queries.GetClaimDetails;

public class GetClaimDetailsQueryHandlerTests
{
    private const string OwnerUserId = "owner-id";
    private const string ClaimantUserId = "claimant-id";
    private const string UnrelatedUserId = "unrelated-id";

    private async Task<(AppDbContext context, string claimId)> SeedContext()
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
            ContactInfo = "owner@test.com",
            UserId = OwnerUserId,
            IsActive = true
        });
        var claim = new ItemClaim
        {
            Description = "This is my item",
            LostItemId = "item-1",
            ClaimantId = ClaimantUserId,
            Status = ClaimStatus.Pending,
            ExpiresAt = DateTime.UtcNow.AddDays(2),
            ExtensionCount = 0,
            IsActive = true
        };
        context.ItemClaims.Add(claim);
        await context.SaveChangesAsync();
        return (context, claim.Id);
    }

    [Fact]
    public async Task Handle_ShouldReturnClaimDetails_WhenClaimantRequests()
    {
        var (context, claimId) = await SeedContext();

        var currentUserMock = new Mock<ICurrentUserService>();
        currentUserMock.Setup(x => x.UserId).Returns(ClaimantUserId);

        var handler = new GetClaimDetailsQueryHandler(context, currentUserMock.Object);
        var query = new GetClaimDetailsQuery { Id = claimId };

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Description.Should().Be("This is my item");
        result.Value.ClaimantId.Should().Be(ClaimantUserId);
        result.Value.ClaimantFullName.Should().Be("Claimant User");
        result.Value.LostItemTitle.Should().Be("Test Item");
    }

    [Fact]
    public async Task Handle_ShouldReturnClaimDetails_WhenItemOwnerRequests()
    {
        var (context, claimId) = await SeedContext();

        var currentUserMock = new Mock<ICurrentUserService>();
        currentUserMock.Setup(x => x.UserId).Returns(OwnerUserId);

        var handler = new GetClaimDetailsQueryHandler(context, currentUserMock.Object);
        var query = new GetClaimDetailsQuery { Id = claimId };

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenClaimDoesNotExist()
    {
        var context = TestDbContextFactory.CreateInMemoryDbContext();
        var currentUserMock = new Mock<ICurrentUserService>();
        currentUserMock.Setup(x => x.UserId).Returns(ClaimantUserId);

        var handler = new GetClaimDetailsQueryHandler(context, currentUserMock.Object);
        var query = new GetClaimDetailsQuery { Id = "non-existent" };

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Code.Should().Be(404);
        result.Message.Should().Be("Talep bulunamadı");
    }

    [Fact]
    public async Task Handle_ShouldReturnForbidden_WhenUserIsUnrelated()
    {
        var (context, claimId) = await SeedContext();

        var currentUserMock = new Mock<ICurrentUserService>();
        currentUserMock.Setup(x => x.UserId).Returns(UnrelatedUserId);

        var handler = new GetClaimDetailsQueryHandler(context, currentUserMock.Object);
        var query = new GetClaimDetailsQuery { Id = claimId };

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Code.Should().Be(403);
        result.Message.Should().Be("Bu talebi görüntüleme yetkiniz yok");
    }
}
