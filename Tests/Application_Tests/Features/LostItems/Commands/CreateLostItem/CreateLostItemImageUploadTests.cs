using Application.Features.LostItems.Commands.CreateLostItem;
using Application.Interfaces;
using AutoMapper;
using Domain;
using Domain.Common.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Tests.Helpers;

namespace Tests.Application_Tests.Features.LostItems.Commands.CreateLostItem;

public class CreateLostItemImageUploadTests
{
    private const string TestUserId = "test-user-id";

    private static void SeedUser(Persistence.AppDbContext context)
    {
        if (!context.Users.Any(u => u.Id == TestUserId))
        {
            context.Users.Add(new ApplicationUser
            {
                Id = TestUserId,
                UserName = "testuser",
                Email = "test@test.com",
                EmailConfirmed = true,
                FirstName = "Test",
                LastName = "User"
            });
            context.SaveChanges();
        }
    }

    [Fact]
    public async Task Handle_WithImage_ShouldUploadAndSetImageFields()
    {
        var context = TestDbContextFactory.CreateInMemoryDbContext();
        SeedUser(context);

        var mapperMock = new Mock<IMapper>();
        var currentUserMock = new Mock<ICurrentUserService>();
        var imageStorageMock = new Mock<IImageStorageService>();

        currentUserMock.Setup(x => x.UserId).Returns(TestUserId);
        imageStorageMock
            .Setup(x => x.UploadImageAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ImageUploadResult
            {
                Url = "https://res.cloudinary.com/demo/image/upload/v1/test.jpg",
                PublicId = "unilostitem/test123"
            });

        var handler = new CreateLostItemCommandHandler(context, mapperMock.Object, currentUserMock.Object, imageStorageMock.Object);

        var imageStream = new MemoryStream(new byte[] { 1, 2, 3 });
        var dto = new CreateLostItemDto
        {
            Title = "Test",
            Description = "Test Desc",
            Category = ItemCategory.Electronics,
            ItemType = ItemType.Lost,
            IncidentDate = DateTime.UtcNow.AddDays(-1),
            LocationLabel = "Test Location",
            Latitude = 41.0,
            Longitude = 29.0,
            ContactInfo = "test@test.com",
            ImageStream = imageStream,
            ImageFileName = "photo.jpg"
        };

        var lostItem = new LostItem
        {
            Title = dto.Title,
            Description = dto.Description,
            Category = dto.Category,
            ItemType = dto.ItemType,
            IncidentDate = dto.IncidentDate,
            LocationLabel = dto.LocationLabel,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            ContactInfo = "",
            UserId = ""
        };

        mapperMock.Setup(m => m.Map<LostItem>(dto)).Returns(lostItem);

        var result = await handler.Handle(new CreateLostItemCommand { CreateLostItemDto = dto }, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        var savedItem = await context.LostItems.FirstOrDefaultAsync();
        savedItem.Should().NotBeNull();
        savedItem!.ImageUrl.Should().Be("https://res.cloudinary.com/demo/image/upload/v1/test.jpg");
        savedItem.ImagePublicId.Should().Be("unilostitem/test123");

        imageStorageMock.Verify(x => x.UploadImageAsync(imageStream, "photo.jpg", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithoutImage_ShouldNotCallImageService()
    {
        var context = TestDbContextFactory.CreateInMemoryDbContext();
        SeedUser(context);

        var mapperMock = new Mock<IMapper>();
        var currentUserMock = new Mock<ICurrentUserService>();
        var imageStorageMock = new Mock<IImageStorageService>();

        currentUserMock.Setup(x => x.UserId).Returns(TestUserId);

        var handler = new CreateLostItemCommandHandler(context, mapperMock.Object, currentUserMock.Object, imageStorageMock.Object);

        var dto = new CreateLostItemDto
        {
            Title = "No Image",
            Description = "No Image Desc",
            Category = ItemCategory.Other,
            ItemType = ItemType.Found,
            IncidentDate = DateTime.UtcNow,
            LocationLabel = "Test Location",
            Latitude = 41.0,
            Longitude = 29.0,
            ContactInfo = "test@test.com"
        };

        var lostItem = new LostItem
        {
            Title = dto.Title,
            Description = dto.Description,
            Category = dto.Category,
            ItemType = dto.ItemType,
            IncidentDate = dto.IncidentDate,
            LocationLabel = dto.LocationLabel,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            ContactInfo = "",
            UserId = ""
        };

        mapperMock.Setup(m => m.Map<LostItem>(dto)).Returns(lostItem);

        var result = await handler.Handle(new CreateLostItemCommand { CreateLostItemDto = dto }, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        var savedItem = await context.LostItems.FirstOrDefaultAsync();
        savedItem!.ImageUrl.Should().BeNull();
        savedItem.ImagePublicId.Should().BeNull();

        imageStorageMock.Verify(x => x.UploadImageAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
