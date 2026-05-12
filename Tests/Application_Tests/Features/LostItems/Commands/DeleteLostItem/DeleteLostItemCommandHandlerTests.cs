using Application.Core;
using Application.Features.LostItems.Commands.DeleteLostItem;
using Application.Interfaces;
using Domain;
using Domain.Common.Enums;
using FluentAssertions;
using Moq;
using Persistence;
using Tests.Helpers;

namespace Tests.Application_Tests.Features.LostItems.Commands.DeleteLostItem;

public class DeleteLostItemCommandHandlerTests
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
            Title = "Test Item",
            Description = "Test Desc",
            Category = ItemCategory.Electronics,
            ItemType = ItemType.Lost,
            Status = ItemStatus.Active,
            IncidentDate = DateTime.UtcNow,
            LocationLabel = "Test Location",
            Latitude = 41.0,
            Longitude = 29.0,
            ContactInfo = "test@test.com",
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

        var handler = new DeleteLostItemCommandHandler(context, currentUserMock.Object);

        var command = new DeleteLostItemCommand { Id = "non-existent" };

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

        var handler = new DeleteLostItemCommandHandler(context, currentUserMock.Object);

        var command = new DeleteLostItemCommand { Id = "item-1" };

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Code.Should().Be(403);
        result.Message.Should().Be("Bu kaydı silme yetkiniz yok");
    }

    [Fact]
    public async Task Handle_ShouldSoftDelete_WhenUserIsOwner()
    {
        var context = await CreateContextWithItem(OwnerUserId);
        var currentUserMock = new Mock<ICurrentUserService>();
        currentUserMock.Setup(x => x.UserId).Returns(OwnerUserId);

        var handler = new DeleteLostItemCommandHandler(context, currentUserMock.Object);

        var command = new DeleteLostItemCommand { Id = "item-1" };

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Be("Kayıt başarıyla silindi");

        var item = await context.LostItems.FindAsync("item-1");
        item.Should().NotBeNull();
        item!.IsDeleted.Should().BeTrue();
        item.UpdatedBy.Should().Be(OwnerUserId);
        item.UpdatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenItemAlreadySoftDeleted()
    {
        var context = await CreateContextWithItem(OwnerUserId);
        var item = await context.LostItems.FindAsync("item-1");
        item!.IsDeleted = true;
        await context.SaveChangesAsync();

        var currentUserMock = new Mock<ICurrentUserService>();
        currentUserMock.Setup(x => x.UserId).Returns(OwnerUserId);

        var handler = new DeleteLostItemCommandHandler(context, currentUserMock.Object);

        var command = new DeleteLostItemCommand { Id = "item-1" };

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Code.Should().Be(404);
    }
}
