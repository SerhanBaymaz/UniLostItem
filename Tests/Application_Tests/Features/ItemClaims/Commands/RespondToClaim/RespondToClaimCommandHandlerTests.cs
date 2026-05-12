using Application.Core;
using Application.Features.ItemClaims.Commands.RespondToClaim;
using Application.Interfaces;
using Domain;
using Domain.Common.Enums;
using FluentAssertions;
using Moq;
using Persistence;
using Tests.Helpers;

namespace Tests.Application_Tests.Features.ItemClaims.Commands.RespondToClaim;

public class RespondToClaimCommandHandlerTests
{
    private const string OwnerUserId = "owner-id";
    private const string ClaimantUserId = "claimant-id";
    private const string OtherUserId = "other-id";

    private async Task<AppDbContext> CreateContextWithPendingClaim()
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
            Id = "claim-1",
            Description = "My item",
            LostItemId = "item-1",
            ClaimantId = ClaimantUserId,
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
        currentUserMock.Setup(x => x.UserId).Returns(OwnerUserId);

        var handler = new RespondToClaimCommandHandler(context, currentUserMock.Object);

        var command = new RespondToClaimCommand
        {
            Id = "non-existent",
            RespondToClaimDto = new RespondToClaimDto { IsApproved = true }
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Code.Should().Be(404);
        result.Message.Should().Be("Talep bulunamadı");
    }

    [Fact]
    public async Task Handle_ShouldReturnForbidden_WhenUserIsNotItemOwner()
    {
        var context = await CreateContextWithPendingClaim();
        var currentUserMock = new Mock<ICurrentUserService>();
        currentUserMock.Setup(x => x.UserId).Returns(OtherUserId);

        var handler = new RespondToClaimCommandHandler(context, currentUserMock.Object);

        var command = new RespondToClaimCommand
        {
            Id = "claim-1",
            RespondToClaimDto = new RespondToClaimDto { IsApproved = true }
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Code.Should().Be(403);
        result.Message.Should().Be("Bu talebi değerlendirme yetkiniz yok");
    }

    [Fact]
    public async Task Handle_ShouldReturnBadRequest_WhenClaimNotPending()
    {
        var context = await CreateContextWithPendingClaim();
        var claim = await context.ItemClaims.FindAsync("claim-1");
        claim!.Status = ClaimStatus.Cancelled;
        await context.SaveChangesAsync();

        var currentUserMock = new Mock<ICurrentUserService>();
        currentUserMock.Setup(x => x.UserId).Returns(OwnerUserId);

        var handler = new RespondToClaimCommandHandler(context, currentUserMock.Object);

        var command = new RespondToClaimCommand
        {
            Id = "claim-1",
            RespondToClaimDto = new RespondToClaimDto { IsApproved = true }
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Code.Should().Be(400);
        result.Message.Should().Be("Bu talep zaten değerlendirilmiş");
    }

    [Fact]
    public async Task Handle_ShouldApprove_WhenPendingAndApproved()
    {
        var context = await CreateContextWithPendingClaim();
        var currentUserMock = new Mock<ICurrentUserService>();
        currentUserMock.Setup(x => x.UserId).Returns(OwnerUserId);

        var handler = new RespondToClaimCommandHandler(context, currentUserMock.Object);

        var command = new RespondToClaimCommand
        {
            Id = "claim-1",
            RespondToClaimDto = new RespondToClaimDto { IsApproved = true, Comment = "Verified, item returned" }
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Be("Talep başarıyla yanıtlandı");

        var claim = await context.ItemClaims.FindAsync("claim-1");
        claim!.Status.Should().Be(ClaimStatus.ApprovedByOwner);
        claim.OwnerComment.Should().Be("Verified, item returned");
        claim.OwnerResponseDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        claim.UpdatedBy.Should().Be(OwnerUserId);

        var lostItem = await context.LostItems.FindAsync("item-1");
        lostItem!.Status.Should().Be(ItemStatus.Resolved);
    }

    [Fact]
    public async Task Handle_ShouldReject_WhenPendingAndRejected()
    {
        var context = await CreateContextWithPendingClaim();
        var currentUserMock = new Mock<ICurrentUserService>();
        currentUserMock.Setup(x => x.UserId).Returns(OwnerUserId);

        var handler = new RespondToClaimCommandHandler(context, currentUserMock.Object);

        var command = new RespondToClaimCommand
        {
            Id = "claim-1",
            RespondToClaimDto = new RespondToClaimDto { IsApproved = false, Comment = "Not a match" }
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        var claim = await context.ItemClaims.FindAsync("claim-1");
        claim!.Status.Should().Be(ClaimStatus.RejectedByOwner);
        claim.OwnerComment.Should().Be("Not a match");

        var lostItem = await context.LostItems.FindAsync("item-1");
        lostItem!.Status.Should().Be(ItemStatus.Active);
    }
}
