using Application.Core;
using Application.Features.LostItems.Commands.CreateLostItem;
using Application.Interfaces;
using AutoMapper;
using Domain;
using Domain.Common.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Persistence;
using Tests.Helpers;

namespace Tests.Application_Tests.Features.LostItems.Commands.CreateLostItem;

public class CreateLostItemCommandHandlerTests
{
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IImageStorageService> _imageStorageServiceMock;
    private readonly AppDbContext _context;
    private readonly CreateLostItemCommandHandler _handler;

    private const string TestUserId = "test-user-id";

    public CreateLostItemCommandHandlerTests()
    {
        _context = TestDbContextFactory.CreateInMemoryDbContext();
        _mapperMock = new Mock<IMapper>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _imageStorageServiceMock = new Mock<IImageStorageService>();
        _currentUserServiceMock.Setup(x => x.UserId).Returns(TestUserId);
        _handler = new CreateLostItemCommandHandler(_context, _mapperMock.Object, _currentUserServiceMock.Object, _imageStorageServiceMock.Object);
    }

    private void SeedUser()
    {
        if (!_context.Users.Any(u => u.Id == TestUserId))
        {
            _context.Users.Add(new ApplicationUser
            {
                Id = TestUserId,
                UserName = "testuser",
                Email = "test@test.com",
                EmailConfirmed = true,
                FirstName = "Test",
                LastName = "User"
            });
            _context.SaveChanges();
        }
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateLostItem()
    {
        SeedUser();
        var dto = new CreateLostItemDto
        {
            Title = "iPhone 15",
            Description = "Siyah renk, kılıfı yok",
            Category = ItemCategory.Electronics,
            ItemType = ItemType.Lost,
            IncidentDate = DateTime.UtcNow.AddDays(-1),
            LocationLabel = "Kütüphane B Blok",
            Latitude = 41.0082,
            Longitude = 28.9784,
            ContactInfo = "test@test.com"
        };

        var command = new CreateLostItemCommand { CreateLostItemDto = dto };

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

        _mapperMock.Setup(m => m.Map<LostItem>(dto)).Returns(lostItem);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Be("Kayıt başarıyla oluşturuldu");
        result.Value.Should().NotBeNullOrEmpty();

        var savedItem = await _context.LostItems.FirstOrDefaultAsync();
        savedItem.Should().NotBeNull();
        savedItem!.Title.Should().Be("iPhone 15");
        savedItem.UserId.Should().Be(TestUserId);
        savedItem.CreatedBy.Should().Be(TestUserId);
    }

    [Fact]
    public async Task Handle_ShouldSetUserId_FromCurrentUserService()
    {
        SeedUser();
        var dto = new CreateLostItemDto
        {
            Title = "Test Item",
            Description = "Test Description",
            Category = ItemCategory.Other,
            ItemType = ItemType.Found,
            IncidentDate = DateTime.UtcNow,
            LocationLabel = "Test Location",
            Latitude = 40.0,
            Longitude = 30.0,
            ContactInfo = "contact@test.com"
        };

        var command = new CreateLostItemCommand { CreateLostItemDto = dto };

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

        _mapperMock.Setup(m => m.Map<LostItem>(dto)).Returns(lostItem);

        await _handler.Handle(command, CancellationToken.None);

        var savedItem = await _context.LostItems.FirstOrDefaultAsync();
        savedItem.Should().NotBeNull();
        savedItem!.UserId.Should().Be(TestUserId);
    }

    [Fact]
    public async Task Handle_ShouldCallMapperWithCorrectDto()
    {
        SeedUser();
        var dto = new CreateLostItemDto
        {
            Title = "Mapper Test",
            Description = "Mapper Description",
            Category = ItemCategory.BagWallet,
            ItemType = ItemType.Lost,
            IncidentDate = DateTime.UtcNow,
            LocationLabel = "Mapper Location",
            Latitude = 41.0,
            Longitude = 29.0,
            ContactInfo = "mapper@test.com"
        };

        var command = new CreateLostItemCommand { CreateLostItemDto = dto };

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

        _mapperMock.Setup(m => m.Map<LostItem>(dto)).Returns(lostItem);

        await _handler.Handle(command, CancellationToken.None);

        _mapperMock.Verify(m => m.Map<LostItem>(dto), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenSaveFails()
    {
        SeedUser();
        var mockContext = new Mock<IAppDbContext>();
        var mockDbSet = new Mock<DbSet<LostItem>>();
        var mockUserService = new Mock<ICurrentUserService>();

        mockContext.Setup(c => c.LostItems).Returns(mockDbSet.Object);
        mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(0);
        mockUserService.Setup(x => x.UserId).Returns(TestUserId);

        var handler = new CreateLostItemCommandHandler(mockContext.Object, _mapperMock.Object, mockUserService.Object, _imageStorageServiceMock.Object);

        var dto = new CreateLostItemDto
        {
            Title = "Fail Test",
            Description = "Fail Description",
            Category = ItemCategory.Electronics,
            ItemType = ItemType.Lost,
            IncidentDate = DateTime.UtcNow,
            LocationLabel = "Fail Location",
            Latitude = 41.0,
            Longitude = 29.0,
            ContactInfo = "fail@test.com"
        };

        var command = new CreateLostItemCommand { CreateLostItemDto = dto };

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

        _mapperMock.Setup(m => m.Map<LostItem>(dto)).Returns(lostItem);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Kayıt oluşturulamadı");
        result.Code.Should().Be(400);
    }
}
