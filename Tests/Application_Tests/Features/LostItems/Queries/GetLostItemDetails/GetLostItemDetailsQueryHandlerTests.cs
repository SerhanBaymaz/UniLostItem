using Application.Core;
using Application.Features.LostItems.Queries.GetLostItemDetails;
using Domain;
using Domain.Common.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Tests.Helpers;

namespace Tests.Application_Tests.Features.LostItems.Queries.GetLostItemDetails;

public class GetLostItemDetailsQueryHandlerTests
{
    private readonly AppDbContext _context;

    public GetLostItemDetailsQueryHandlerTests()
    {
        _context = TestDbContextFactory.CreateInMemoryDbContext();
    }

    private async Task<string> SeedItem()
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

        var item = new LostItem
        {
            Title = "iPhone 15",
            Description = "Siyah renk iPhone",
            Category = ItemCategory.Electronics,
            ItemType = ItemType.Lost,
            Status = ItemStatus.Active,
            IncidentDate = DateTime.UtcNow.AddDays(-1),
            ImageUrl = "https://example.com/image.jpg",
            ContactInfo = "test@test.com",
            LocationLabel = "Kütüphane B Blok",
            Latitude = 41.0082,
            Longitude = 28.9784,
            UserId = "user-1",
            IsActive = true
        };

        _context.LostItems.Add(item);
        await _context.SaveChangesAsync();
        return item.Id;
    }

    [Fact]
    public async Task Handle_ShouldReturnItemDetails()
    {
        var itemId = await SeedItem();
        var handler = new GetLostItemDetailsQueryHandler(_context);

        var query = new GetLostItemDetailsQuery { Id = itemId };

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Title.Should().Be("iPhone 15");
        result.Value.ContactInfo.Should().Be("test@test.com");
        result.Value.ImageUrl.Should().Be("https://example.com/image.jpg");
        result.Value.UserFullName.Should().Be("Test User");
        result.Value.IsDeleted.Should().BeFalse();
        result.Value.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenItemDoesNotExist()
    {
        var handler = new GetLostItemDetailsQueryHandler(_context);

        var query = new GetLostItemDetailsQuery { Id = "non-existent" };

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Code.Should().Be(404);
        result.Message.Should().Be("Kayıt bulunamadı");
    }

    [Fact]
    public async Task Handle_ShouldReturnGone_WhenItemIsSoftDeleted()
    {
        var itemId = await SeedItem();
        var item = await _context.LostItems.FirstAsync();
        item.IsDeleted = true;
        await _context.SaveChangesAsync();

        var handler = new GetLostItemDetailsQueryHandler(_context);

        var query = new GetLostItemDetailsQuery { Id = itemId };

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Code.Should().Be(410);
        result.Message.Should().Be("Kayıt silinmiş");
    }

    [Fact]
    public async Task Handle_ShouldReturnLocked_WhenItemIsInactive()
    {
        var itemId = await SeedItem();
        var item = await _context.LostItems.FirstAsync();
        item.IsActive = false;
        await _context.SaveChangesAsync();

        var handler = new GetLostItemDetailsQueryHandler(_context);

        var query = new GetLostItemDetailsQuery { Id = itemId };

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Code.Should().Be(423);
        result.Message.Should().Be("Kayıt aktif değil");
    }
}
