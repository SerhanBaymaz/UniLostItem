using Application.Core;
using Application.Features.ItemClaims.Commands.CancelItemClaim;
using Application.Interfaces;
using Domain;
using Domain.Common.Enums;
using FluentAssertions;
using Moq;
using Persistence;
using Tests.Helpers;

namespace Tests.Application_Tests.Features.ItemClaims.Commands.CancelItemClaim;

public class CancelItemClaimCommandHandlerTests
{
    private const string OwnerUserId = "owner-id";
    private const string ClaimantUserId = "claimant-id";
    private const string OtherUserId = "other-id";

    private async Task<AppDbContext> CreateContextWithClaim(string claimantId = ClaimantUserId)
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
            Id = claimantId,
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
            UserId = OwnerUserId,
            IsActive = true
        });
        context.ItemClaims.Add(new ItemClaim
        {
            Id = "claim-1",
            Description = "My item",
            LostItemId = "item-1",
            ClaimantId = claimantId,
            Status = ClaimStatus.Pending,
            ExpiresAt = DateTime.UtcNow.AddDays(2),
            ExtensionCount = 0,
            IsActive = true
        });
        await context.SaveChangesAsync();
        return context;
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenClaimDoesNotExist()
    {
        var context = TestDbContextFactory.CreateInMemoryDbContext();
        var currentUserMock = new Mock<ICurrentUserService>();
        currentUserMock.Setup(x => x.UserId).Returns(ClaimantUserId);

        var handler = new CancelItemClaimCommandHandler(context, currentUserMock.Object);

        var command = new CancelItemClaimCommand { Id = "non-existent" };

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Code.Should().Be(404);
        result.Message.Should().Be("Talep bulunamadı");
    }

    [Fact]
    public async Task Handle_ShouldReturnForbidden_WhenUserIsNotClaimant()
    {
        var context = await CreateContextWithClaim(ClaimantUserId);
        var currentUserMock = new Mock<ICurrentUserService>();
        currentUserMock.Setup(x => x.UserId).Returns(OtherUserId);

        var handler = new CancelItemClaimCommandHandler(context, currentUserMock.Object);

        var command = new CancelItemClaimCommand { Id = "claim-1" };

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Code.Should().Be(403);
        result.Message.Should().Be("Bu talebi iptal etme yetkiniz yok");
    }

    [Fact]
    public async Task Handle_ShouldReturnBadRequest_WhenClaimNotPending()
    {
        var context = await CreateContextWithClaim(ClaimantUserId);
        var claim = await context.ItemClaims.FindAsync("claim-1");
        claim!.Status = ClaimStatus.ApprovedByOwner;
        await context.SaveChangesAsync();

        var currentUserMock = new Mock<ICurrentUserService>();
        currentUserMock.Setup(x => x.UserId).Returns(ClaimantUserId);

        var handler = new CancelItemClaimCommandHandler(context, currentUserMock.Object);

        var command = new CancelItemClaimCommand { Id = "claim-1" };

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Code.Should().Be(400);
        result.Message.Should().Be("Sadece bekleyen talepler iptal edilebilir");
    }

    [Fact]
    public async Task Handle_ShouldCancel_WhenPending()
    {
        var context = await CreateContextWithClaim(ClaimantUserId);
        var currentUserMock = new Mock<ICurrentUserService>();
        currentUserMock.Setup(x => x.UserId).Returns(ClaimantUserId);

        var handler = new CancelItemClaimCommandHandler(context, currentUserMock.Object);

        var command = new CancelItemClaimCommand { Id = "claim-1" };

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Be("Talep başarıyla iptal edildi");

        var claim = await context.ItemClaims.FindAsync("claim-1");
        claim!.Status.Should().Be(ClaimStatus.Cancelled);
        claim.UpdatedBy.Should().Be(ClaimantUserId);
    }
}
