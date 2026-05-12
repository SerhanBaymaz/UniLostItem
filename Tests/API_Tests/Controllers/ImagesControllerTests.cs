using API.Controllers;
using API.Responses;
using Application.Features.Images.Commands.DeleteImage;
using Application.Features.Images.Commands.UploadImage;
using Application.Interfaces;
using Application.Core;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Tests.API_Tests.Controllers;

public class ImagesControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly ImagesController _controller;

    public ImagesControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new ImagesController
        {
            ControllerContext = new ControllerContext()
        };

        // Setup BaseApiController.Mediator (it uses HttpContext.RequestServices.GetService<IMediator>())
        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(x => x.GetService(typeof(IMediator))).Returns(_mediatorMock.Object);

        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.Setup(x => x.RequestServices).Returns(serviceProviderMock.Object);
        _controller.ControllerContext.HttpContext = httpContextMock.Object;
    }

    [Fact]
    public async Task UploadImage_WithValidFile_ShouldReturnOk()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(x => x.Length).Returns(100);
        fileMock.Setup(x => x.FileName).Returns("test.jpg");
        fileMock.Setup(x => x.OpenReadStream()).Returns(new MemoryStream());

        var uploadResult = new ImageUploadResult { Url = "http://test.com", PublicId = "id" };
        _mediatorMock.Setup(x => x.Send(It.IsAny<UploadImageCommand>(), default)).ReturnsAsync(uploadResult);

        // Act
        var result = await _controller.UploadImage(fileMock.Object);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var apiResponse = okResult.Value.Should().BeOfType<StandardApiResponse<ImageUploadResult>>().Subject;
        apiResponse.Success.Should().BeTrue();
        apiResponse.Data.Should().BeEquivalentTo(uploadResult);
    }

    [Fact]
    public async Task UploadImage_WithNullFile_ShouldReturnBadRequest()
    {
        // Act
        var result = await _controller.UploadImage(null!);

        // Assert
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var apiResponse = badRequestResult.Value.Should().BeOfType<StandardApiResponse<ImageUploadResult>>().Subject;
        apiResponse.Success.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteImage_ShouldReturnHandleResult()
    {
        // Arrange
        var publicId = "test/id";
        _mediatorMock.Setup(x => x.Send(It.IsAny<DeleteImageCommand>(), default))
            .ReturnsAsync(Result<Unit>.Success("Deleted", Unit.Value));

        // Act
        var result = await _controller.DeleteImage(publicId);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var apiResponse = okResult.Value.Should().BeOfType<StandardApiResponse<Unit>>().Subject;
        apiResponse.Success.Should().BeTrue();
    }
}
