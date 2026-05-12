using Application.Core;
using Application.Features.LostItems.Queries.GetMyLostItems;
using Application.Interfaces;
using Domain;
using Domain.Common.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Persistence;
using Tests.Helpers;

namespace Tests.Application_Tests.Features.LostItems.Queries.GetMyLostItems;

public class GetMyLostItemsQueryHandlerTests
{
    private readonly AppDbContext _context;

    private const string OwnerId = "owner-123";
    private const string OtherId = "other-456";

    public GetMyLostItemsQueryHandlerTests()
    {
        _context = TestDbContextFactory.CreateInMemoryDbContext();
    }

    private async Task SeedData()
    {
        var owner = new ApplicationUser { Id = OwnerId, UserName = "owner", Email = "owner@test.com", EmailConfirmed = true, FirstName = "Owner", LastName = "User" };
        var other = new ApplicationUser { Id = OtherId, UserName = "other", Email = "other@test.com", EmailConfirmed = true, FirstName = "Other", LastName = "User" };
        _context.Users.AddRange(owner, other);

        var items = new List<LostItem>
        {
            new()
            {
                Title = "My Item 1",
                Description = "Owner's item",
                Category = ItemCategory.Electronics,
                ItemType = ItemType.Lost,
                Status = ItemStatus.Active,
                IncidentDate = DateTime.UtcNow,
                LocationLabel = "Loc1",
                Latitude = 41.0,
                Longitude = 29.0,
                ContactInfo = "owner1@test.com",
                UserId = OwnerId,
                IsActive = true
            },
            new()
            {
                Title = "My Item 2",
                Description = "Owner's second item",
                Category = ItemCategory.BagWallet,
                ItemType = ItemType.Found,
                Status = ItemStatus.Active,
                IncidentDate = DateTime.UtcNow,
                LocationLabel = "Loc2",
                Latitude = 41.1,
                Longitude = 29.1,
                ContactInfo = "owner2@test.com",
                UserId = OwnerId,
                IsActive = true
            },
            new()
            {
                Title = "Other's Item",
                Description = "Belongs to other user",
                Category = ItemCategory.Key,
                ItemType = ItemType.Lost,
                Status = ItemStatus.Active,
                IncidentDate = DateTime.UtcNow,
                LocationLabel = "Loc3",
                Latitude = 41.2,
                Longitude = 29.2,
                ContactInfo = "other@test.com",
                UserId = OtherId,
                IsActive = true
            }
        };

        _context.LostItems.AddRange(items);
        await _context.SaveChangesAsync();
    }

    [Fact]
    public async Task Handle_ShouldReturnOnlyOwnItems()
    {
        await SeedData();
        var currentUserMock = new Mock<ICurrentUserService>();
        currentUserMock.Setup(x => x.UserId).Returns(OwnerId);

        var handler = new GetMyLostItemsQueryHandler(_context, currentUserMock.Object);

        var query = new GetMyLostItemsQuery();

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(2);
        result.Value.Items.All(x => x.UserId == OwnerId).Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldFilterByCategory()
    {
        await SeedData();
        var currentUserMock = new Mock<ICurrentUserService>();
        currentUserMock.Setup(x => x.UserId).Returns(OwnerId);

        var handler = new GetMyLostItemsQueryHandler(_context, currentUserMock.Object);

        var query = new GetMyLostItemsQuery { Category = ItemCategory.BagWallet };

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(1);
        result.Value.Items[0].Title.Should().Be("My Item 2");
    }

    [Fact]
    public async Task Handle_ShouldExcludeSoftDeletedItems()
    {
        await SeedData();
        var currentUserMock = new Mock<ICurrentUserService>();
        currentUserMock.Setup(x => x.UserId).Returns(OwnerId);

        var item = await _context.LostItems.FirstAsync(x => x.UserId == OwnerId);
        item.IsDeleted = true;
        await _context.SaveChangesAsync();

        var handler = new GetMyLostItemsQueryHandler(_context, currentUserMock.Object);

        var query = new GetMyLostItemsQuery();

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenUserHasNoItems()
    {
        await SeedData();
        var currentUserMock = new Mock<ICurrentUserService>();
        currentUserMock.Setup(x => x.UserId).Returns("no-items-user");

        var handler = new GetMyLostItemsQueryHandler(_context, currentUserMock.Object);

        var query = new GetMyLostItemsQuery();

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);
    }
}
