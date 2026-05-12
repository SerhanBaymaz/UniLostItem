using Application.Features.Images.Commands.DeleteImage;
using Application.Interfaces;
using FluentAssertions;
using Moq;

namespace Tests.Application_Tests.Features.Images.Commands.DeleteImage;

public class DeleteImageCommandHandlerTests
{
    private readonly Mock<IImageStorageService> _imageStorageServiceMock;
    private readonly DeleteImageCommandHandler _handler;

    public DeleteImageCommandHandlerTests()
    {
        _imageStorageServiceMock = new Mock<IImageStorageService>();
        _handler = new DeleteImageCommandHandler(_imageStorageServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenDeletionIsSuccessful()
    {
        _imageStorageServiceMock
            .Setup(x => x.DeleteImageAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var command = new DeleteImageCommand { PublicId = "unilostitem/test123" };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Be("Görsel başarıyla silindi");

        _imageStorageServiceMock.Verify(x => x.DeleteImageAsync("unilostitem/test123", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenDeletionFails()
    {
        _imageStorageServiceMock
            .Setup(x => x.DeleteImageAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var command = new DeleteImageCommand { PublicId = "unilostitem/test123" };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Code.Should().Be(400);
        result.Message.Should().Be("Görsel silinemedi veya bulunamadı");
    }
}
