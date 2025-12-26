using System.Threading;
using System.Threading.Tasks;
using Application.Core;
using Application.Features.SerhanKitaplar.Commands.DeleteSerhanKitap;
using Domain;
using Moq;
using Persistence;
using Xunit;
using MediatR;

namespace Tests.Features.SerhanKitaplar.Commands.DeleteSerhanKitap
{
    public class DeleteSerhanKitapCommandHandlerTests
    {
        private readonly Mock<IAppDbContext> _mockContext;
        private readonly DeleteSerhanKitapCommandHandler _handler;

        public DeleteSerhanKitapCommandHandlerTests()
        {
            _mockContext = new Mock<IAppDbContext>();
            _handler = new DeleteSerhanKitapCommandHandler(_mockContext.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnNotFound_WhenSerhanKitapDoesNotExist()
        {
            // Arrange
            _mockContext.Setup(x => x.SerhanKitaplar.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((SerhanKitap?)null!);

            var command = new DeleteSerhanKitapCommand { Id = "1" };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.Code);
            Assert.Equal("SerhanKitap not found", result.Message);
        }

        [Fact]
        public async Task Handle_ShouldReturnSuccess_WhenSerhanKitapIsDeleted()
        {
            // Arrange
            var kitap = new SerhanKitap { Id = "1", KitapName = "Test", KitapYazar = "Yazar", KitapSayfaSayisi = 100 };
            _mockContext.Setup(x => x.SerhanKitaplar.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(kitap);
            _mockContext.Setup(x => x.SerhanKitaplar.Remove(It.IsAny<SerhanKitap>()));
            _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var command = new DeleteSerhanKitapCommand { Id = "1" };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("Serhan kitap deleted successfully", result.Message);
        }

        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenSaveChangesFails()
        {
            // Arrange
            var kitap = new SerhanKitap { Id = "1", KitapName = "Test", KitapYazar = "Yazar", KitapSayfaSayisi = 100 };
            _mockContext.Setup(x => x.SerhanKitaplar.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(kitap);
            _mockContext.Setup(x => x.SerhanKitaplar.Remove(It.IsAny<SerhanKitap>()));
            _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(0);

            var command = new DeleteSerhanKitapCommand { Id = "1" };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.Code);
            Assert.Equal("Failed to delete the serhan kitap", result.Message);
        }
    }
}
