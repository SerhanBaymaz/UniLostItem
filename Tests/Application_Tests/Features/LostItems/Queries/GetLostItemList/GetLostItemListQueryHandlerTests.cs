using Application.Core;
using Application.Features.LostItems.Queries.GetLostItemList;
using Application.Features.LostItems.Queries.Common.Enums;
using Domain;
using Domain.Common.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Tests.Helpers;

namespace Tests.Application_Tests.Features.LostItems.Queries.GetLostItemList;

public class GetLostItemListQueryHandlerTests
{
    private readonly AppDbContext _context;

    public GetLostItemListQueryHandlerTests()
    {
        _context = TestDbContextFactory.CreateInMemoryDbContext();
    }

    private async Task SeedData()
    {
        var user = new ApplicationUser
        {
            Id = "user-1",
            UserName = "testuser",
            Email = "test@test.com",
            EmailConfirmed = true,
            FirstName = "Test",
            LastName = "User"
        };
        _context.Users.Add(user);

        var items = new List<LostItem>
        {
            new()
            {
                Title = "Alpha Item",
                Description = "First item",
                Category = ItemCategory.Electronics,
                ItemType = ItemType.Lost,
                Status = ItemStatus.Active,
                IncidentDate = DateTime.UtcNow.AddDays(-2),
                LocationLabel = "Location A",
                Latitude = 41.0,
                Longitude = 29.0,
                UserId = "user-1",
                IsActive = true
            },
            new()
            {
                Title = "Beta Item",
                Description = "Second item",
                Category = ItemCategory.Electronics,
                ItemType = ItemType.Found,
                Status = ItemStatus.Active,
                IncidentDate = DateTime.UtcNow.AddDays(-1),
                LocationLabel = "Location B",
                Latitude = 41.1,
                Longitude = 29.1,
                UserId = "user-1",
                IsActive = true
            },
            new()
            {
                Title = "Gamma Item",
                Description = "Third item",
                Category = ItemCategory.BagWallet,
                ItemType = ItemType.Found,
                Status = ItemStatus.Active,
                IncidentDate = DateTime.UtcNow,
                LocationLabel = "Location C",
                Latitude = 41.2,
                Longitude = 29.2,
                UserId = "user-1",
                IsActive = true
            }
        };

        _context.LostItems.AddRange(items);
        await _context.SaveChangesAsync();
    }

    [Fact]
    public async Task Handle_ShouldReturnAllActiveItems()
    {
        await SeedData();
        var handler = new GetLostItemListQueryHandler(_context);

        var query = new GetLostItemListQuery();

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(3);
    }

    [Fact]
    public async Task Handle_ShouldFilterByItemType()
    {
        await SeedData();
        var handler = new GetLostItemListQueryHandler(_context);

        var query = new GetLostItemListQuery { ItemType = ItemType.Found };

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(2);
        result.Value.Items.All(x => x.ItemType == ItemType.Found).Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldFilterByCategory()
    {
        await SeedData();
        var handler = new GetLostItemListQueryHandler(_context);

        var query = new GetLostItemListQuery { Category = ItemCategory.BagWallet };

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(1);
        result.Value.Items[0].Title.Should().Be("Gamma Item");
    }

    [Fact]
    public async Task Handle_ShouldFilterBySearchTerm()
    {
        await SeedData();
        var handler = new GetLostItemListQueryHandler(_context);

        var query = new GetLostItemListQuery { SearchTerm = "beta" };

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(1);
        result.Value.Items[0].Title.Should().Be("Beta Item");
    }

    [Fact]
    public async Task Handle_ShouldExcludeSoftDeletedItems()
    {
        await SeedData();
        var handler = new GetLostItemListQueryHandler(_context);

        var item = await _context.LostItems.FirstAsync();
        item.IsDeleted = true;
        await _context.SaveChangesAsync();

        var query = new GetLostItemListQuery();
        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_ShouldReturnPaginationMetadata()
    {
        await SeedData();
        var handler = new GetLostItemListQueryHandler(_context);

        var query = new GetLostItemListQuery { PageNumber = 1, PageSize = 2 };

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.TotalCount.Should().Be(3);
        result.Value.PageNumber.Should().Be(1);
        result.Value.PageSize.Should().Be(2);
        result.Value.TotalPages.Should().Be(2);
        result.Value.HasNext.Should().BeTrue();
        result.Value.HasPrevious.Should().BeFalse();
        result.Value.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_ShouldSortByTitleAscending()
    {
        await SeedData();
        var handler = new GetLostItemListQueryHandler(_context);

        var query = new GetLostItemListQuery { SortBy = LostItemSortField.Title, SortDescending = false };

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Items[0].Title.Should().Be("Alpha Item");
        result.Value.Items[1].Title.Should().Be("Beta Item");
        result.Value.Items[2].Title.Should().Be("Gamma Item");
    }

    [Fact]
    public async Task Handle_ShouldSortByTitleDescending()
    {
        await SeedData();
        var handler = new GetLostItemListQueryHandler(_context);

        var query = new GetLostItemListQuery { SortBy = LostItemSortField.Title, SortDescending = true };

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Items[0].Title.Should().Be("Gamma Item");
    }
}
