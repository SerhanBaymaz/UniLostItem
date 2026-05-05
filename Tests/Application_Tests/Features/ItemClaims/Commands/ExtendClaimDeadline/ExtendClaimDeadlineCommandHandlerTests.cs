using Application.Core;
using Application.Features.ItemClaims.Commands.ExtendClaimDeadline;
using Application.Interfaces;
using Domain;
using Domain.Common.Enums;
using FluentAssertions;
using Moq;
using Persistence;
using Tests.Helpers;

namespace Tests.Application_Tests.Features.ItemClaims.Commands.ExtendClaimDeadline;

public class ExtendClaimDeadlineCommandHandlerTests
{
    private const string OwnerUserId = "owner-id";
    private const string ClaimantUserId = "claimant-id";
    private const string OtherUserId = "other-id";

    private async Task<AppDbContext> CreateContextWithPendingClaim(int extensionCount = 0)
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
            ExtensionCount = extensionCount,
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

        var handler = new ExtendClaimDeadlineCommandHandler(context, currentUserMock.Object);

        var command = new ExtendClaimDeadlineCommand { Id = "non-existent" };

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

        var handler = new ExtendClaimDeadlineCommandHandler(context, currentUserMock.Object);

        var command = new ExtendClaimDeadlineCommand { Id = "claim-1" };

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Code.Should().Be(403);
        result.Message.Should().Be("Bu talebin süresini uzatma yetkiniz yok");
    }

    [Fact]
    public async Task Handle_ShouldReturnBadRequest_WhenClaimNotPending()
    {
        var context = await CreateContextWithPendingClaim();
        var claim = await context.ItemClaims.FindAsync("claim-1");
        claim!.Status = ClaimStatus.RejectedByOwner;
        await context.SaveChangesAsync();

        var currentUserMock = new Mock<ICurrentUserService>();
        currentUserMock.Setup(x => x.UserId).Returns(OwnerUserId);

        var handler = new ExtendClaimDeadlineCommandHandler(context, currentUserMock.Object);

        var command = new ExtendClaimDeadlineCommand { Id = "claim-1" };

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Code.Should().Be(400);
        result.Message.Should().Be("Sadece bekleyen taleplerin süresi uzatılabilir");
    }

    [Fact]
    public async Task Handle_ShouldReturnBadRequest_WhenMaxExtensionsReached()
    {
        var context = await CreateContextWithPendingClaim(extensionCount: 2);
        var currentUserMock = new Mock<ICurrentUserService>();
        currentUserMock.Setup(x => x.UserId).Returns(OwnerUserId);

        var handler = new ExtendClaimDeadlineCommandHandler(context, currentUserMock.Object);

        var command = new ExtendClaimDeadlineCommand { Id = "claim-1" };

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Code.Should().Be(400);
        result.Message.Should().Be("Talep süresi en fazla 2 kez uzatılabilir");
    }

    [Fact]
    public async Task Handle_ShouldExtend_WhenPendingAndUnderLimit()
    {
        var context = await CreateContextWithPendingClaim(extensionCount: 0);
        var originalExpiry = (await context.ItemClaims.FindAsync("claim-1"))!.ExpiresAt;

        var currentUserMock = new Mock<ICurrentUserService>();
        currentUserMock.Setup(x => x.UserId).Returns(OwnerUserId);

        var handler = new ExtendClaimDeadlineCommandHandler(context, currentUserMock.Object);

        var command = new ExtendClaimDeadlineCommand { Id = "claim-1" };

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Be("Talep süresi başarıyla uzatıldı");

        var claim = await context.ItemClaims.FindAsync("claim-1");
        claim!.ExtensionCount.Should().Be(1);
        claim.ExpiresAt.Should().BeCloseTo(originalExpiry.AddDays(2), TimeSpan.FromSeconds(5));
        claim.UpdatedBy.Should().Be(OwnerUserId);
    }
}
