using Application.Core;
using Application.Features.ItemClaims.Commands.CreateItemClaim;
using Application.Interfaces;
using Domain;
using Domain.Common.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Persistence;
using Tests.Helpers;

namespace Tests.Application_Tests.Features.ItemClaims.Commands.CreateItemClaim;

public class CreateItemClaimCommandHandlerTests
{
    private const string OwnerUserId = "owner-id";
    private const string ClaimantUserId = "claimant-id";

    private readonly AppDbContext _context;
    private readonly Mock<ICurrentUserService> _currentUserMock;
    private readonly CreateItemClaimCommandHandler _handler;

    public CreateItemClaimCommandHandlerTests()
    {
        _context = TestDbContextFactory.CreateInMemoryDbContext();
        _currentUserMock = new Mock<ICurrentUserService>();
        _currentUserMock.Setup(x => x.UserId).Returns(ClaimantUserId);
        _handler = new CreateItemClaimCommandHandler(_context, _currentUserMock.Object);
    }

    private async Task SeedItem()
    {
        _context.Users.Add(new ApplicationUser
        {
            Id = OwnerUserId,
            UserName = "owner",
            Email = "owner@test.com",
            EmailConfirmed = true,
            FirstName = "Owner",
            LastName = "User"
        });
        _context.Users.Add(new ApplicationUser
        {
            Id = ClaimantUserId,
            UserName = "claimant",
            Email = "claimant@test.com",
            EmailConfirmed = true,
            FirstName = "Claimant",
            LastName = "User"
        });
        _context.LostItems.Add(new LostItem
        {
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
        await _context.SaveChangesAsync();
    }

    [Fact]
    public async Task Handle_ShouldCreateClaim_WhenValid()
    {
        await SeedItem();

        var command = new CreateItemClaimCommand
        {
            CreateItemClaimDto = new CreateItemClaimDto
            {
                LostItemId = (await _context.LostItems.FirstAsync()).Id,
                Description = "This is my item, I have proof"
            }
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Be("Talep başarıyla oluşturuldu");
        result.Value.Should().NotBeNullOrEmpty();

        var savedClaim = await _context.ItemClaims.FirstOrDefaultAsync();
        savedClaim.Should().NotBeNull();
        savedClaim!.Description.Should().Be("This is my item, I have proof");
        savedClaim.ClaimantId.Should().Be(ClaimantUserId);
        savedClaim.Status.Should().Be(ClaimStatus.Pending);
        savedClaim.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddDays(2), TimeSpan.FromSeconds(5));
        savedClaim.ExtensionCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenLostItemDoesNotExist()
    {
        _context.Users.Add(new ApplicationUser
        {
            Id = ClaimantUserId,
            UserName = "claimant",
            Email = "claimant@test.com",
            EmailConfirmed = true,
            FirstName = "Claimant",
            LastName = "User"
        });
        await _context.SaveChangesAsync();

        var command = new CreateItemClaimCommand
        {
            CreateItemClaimDto = new CreateItemClaimDto
            {
                LostItemId = "non-existent",
                Description = "Claim description"
            }
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Code.Should().Be(404);
        result.Message.Should().Be("İlan bulunamadı");
    }

    [Fact]
    public async Task Handle_ShouldReturnBadRequest_WhenClaimingOwnItem()
    {
        await SeedItem();

        var currentUserMock = new Mock<ICurrentUserService>();
        currentUserMock.Setup(x => x.UserId).Returns(OwnerUserId);
        var handler = new CreateItemClaimCommandHandler(_context, currentUserMock.Object);

        var command = new CreateItemClaimCommand
        {
            CreateItemClaimDto = new CreateItemClaimDto
            {
                LostItemId = (await _context.LostItems.FirstAsync()).Id,
                Description = "My own item"
            }
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Code.Should().Be(400);
        result.Message.Should().Be("Kendi ilanınız için talep oluşturamazsınız");
    }

    [Fact]
    public async Task Handle_ShouldReturnBadRequest_WhenDuplicatePendingClaim()
    {
        await SeedItem();
        var itemId = (await _context.LostItems.FirstAsync()).Id;

        _context.ItemClaims.Add(new ItemClaim
        {
            Description = "First claim",
            LostItemId = itemId,
            ClaimantId = ClaimantUserId,
            Status = ClaimStatus.Pending,
            ExpiresAt = DateTime.UtcNow.AddDays(2),
            ExtensionCount = 0,
            IsActive = true
        });
        await _context.SaveChangesAsync();

        var command = new CreateItemClaimCommand
        {
            CreateItemClaimDto = new CreateItemClaimDto
            {
                LostItemId = itemId,
                Description = "Duplicate claim"
            }
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Code.Should().Be(400);
        result.Message.Should().Be("Bu ilan için zaten bekleyen bir talebiniz var");
    }
}
