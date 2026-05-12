using API.Controllers;
using API.Responses;
using Application.Core;
using Application.Features.LostItems.Commands.CreateLostItem;
using Application.Features.LostItems.Commands.UpdateLostItem;
using Domain.Common.Enums;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Tests.API_Tests.Controllers;

public class LostItemsControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly LostItemsController _controller;

    public LostItemsControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new LostItemsController
        {
            ControllerContext = new ControllerContext()
        };

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(x => x.GetService(typeof(IMediator))).Returns(_mediatorMock.Object);

        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.Setup(x => x.RequestServices).Returns(serviceProviderMock.Object);
        _controller.ControllerContext.HttpContext = httpContextMock.Object;
    }

    [Fact]
    public async Task CreateItem_WithImage_ShouldSetStreamAndFileName()
    {
        // Arrange
        var dto = new CreateLostItemDto
        {
            Title = "Test",
            Description = "Desc",
            Category = ItemCategory.Other,
            ItemType = ItemType.Lost,
            IncidentDate = DateTime.UtcNow,
            ContactInfo = "test@test.com",
            LocationLabel = "Loc"
        };
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(x => x.OpenReadStream()).Returns(new MemoryStream());
        fileMock.Setup(x => x.FileName).Returns("test.jpg");

        _mediatorMock.Setup(x => x.Send(It.IsAny<CreateLostItemCommand>(), default))
            .ReturnsAsync(Result<string>.Success("Created", "id-123"));

        // Act
        await _controller.CreateItem(dto, fileMock.Object);

        // Assert
        _mediatorMock.Verify(m => m.Send(It.Is<CreateLostItemCommand>(c =>
            c.CreateLostItemDto.ImageStream != null &&
            c.CreateLostItemDto.ImageFileName == "test.jpg"), default), Times.Once);
    }

    [Fact]
    public async Task UpdateItem_WithImage_ShouldSetStreamAndFileName()
    {
        // Arrange
        var id = "item-1";
        var dto = new UpdateLostItemDto
        {
            Title = "Updated",
            Description = "Updated Desc",
            Category = ItemCategory.Other,
            IncidentDate = DateTime.UtcNow,
            ContactInfo = "test@test.com",
            LocationLabel = "Loc"
        };
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(x => x.OpenReadStream()).Returns(new MemoryStream());
        fileMock.Setup(x => x.FileName).Returns("updated.png");

        _mediatorMock.Setup(x => x.Send(It.IsAny<UpdateLostItemCommand>(), default))
            .ReturnsAsync(Result<Unit>.Success("Updated", Unit.Value));

        // Act
        await _controller.UpdateItem(id, dto, fileMock.Object);

        // Assert
        _mediatorMock.Verify(m => m.Send(It.Is<UpdateLostItemCommand>(c =>
            c.Id == id &&
            c.UpdateLostItemDto.ImageStream != null &&
            c.UpdateLostItemDto.ImageFileName == "updated.png"), default), Times.Once);
    }
}
