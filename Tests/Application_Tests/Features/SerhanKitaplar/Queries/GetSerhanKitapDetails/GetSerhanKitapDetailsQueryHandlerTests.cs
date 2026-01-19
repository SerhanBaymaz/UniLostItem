using System.Threading;
using System.Threading.Tasks;
using Application.Core;
using Application.Features.SerhanKitaplar.Queries.Common.DTOs;
using Application.Features.SerhanKitaplar.Queries.GetSerhanKitapDetails;
using AutoMapper;
using Domain;
using FluentAssertions;
using Moq;
using Persistence;
using Xunit;

namespace Tests.Application_Tests.Features.SerhanKitaplar.Queries.GetSerhanKitapDetails;

public class GetSerhanKitapDetailsQueryHandlerTests
{
    private readonly Mock<IAppDbContext> _mockContext;
    private readonly Mock<IMapper> _mockMapper;
    private readonly GetSerhanKitapDetailsQueryHandler _handler;

    public GetSerhanKitapDetailsQueryHandlerTests()
    {
        _mockContext = new Mock<IAppDbContext>();
        _mockMapper = new Mock<IMapper>();
        _handler = new GetSerhanKitapDetailsQueryHandler(_mockContext.Object, _mockMapper.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenSerhanKitapDoesNotExist()
    {
        // Arrange
        _mockContext.Setup(x => x.SerhanKitaplar.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((SerhanKitap?)null!);

        var query = new GetSerhanKitapDetailsQuery { Id = "1" };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Code.Should().Be(404);
        result.Message.Should().Be("SerhanKitap not found");
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenSerhanKitapExists()
    {
        // Arrange
        var kitap = new SerhanKitap { Id = "1", KitapName = "A", KitapYazar = "Y1", KitapSayfaSayisi = 100, IsDeleted = false, IsActive = true };
        var kitapDto = new GetSerhanKitapDto { Id = "1", KitapName = "A", KitapYazar = "Y1", KitapSayfaSayisi = 100 };
        _mockContext.Setup(x => x.SerhanKitaplar.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(kitap);
        _mockMapper.Setup(x => x.Map<GetSerhanKitapDto>(kitap)).Returns(kitapDto);

        var query = new GetSerhanKitapDetailsQuery { Id = "1" };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Be("SerhanKitap retrieved successfully");
        result.Value.Should().NotBeNull();
        result.Value!.KitapName.Should().Be("A");
    }

    [Fact]
    public async Task Handle_ShouldReturnGone_WhenSerhanKitapIsSoftDeleted()
    {
        // Arrange
        var kitap = new SerhanKitap { Id = "1", KitapName = "Deleted Book", KitapYazar = "Author", KitapSayfaSayisi = 200, IsDeleted = true, IsActive = true };
        _mockContext.Setup(x => x.SerhanKitaplar.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(kitap);

        var query = new GetSerhanKitapDetailsQuery { Id = "1" };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Code.Should().Be(410);
        result.Message.Should().Be("SerhanKitap has been deleted");
    }

    [Fact]
    public async Task Handle_ShouldReturnLocked_WhenSerhanKitapIsInactive()
    {
        // Arrange
        var kitap = new SerhanKitap { Id = "1", KitapName = "Inactive Book", KitapYazar = "Author", KitapSayfaSayisi = 200, IsDeleted = false, IsActive = false };
        _mockContext.Setup(x => x.SerhanKitaplar.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(kitap);

        var query = new GetSerhanKitapDetailsQuery { Id = "1" };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Code.Should().Be(423);
        result.Message.Should().Be("SerhanKitap is not active");
    }

    [Fact]
    public async Task Handle_ShouldReturnDifferentStatusCodes_ForDifferentFailureStates()
    {
        // Arrange - Test 404 for non-existent
        _mockContext.Setup(x => x.SerhanKitaplar.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((SerhanKitap?)null!);
        var notFoundQuery = new GetSerhanKitapDetailsQuery { Id = "nonexistent" };
        var notFoundResult = await _handler.Handle(notFoundQuery, CancellationToken.None);

        // Arrange - Test 410 for soft-deleted
        var deletedKitap = new SerhanKitap { Id = "2", KitapName = "Deleted", KitapYazar = "Author", KitapSayfaSayisi = 100, IsDeleted = true, IsActive = true };
        _mockContext.Setup(x => x.SerhanKitaplar.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(deletedKitap);
        var deletedQuery = new GetSerhanKitapDetailsQuery { Id = "2" };
        var deletedResult = await _handler.Handle(deletedQuery, CancellationToken.None);

        // Arrange - Test 423 for inactive
        var inactiveKitap = new SerhanKitap { Id = "3", KitapName = "Inactive", KitapYazar = "Author", KitapSayfaSayisi = 100, IsDeleted = false, IsActive = false };
        _mockContext.Setup(x => x.SerhanKitaplar.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(inactiveKitap);
        var inactiveQuery = new GetSerhanKitapDetailsQuery { Id = "3" };
        var inactiveResult = await _handler.Handle(inactiveQuery, CancellationToken.None);

        // Assert
        notFoundResult.Code.Should().Be(404);
        deletedResult.Code.Should().Be(410);
        inactiveResult.Code.Should().Be(423);

        // Verify all status codes are different
        notFoundResult.Code.Should().NotBe(deletedResult.Code);
        notFoundResult.Code.Should().NotBe(inactiveResult.Code);
        deletedResult.Code.Should().NotBe(inactiveResult.Code);
    }
}
