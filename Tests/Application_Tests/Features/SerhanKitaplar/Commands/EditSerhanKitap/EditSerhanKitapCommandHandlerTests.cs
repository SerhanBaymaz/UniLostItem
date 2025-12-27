using System.Threading;
using System.Threading.Tasks;
using Application.Core;
using Application.Features.SerhanKitaplar.Commands.EditSerhanKitap;
using Domain;
using Moq;
using Persistence;
using Xunit;
using MediatR;

namespace Tests.Application_Tests.Features.SerhanKitaplar.Commands.EditSerhanKitap;

public class EditSerhanKitapCommandHandlerTests
{
    private readonly Mock<IAppDbContext> _mockContext;
    private readonly EditSerhanKitapCommandHandler _handler;

    public EditSerhanKitapCommandHandlerTests()
    {
        _mockContext = new Mock<IAppDbContext>();
        _handler = new EditSerhanKitapCommandHandler(_mockContext.Object);
    }

        [Fact]
        public async Task Handle_ShouldReturnNotFound_WhenSerhanKitapDoesNotExist()
        {
            // Arrange
            _mockContext.Setup(x => x.SerhanKitaplar.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((SerhanKitap?)null!);

            var command = new EditSerhanKitapCommand
            {
                Id = "1",
                EditSerhanKitapDto = new EditSerhanKitapDto
                {
                    KitapName = "Test",
                    KitapYazar = "Yazar",
                    KitapSayfaSayisi = 100
                }
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.Code);
            Assert.Equal("SerhanKitap not found", result.Message);
        }

        [Fact]
        public async Task Handle_ShouldReturnSuccess_WhenSerhanKitapIsUpdated()
        {
            // Arrange
            var kitap = new SerhanKitap { Id = "1", KitapName = "OldName", KitapYazar = "OldYazar", KitapSayfaSayisi = 50 };
            _mockContext.Setup(x => x.SerhanKitaplar.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(kitap);
            _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var command = new EditSerhanKitapCommand
            {
                Id = "1",
                EditSerhanKitapDto = new EditSerhanKitapDto
                {
                    KitapName = "NewName",
                    KitapYazar = "NewYazar",
                    KitapSayfaSayisi = 100
                }
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("SerhanKitap updated successfully.", result.Message);
            Assert.Equal("NewName", kitap.KitapName);
            Assert.Equal("NewYazar", kitap.KitapYazar);
            Assert.Equal(100, kitap.KitapSayfaSayisi);
        }

        [Fact]
        public async Task Handle_ShouldReturnNoChange_WhenNoChangesMade()
        {
            // Arrange
            var kitap = new SerhanKitap { Id = "1", KitapName = "SameName", KitapYazar = "SameYazar", KitapSayfaSayisi = 100 };
            _mockContext.Setup(x => x.SerhanKitaplar.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(kitap);
            _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(0);

            var command = new EditSerhanKitapCommand
            {
                Id = "1",
                EditSerhanKitapDto = new EditSerhanKitapDto
                {
                    KitapName = "SameName",
                    KitapYazar = "SameYazar",
                    KitapSayfaSayisi = 100
                }
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal($"No changes were made to the {kitap.KitapName}.", result.Message);
        }
    }
