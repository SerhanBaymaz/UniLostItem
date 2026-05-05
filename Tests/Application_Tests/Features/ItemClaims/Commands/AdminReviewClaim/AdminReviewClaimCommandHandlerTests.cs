using Application.Core;
using Application.Features.ItemClaims.Commands.AdminReviewClaim;
using Application.Interfaces;
using Domain;
using Domain.Common.Enums;
using FluentAssertions;
using Moq;
using Persistence;
using Tests.Helpers;

namespace Tests.Application_Tests.Features.ItemClaims.Commands.AdminReviewClaim;

public class AdminReviewClaimCommandHandlerTests
{
    private const string OwnerUserId = "owner-id";
    private const string ClaimantUserId = "claimant-id";
    private const string AdminUserId = "admin-id";

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
        currentUserMock.Setup(x => x.UserId).Returns(AdminUserId);

        var handler = new AdminReviewClaimCommandHandler(context, currentUserMock.Object);

        var command = new AdminReviewClaimCommand
        {
            Id = "non-existent",
            AdminReviewClaimDto = new AdminReviewClaimDto { IsApproved = true }
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Code.Should().Be(404);
        result.Message.Should().Be("Talep bulunamadı");
    }

    [Fact]
    public async Task Handle_ShouldReturnBadRequest_WhenClaimNotPending()
    {
        var context = await CreateContextWithPendingClaim();
        var claim = await context.ItemClaims.FindAsync("claim-1");
        claim!.Status = ClaimStatus.ApprovedByOwner;
        await context.SaveChangesAsync();

        var currentUserMock = new Mock<ICurrentUserService>();
        currentUserMock.Setup(x => x.UserId).Returns(AdminUserId);

        var handler = new AdminReviewClaimCommandHandler(context, currentUserMock.Object);

        var command = new AdminReviewClaimCommand
        {
            Id = "claim-1",
            AdminReviewClaimDto = new AdminReviewClaimDto { IsApproved = true }
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Code.Should().Be(400);
        result.Message.Should().Be("Bu talep zaten değerlendirilmiş");
    }

    [Fact]
    public async Task Handle_ShouldApprove_WhenAdminApproves()
    {
        var context = await CreateContextWithPendingClaim();
        var currentUserMock = new Mock<ICurrentUserService>();
        currentUserMock.Setup(x => x.UserId).Returns(AdminUserId);

        var handler = new AdminReviewClaimCommandHandler(context, currentUserMock.Object);

        var command = new AdminReviewClaimCommand
        {
            Id = "claim-1",
            AdminReviewClaimDto = new AdminReviewClaimDto { IsApproved = true, Comment = "Admin approved" }
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Be("Talep başarıyla değerlendirildi");

        var claim = await context.ItemClaims.FindAsync("claim-1");
        claim!.Status.Should().Be(ClaimStatus.ApprovedByAdmin);
        claim.ReviewedBy.Should().Be(AdminUserId);
        claim.AdminComment.Should().Be("Admin approved");
        claim.ReviewedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        var lostItem = await context.LostItems.FindAsync("item-1");
        lostItem!.Status.Should().Be(ItemStatus.Resolved);
    }

    [Fact]
    public async Task Handle_ShouldReject_WhenAdminRejects()
    {
        var context = await CreateContextWithPendingClaim();
        var currentUserMock = new Mock<ICurrentUserService>();
        currentUserMock.Setup(x => x.UserId).Returns(AdminUserId);

        var handler = new AdminReviewClaimCommandHandler(context, currentUserMock.Object);

        var command = new AdminReviewClaimCommand
        {
            Id = "claim-1",
            AdminReviewClaimDto = new AdminReviewClaimDto { IsApproved = false, Comment = "Insufficient proof" }
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        var claim = await context.ItemClaims.FindAsync("claim-1");
        claim!.Status.Should().Be(ClaimStatus.RejectedByAdmin);
        claim.AdminComment.Should().Be("Insufficient proof");

        var lostItem = await context.LostItems.FindAsync("item-1");
        lostItem!.Status.Should().Be(ItemStatus.Active);
    }
}
