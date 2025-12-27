using System.Threading;
using System.Threading.Tasks;
using Application.Core;
using Application.Features.SerhanKitaplar.Queries.Common.DTOs;
using Application.Features.SerhanKitaplar.Queries.GetSerhanKitapDetails;
using AutoMapper;
using Domain;
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
        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.Code);
        Assert.Equal("SerhanKitap not found", result.Message);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenSerhanKitapExists()
    {
        // Arrange
        var kitap = new SerhanKitap { Id = "1", KitapName = "A", KitapYazar = "Y1", KitapSayfaSayisi = 100 };
        var kitapDto = new GetSerhanKitapDto { Id = "1", KitapName = "A", KitapYazar = "Y1", KitapSayfaSayisi = 100 };
        _mockContext.Setup(x => x.SerhanKitaplar.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(kitap);
        _mockMapper.Setup(x => x.Map<GetSerhanKitapDto>(kitap)).Returns(kitapDto);

        var query = new GetSerhanKitapDetailsQuery { Id = "1" };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("SerhanKitap retrieved successfully", result.Message);
        Assert.NotNull(result.Value);
        Assert.Equal("A", result.Value.KitapName);
    }
}
