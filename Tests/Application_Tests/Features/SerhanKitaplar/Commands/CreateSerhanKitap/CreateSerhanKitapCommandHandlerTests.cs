using Application.Core;
using Application.Features.SerhanKitaplar.Commands.CreateSerhanKitap;
using AutoMapper;
using Domain;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Persistence;
using Tests.Helpers;

namespace Tests.Application_Tests.Features.SerhanKitaplar.Commands.CreateSerhanKitap;

public class CreateSerhanKitapCommandHandlerTests
{
    private readonly Mock<IMapper> _mapperMock;
    private readonly IAppDbContext _context;
    private readonly CreateSerhanKitapCommandHandler _handler;

    public CreateSerhanKitapCommandHandlerTests()
    {
        _context = TestDbContextFactory.CreateInMemoryDbContext();
        _mapperMock = new Mock<IMapper>();
        _handler = new CreateSerhanKitapCommandHandler(_context, _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateSerhanKitap()
    {
        // Arrange
        var dto = new CreateSerhanKitapDto
        {
            KitapName = "Test Kitap",
            KitapYazar = "Test Yazar",
            KitapSayfaSayisi = 100
        };

        var command = new CreateSerhanKitapCommand
        {
            CreateSerhanKitapDto = dto
        };

        var serhanKitap = new SerhanKitap
        {
            KitapName = dto.KitapName,
            KitapYazar = dto.KitapYazar,
            KitapSayfaSayisi = dto.KitapSayfaSayisi
        };

        _mapperMock.Setup(m => m.Map<SerhanKitap>(dto))
            .Returns(serhanKitap);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Be("Serhan kitap created successfully");
        result.Value.Should().NotBeNullOrEmpty();

        var savedKitap = await _context.SerhanKitaplar.FirstOrDefaultAsync();
        savedKitap.Should().NotBeNull();
        savedKitap!.KitapName.Should().Be("Test Kitap");
        savedKitap.KitapYazar.Should().Be("Test Yazar");
        savedKitap.KitapSayfaSayisi.Should().Be(100);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnSuccessResult()
    {
        // Arrange
        var dto = new CreateSerhanKitapDto
        {
            KitapName = "Başarılı Kitap",
            KitapYazar = "Başarılı Yazar",
            KitapSayfaSayisi = 250
        };

        var command = new CreateSerhanKitapCommand
        {
            CreateSerhanKitapDto = dto
        };

        var serhanKitap = new SerhanKitap
        {
            KitapName = dto.KitapName,
            KitapYazar = dto.KitapYazar,
            KitapSayfaSayisi = dto.KitapSayfaSayisi
        };

        _mapperMock.Setup(m => m.Map<SerhanKitap>(dto))
            .Returns(serhanKitap);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(serhanKitap.Id);
    }

    [Fact]
    public async Task Handle_ShouldCallMapperWithCorrectDto()
    {
        // Arrange
        var dto = new CreateSerhanKitapDto
        {
            KitapName = "Mapper Test",
            KitapYazar = "Mapper Yazar",
            KitapSayfaSayisi = 300
        };

        var command = new CreateSerhanKitapCommand
        {
            CreateSerhanKitapDto = dto
        };

        var serhanKitap = new SerhanKitap
        {
            KitapName = dto.KitapName,
            KitapYazar = dto.KitapYazar,
            KitapSayfaSayisi = dto.KitapSayfaSayisi
        };

        _mapperMock.Setup(m => m.Map<SerhanKitap>(dto))
            .Returns(serhanKitap);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mapperMock.Verify(m => m.Map<SerhanKitap>(dto), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldAddKitapToContext()
    {
        // Arrange
        var dto = new CreateSerhanKitapDto
        {
            KitapName = "Context Test",
            KitapYazar = "Context Yazar",
            KitapSayfaSayisi = 150
        };

        var command = new CreateSerhanKitapCommand
        {
            CreateSerhanKitapDto = dto
        };

        var serhanKitap = new SerhanKitap
        {
            KitapName = dto.KitapName,
            KitapYazar = dto.KitapYazar,
            KitapSayfaSayisi = dto.KitapSayfaSayisi
        };

        _mapperMock.Setup(m => m.Map<SerhanKitap>(dto))
            .Returns(serhanKitap);

        var initialCount = await _context.SerhanKitaplar.CountAsync();

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var finalCount = await _context.SerhanKitaplar.CountAsync();
        finalCount.Should().Be(initialCount + 1);
    }

    [Fact]
    public async Task Handle_WithMultipleBooks_ShouldCreateAll()
    {
        // Arrange & Act
        for (int i = 1; i <= 3; i++)
        {
            var dto = new CreateSerhanKitapDto
            {
                KitapName = $"Kitap {i}",
                KitapYazar = $"Yazar {i}",
                KitapSayfaSayisi = i * 100
            };

            var command = new CreateSerhanKitapCommand
            {
                CreateSerhanKitapDto = dto
            };

            var serhanKitap = new SerhanKitap
            {
                KitapName = dto.KitapName,
                KitapYazar = dto.KitapYazar,
                KitapSayfaSayisi = dto.KitapSayfaSayisi
            };

            _mapperMock.Setup(m => m.Map<SerhanKitap>(dto))
                .Returns(serhanKitap);

            await _handler.Handle(command, CancellationToken.None);
        }

        // Assert
        var count = await _context.SerhanKitaplar.CountAsync();
        count.Should().Be(3);
    }

    [Fact]
    public async Task Handle_ShouldGenerateUniqueId()
    {
        // Arrange
        var dto1 = new CreateSerhanKitapDto
        {
            KitapName = "Kitap 1",
            KitapYazar = "Yazar 1",
            KitapSayfaSayisi = 100
        };

        var dto2 = new CreateSerhanKitapDto
        {
            KitapName = "Kitap 2",
            KitapYazar = "Yazar 2",
            KitapSayfaSayisi = 200
        };

        var serhanKitap1 = new SerhanKitap
        {
            KitapName = dto1.KitapName,
            KitapYazar = dto1.KitapYazar,
            KitapSayfaSayisi = dto1.KitapSayfaSayisi
        };

        var serhanKitap2 = new SerhanKitap
        {
            KitapName = dto2.KitapName,
            KitapYazar = dto2.KitapYazar,
            KitapSayfaSayisi = dto2.KitapSayfaSayisi
        };

        _mapperMock.Setup(m => m.Map<SerhanKitap>(dto1))
            .Returns(serhanKitap1);
        _mapperMock.Setup(m => m.Map<SerhanKitap>(dto2))
            .Returns(serhanKitap2);

        var command1 = new CreateSerhanKitapCommand { CreateSerhanKitapDto = dto1 };
        var command2 = new CreateSerhanKitapCommand { CreateSerhanKitapDto = dto2 };

        // Act
        var result1 = await _handler.Handle(command1, CancellationToken.None);
        var result2 = await _handler.Handle(command2, CancellationToken.None);

        // Assert
        result1.Value.Should().NotBe(result2.Value);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailureResult_WhenSaveFails()
    {
        // Arrange
        var mockContext = new Mock<IAppDbContext>();
        var mockDbSet = new Mock<DbSet<SerhanKitap>>();

        mockContext.Setup(c => c.SerhanKitaplar).Returns(mockDbSet.Object);
        mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(0); // Simulate save failure

        var handler = new CreateSerhanKitapCommandHandler(mockContext.Object, _mapperMock.Object);

        var dto = new CreateSerhanKitapDto
        {
            KitapName = "Fail Test",
            KitapYazar = "Fail Yazar",
            KitapSayfaSayisi = 200
        };

        var command = new CreateSerhanKitapCommand
        {
            CreateSerhanKitapDto = dto
        };

        var serhanKitap = new SerhanKitap
        {
            KitapName = dto.KitapName,
            KitapYazar = dto.KitapYazar,
            KitapSayfaSayisi = dto.KitapSayfaSayisi
        };

        _mapperMock.Setup(m => m.Map<SerhanKitap>(dto))
            .Returns(serhanKitap);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Failed to create the serhan kitap");
        result.Code.Should().Be(400);
    }

}
