using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Core;
using Application.Features.SerhanKitaplar.Commands.DeleteSerhanKitap;
using Application.Interfaces;
using Domain;
using FluentAssertions;
using MediatR;
using Moq;
using Persistence;
using Xunit;

namespace Tests.Application_Tests.Features.SerhanKitaplar.Commands.DeleteSerhanKitap;

public class DeleteSerhanKitapCommandHandlerTests
{
    private readonly Mock<IAppDbContext> _mockContext;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly DeleteSerhanKitapCommandHandler _handler;

    public DeleteSerhanKitapCommandHandlerTests()
    {
        _mockContext = new Mock<IAppDbContext>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _currentUserServiceMock.Setup(x => x.UserId).Returns((string?)null); // Simulate anonymous user
        _handler = new DeleteSerhanKitapCommandHandler(_mockContext.Object, _currentUserServiceMock.Object);
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
            _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var command = new DeleteSerhanKitapCommand { Id = "1" };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Message.Should().Be("Serhan kitap deleted successfully");
            // Verify soft delete behavior
            kitap.IsDeleted.Should().BeTrue();
            kitap.UpdatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public async Task Handle_ShouldReturnNotFound_WhenSerhanKitapIsAlreadySoftDeleted()
        {
            // Arrange
            var kitap = new SerhanKitap { Id = "1", KitapName = "Test", KitapYazar = "Yazar", KitapSayfaSayisi = 100, IsDeleted = true };
            _mockContext.Setup(x => x.SerhanKitaplar.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(kitap);

            var command = new DeleteSerhanKitapCommand { Id = "1" };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Code.Should().Be(404);
            result.Message.Should().Be("SerhanKitap not found");
            kitap.IsDeleted.Should().BeTrue(); // Should remain deleted
        }

        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenSaveChangesFails()
        {
            // Arrange
            var kitap = new SerhanKitap { Id = "1", KitapName = "Test", KitapYazar = "Yazar", KitapSayfaSayisi = 100 };
            _mockContext.Setup(x => x.SerhanKitaplar.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(kitap);
            _mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(0);

            var command = new DeleteSerhanKitapCommand { Id = "1" };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Code.Should().Be(400);
            result.Message.Should().Be("Failed to delete the serhan kitap");
        }
    }
