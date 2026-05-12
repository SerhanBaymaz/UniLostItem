using Application.Features.LostItems.Commands.UpdateLostItem;
using Application.Interfaces;
using Domain;
using Domain.Common.Enums;
using FluentAssertions;
using Moq;
using Tests.Helpers;

namespace Tests.Application_Tests.Features.LostItems.Commands.UpdateLostItem;

public class UpdateLostItemImageUploadTests
{
    private const string OwnerUserId = "owner-id";

    private static async Task<Persistence.AppDbContext> CreateContextWithItem(
        string? imageUrl = null, string? imagePublicId = null)
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
            ContactInfo = "original@test.com",
            UserId = OwnerUserId,
            IsActive = true,
            ImageUrl = imageUrl,
            ImagePublicId = imagePublicId
        });
        await context.SaveChangesAsync();
        return context;
    }

    [Fact]
    public async Task Handle_WithNewImage_ShouldUploadAndSetImageFields()
    {
        var context = await CreateContextWithItem();
        var currentUserMock = new Mock<ICurrentUserService>();
        var imageStorageMock = new Mock<IImageStorageService>();

        currentUserMock.Setup(x => x.UserId).Returns(OwnerUserId);
        imageStorageMock
            .Setup(x => x.UploadImageAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ImageUploadResult
            {
                Url = "https://res.cloudinary.com/demo/new.jpg",
                PublicId = "unilostitem/new123"
            });

        var handler = new UpdateLostItemCommandHandler(context, currentUserMock.Object, imageStorageMock.Object);

        var imageStream = new MemoryStream(new byte[] { 1, 2, 3 });
        var command = new UpdateLostItemCommand
        {
            Id = "item-1",
            UpdateLostItemDto = new UpdateLostItemDto
            {
                Title = "Updated",
                Description = "Updated Desc",
                Category = ItemCategory.Electronics,
                IncidentDate = DateTime.UtcNow,
                LocationLabel = "Updated Location",
                Latitude = 41.0,
                Longitude = 29.0,
                ContactInfo = "updated@test.com",
                ImageStream = imageStream,
                ImageFileName = "new_photo.jpg"
            }
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        var item = await context.LostItems.FindAsync("item-1");
        item!.ImageUrl.Should().Be("https://res.cloudinary.com/demo/new.jpg");
        item.ImagePublicId.Should().Be("unilostitem/new123");
    }

    [Fact]
    public async Task Handle_WithNewImage_ShouldDeleteOldImage()
    {
        var context = await CreateContextWithItem("https://old.jpg", "unilostitem/old123");
        var currentUserMock = new Mock<ICurrentUserService>();
        var imageStorageMock = new Mock<IImageStorageService>();

        currentUserMock.Setup(x => x.UserId).Returns(OwnerUserId);
        imageStorageMock
            .Setup(x => x.UploadImageAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ImageUploadResult
            {
                Url = "https://res.cloudinary.com/demo/new.jpg",
                PublicId = "unilostitem/new123"
            });
        imageStorageMock
            .Setup(x => x.DeleteImageAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new UpdateLostItemCommandHandler(context, currentUserMock.Object, imageStorageMock.Object);

        var command = new UpdateLostItemCommand
        {
            Id = "item-1",
            UpdateLostItemDto = new UpdateLostItemDto
            {
                Title = "Updated",
                Description = "Updated Desc",
                Category = ItemCategory.Electronics,
                IncidentDate = DateTime.UtcNow,
                LocationLabel = "Updated Location",
                Latitude = 41.0,
                Longitude = 29.0,
                ContactInfo = "updated@test.com",
                ImageStream = new MemoryStream(new byte[] { 1, 2, 3 }),
                ImageFileName = "new_photo.jpg"
            }
        };

        await handler.Handle(command, CancellationToken.None);

        imageStorageMock.Verify(x => x.DeleteImageAsync("unilostitem/old123", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithRemoveImage_ShouldDeleteAndClearFields()
    {
        var context = await CreateContextWithItem("https://old.jpg", "unilostitem/old123");
        var currentUserMock = new Mock<ICurrentUserService>();
        var imageStorageMock = new Mock<IImageStorageService>();

        currentUserMock.Setup(x => x.UserId).Returns(OwnerUserId);
        imageStorageMock
            .Setup(x => x.DeleteImageAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new UpdateLostItemCommandHandler(context, currentUserMock.Object, imageStorageMock.Object);

        var command = new UpdateLostItemCommand
        {
            Id = "item-1",
            UpdateLostItemDto = new UpdateLostItemDto
            {
                Title = "Updated",
                Description = "Updated Desc",
                Category = ItemCategory.Electronics,
                IncidentDate = DateTime.UtcNow,
                LocationLabel = "Updated Location",
                Latitude = 41.0,
                Longitude = 29.0,
                ContactInfo = "updated@test.com",
                RemoveImage = true
            }
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        var item = await context.LostItems.FindAsync("item-1");
        item!.ImageUrl.Should().BeNull();
        item.ImagePublicId.Should().BeNull();

        imageStorageMock.Verify(x => x.DeleteImageAsync("unilostitem/old123", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithoutImageChange_ShouldNotCallImageService()
    {
        var context = await CreateContextWithItem("https://existing.jpg", "unilostitem/existing");
        var currentUserMock = new Mock<ICurrentUserService>();
        var imageStorageMock = new Mock<IImageStorageService>();

        currentUserMock.Setup(x => x.UserId).Returns(OwnerUserId);

        var handler = new UpdateLostItemCommandHandler(context, currentUserMock.Object, imageStorageMock.Object);

        var command = new UpdateLostItemCommand
        {
            Id = "item-1",
            UpdateLostItemDto = new UpdateLostItemDto
            {
                Title = "Updated",
                Description = "Updated Desc",
                Category = ItemCategory.Electronics,
                IncidentDate = DateTime.UtcNow,
                LocationLabel = "Updated Location",
                Latitude = 41.0,
                Longitude = 29.0,
                ContactInfo = "updated@test.com"
            }
        };

        await handler.Handle(command, CancellationToken.None);

        var item = await context.LostItems.FindAsync("item-1");
        item!.ImageUrl.Should().Be("https://existing.jpg");
        item.ImagePublicId.Should().Be("unilostitem/existing");

        imageStorageMock.Verify(x => x.UploadImageAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        imageStorageMock.Verify(x => x.DeleteImageAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
