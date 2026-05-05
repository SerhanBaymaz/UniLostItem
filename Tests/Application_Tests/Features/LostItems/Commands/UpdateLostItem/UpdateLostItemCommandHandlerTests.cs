using Application.Core;
using Application.Features.LostItems.Commands.UpdateLostItem;
using Application.Interfaces;
using Domain;
using Domain.Common.Enums;
using FluentAssertions;
using Moq;
using Persistence;
using Tests.Helpers;

namespace Tests.Application_Tests.Features.LostItems.Commands.UpdateLostItem;

public class UpdateLostItemCommandHandlerTests
{
    private const string OwnerUserId = "owner-id";
    private const string OtherUserId = "other-id";

    private async Task<AppDbContext> CreateContextWithItem(string userId = OwnerUserId)
    {
        var context = TestDbContextFactory.CreateInMemoryDbContext();
        context.Users.Add(new ApplicationUser
        {
            Id = userId,
            UserName = "owner",
            Email = "owner@test.com",
            EmailConfirmed = true,
            FirstName = "Owner",
            LastName = "User"
        });
        context.LostItems.Add(new LostItem
        {
            Id = "item-1",
            Title = "Original Title",
            Description = "Original Desc",
            Category = ItemCategory.Electronics,
            ItemType = ItemType.Lost,
            Status = ItemStatus.Active,
            IncidentDate = DateTime.UtcNow,
            LocationLabel = "Original Location",
            Latitude = 41.0,
            Longitude = 29.0,
            UserId = userId,
            IsActive = true
        });
        await context.SaveChangesAsync();
        return context;
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenItemDoesNotExist()
    {
        var context = TestDbContextFactory.CreateInMemoryDbContext();
        var currentUserMock = new Mock<ICurrentUserService>();
        currentUserMock.Setup(x => x.UserId).Returns(OwnerUserId);

        var handler = new UpdateLostItemCommandHandler(context, currentUserMock.Object);

        var command = new UpdateLostItemCommand
        {
            Id = "non-existent",
            UpdateLostItemDto = new UpdateLostItemDto
            {
                Title = "Updated",
                Description = "Updated Desc",
                Category = ItemCategory.Electronics,
                IncidentDate = DateTime.UtcNow,
                LocationLabel = "Updated Location",
                Latitude = 41.0,
                Longitude = 29.0
            }
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Code.Should().Be(404);
        result.Message.Should().Be("Kayıt bulunamadı");
    }

    [Fact]
    public async Task Handle_ShouldReturnForbidden_WhenUserIsNotOwner()
    {
        var context = await CreateContextWithItem(OwnerUserId);
        var currentUserMock = new Mock<ICurrentUserService>();
        currentUserMock.Setup(x => x.UserId).Returns(OtherUserId);

        var handler = new UpdateLostItemCommandHandler(context, currentUserMock.Object);

        var command = new UpdateLostItemCommand
        {
            Id = "item-1",
            UpdateLostItemDto = new UpdateLostItemDto
            {
                Title = "Hacked",
                Description = "Hacked Desc",
                Category = ItemCategory.BagWallet,
                IncidentDate = DateTime.UtcNow,
                LocationLabel = "Hacked Location",
                Latitude = 40.0,
                Longitude = 30.0
            }
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Code.Should().Be(403);
        result.Message.Should().Be("Bu kaydı güncelleme yetkiniz yok");
    }

    [Fact]
    public async Task Handle_ShouldUpdateItem_WhenUserIsOwner()
    {
        var context = await CreateContextWithItem(OwnerUserId);
        var currentUserMock = new Mock<ICurrentUserService>();
        currentUserMock.Setup(x => x.UserId).Returns(OwnerUserId);

        var handler = new UpdateLostItemCommandHandler(context, currentUserMock.Object);

        var command = new UpdateLostItemCommand
        {
            Id = "item-1",
            UpdateLostItemDto = new UpdateLostItemDto
            {
                Title = "New Title",
                Description = "New Desc",
                Category = ItemCategory.BagWallet,
                IncidentDate = DateTime.UtcNow.AddDays(-2),
                LocationLabel = "New Location",
                Latitude = 40.0,
                Longitude = 30.0
            }
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var item = await context.LostItems.FindAsync("item-1");
        item!.Title.Should().Be("New Title");
        item.Description.Should().Be("New Desc");
        item.Category.Should().Be(ItemCategory.BagWallet);
        item.ItemType.Should().Be(ItemType.Lost);
        item.Status.Should().Be(ItemStatus.Active);
        item.UpdatedBy.Should().Be(OwnerUserId);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenNoChangesMade()
    {
        var context = await CreateContextWithItem(OwnerUserId);
        var item = await context.LostItems.FindAsync("item-1");

        var currentUserMock = new Mock<ICurrentUserService>();
        currentUserMock.Setup(x => x.UserId).Returns(OwnerUserId);

        var handler = new UpdateLostItemCommandHandler(context, currentUserMock.Object);

        var command = new UpdateLostItemCommand
        {
            Id = "item-1",
            UpdateLostItemDto = new UpdateLostItemDto
            {
                Title = item!.Title,
                Description = item.Description,
                Category = item.Category,
                IncidentDate = item.IncidentDate,
                LocationLabel = item.LocationLabel,
                Latitude = item.Latitude,
                Longitude = item.Longitude
            }
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }
}
