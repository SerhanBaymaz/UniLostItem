using Application.Features.Images.Commands.UploadImage;
using Application.Interfaces;
using FluentAssertions;
using Moq;

namespace Tests.Application_Tests.Features.Images.Commands.UploadImage;

public class UploadImageCommandHandlerTests
{
    private readonly Mock<IImageStorageService> _imageStorageServiceMock;
    private readonly UploadImageCommandHandler _handler;

    public UploadImageCommandHandlerTests()
    {
        _imageStorageServiceMock = new Mock<IImageStorageService>();
        _handler = new UploadImageCommandHandler(_imageStorageServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCallImageStorageService_AndReturnResult()
    {
        var expectedResult = new ImageUploadResult
        {
            Url = "https://res.cloudinary.com/demo/image/upload/v1/test.jpg",
            PublicId = "unilostitem/test123"
        };

        _imageStorageServiceMock
            .Setup(x => x.UploadImageAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var stream = new MemoryStream(new byte[] { 1, 2, 3 });
        var command = new UploadImageCommand
        {
            ImageStream = stream,
            FileName = "photo.jpg"
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Url.Should().Be(expectedResult.Url);
        result.PublicId.Should().Be(expectedResult.PublicId);

        _imageStorageServiceMock.Verify(x => x.UploadImageAsync(stream, "photo.jpg", It.IsAny<CancellationToken>()), Times.Once);
    }
}
