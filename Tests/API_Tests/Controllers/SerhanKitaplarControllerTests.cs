using API.Controllers;
using API.Controllers.Requests;
using API.Responses;
using Application.Core;
using Application.Core.Extensions;
using Application.Core.Pagination;
using Application.Features.SerhanKitaplar.Commands.CreateSerhanKitap;
using Application.Features.SerhanKitaplar.Commands.DeleteSerhanKitap;
using Application.Features.SerhanKitaplar.Commands.EditSerhanKitap;
using Application.Features.SerhanKitaplar.Queries.Common.Enums;
using Application.Features.SerhanKitaplar.Queries.Common.DTOs;
using Application.Features.SerhanKitaplar.Queries.GetSerhanKitapDetails;
using Application.Features.SerhanKitaplar.Queries.GetSerhanKitapList;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Tests.API_Tests.Controllers;

public class SerhanKitaplarControllerTests
{
    private const string SuccessMessage = "Success";
    private const string BookNotFoundMessage = "Book not found";
    private const string DefaultAuthor = "Author";
    private readonly SerhanKitaplarController _controller;
    private readonly Mock<IMediator> _mediatorMock;

    public SerhanKitaplarControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new SerhanKitaplarController();

        // Setup HttpContext
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = "/api/v1/serhan-kitaplar";
        httpContext.TraceIdentifier = "test-trace-id";

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(sp => sp.GetService(typeof(IMediator)))
            .Returns(_mediatorMock.Object);

        httpContext.RequestServices = serviceProviderMock.Object;

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
    }

    #region GetSerhanKitaplar Tests (Paginated)

    [Fact]
    public async Task GetSerhanKitaplarPaginated_ShouldReturnOk_WithPaginatedResult()
    {
        // Arrange
        var books = new List<GetSerhanKitapDto>
        {
            new() { Id = "1", KitapName = "1984", KitapYazar = "George Orwell", KitapSayfaSayisi = 328 },
            new() { Id = "2", KitapName = "Cesur Yeni Dünya", KitapYazar = "Aldous Huxley", KitapSayfaSayisi = 268 }
        };
        var paginatedList = new PaginatedListDto<GetSerhanKitapDto>
        {
            Items = books,
            TotalCount = 5,
            PageNumber = 1,
            PageSize = 2
        };
        var result = Result<PaginatedListDto<GetSerhanKitapDto>>.Success("Books retrieved successfully", paginatedList);

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetSerhanKitapListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var actionResult = await _controller.GetSerhanKitaplar(new GetSerhanKitaplarRequest());

        // Assert
        actionResult.Should().NotBeNull();
        actionResult.Result.Should().BeOfType<OkObjectResult>();

        var okResult = actionResult.Result as OkObjectResult;
        var response = okResult!.Value as StandardApiResponse<object>;

        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.Data.Should().NotBeNull();
        response.Metadata.Should().NotBeNull();

        // Data should be the list of items
        var items = response.Data as List<GetSerhanKitapDto>;
        items.Should().NotBeNull();
        items.Should().HaveCount(2);

        // Metadata should contain pagination info
        response.Metadata!.TryGetValue("totalCount", out var totalCount).Should().BeTrue();
        response.Metadata.TryGetValue("pageNumber", out var pageNumber).Should().BeTrue();
        response.Metadata.TryGetValue("pageSize", out var pageSize).Should().BeTrue();
        response.Metadata.TryGetValue("totalPages", out var totalPages).Should().BeTrue();
        response.Metadata.TryGetValue("hasNext", out var hasNext).Should().BeTrue();
        response.Metadata.TryGetValue("hasPrevious", out var hasPrevious).Should().BeTrue();

        totalCount.Should().Be(5);
        pageNumber.Should().Be(1);
        pageSize.Should().Be(2);
        totalPages.Should().Be(3);
        hasNext.Should().Be(true);
        hasPrevious.Should().Be(false);

        response.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task GetSerhanKitaplarPaginated_ShouldPassDefaultParameters_WhenNotProvided()
    {
        // Arrange
        var emptyPaginatedList = new PaginatedListDto<GetSerhanKitapDto>
        {
            Items = new List<GetSerhanKitapDto>(),
            TotalCount = 0,
            PageNumber = 1,
            PageSize = 10
        };
        var result = Result<PaginatedListDto<GetSerhanKitapDto>>.Success(SuccessMessage, emptyPaginatedList);

        GetSerhanKitapListQuery? capturedQuery = null;
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetSerhanKitapListQuery>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<Result<PaginatedListDto<GetSerhanKitapDto>>>, CancellationToken>((q, _) =>
                capturedQuery = q as GetSerhanKitapListQuery)
            .ReturnsAsync(result);

        // Act
        await _controller.GetSerhanKitaplar(new GetSerhanKitaplarRequest());

        // Assert
        capturedQuery.Should().NotBeNull();
        capturedQuery!.PageNumber.Should().Be(1);
        capturedQuery.PageSize.Should().Be(10);
        capturedQuery.SortBy.Should().BeNull();
        capturedQuery.SortDescending.Should().BeFalse();
    }

    [Fact]
    public async Task GetSerhanKitaplarPaginated_ShouldPassCustomParameters_WhenProvided()
    {
        // Arrange
        var paginatedList = new PaginatedListDto<GetSerhanKitapDto>
        {
            Items = new List<GetSerhanKitapDto>(),
            TotalCount = 0,
            PageNumber = 2,
            PageSize = 20
        };
        var result = Result<PaginatedListDto<GetSerhanKitapDto>>.Success(SuccessMessage, paginatedList);

        GetSerhanKitapListQuery? capturedQuery = null;
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetSerhanKitapListQuery>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<Result<PaginatedListDto<GetSerhanKitapDto>>>, CancellationToken>((q, _) =>
                capturedQuery = q as GetSerhanKitapListQuery)
            .ReturnsAsync(result);

        // Act
        await _controller.GetSerhanKitaplar(new GetSerhanKitaplarRequest
        {
            PageNumber = 2,
            PageSize = 20,
            SortBy = KitapSortField.KitapName,
            SortDescending = true
        });

        // Assert
        capturedQuery.Should().NotBeNull();
        capturedQuery!.PageNumber.Should().Be(2);
        capturedQuery.PageSize.Should().Be(20);
        capturedQuery.SortBy.Should().Be(KitapSortField.KitapName);
        capturedQuery.SortDescending.Should().Be(true);
    }

    [Fact]
    public async Task GetSerhanKitaplarPaginated_ShouldReturnOk_WhenNoBooksExist()
    {
        // Arrange
        var emptyPaginatedList = new PaginatedListDto<GetSerhanKitapDto>
        {
            Items = new List<GetSerhanKitapDto>(),
            TotalCount = 0,
            PageNumber = 1,
            PageSize = 10
        };
        var result = Result<PaginatedListDto<GetSerhanKitapDto>>.Success("No books found", emptyPaginatedList);

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetSerhanKitapListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var actionResult = await _controller.GetSerhanKitaplar(new GetSerhanKitaplarRequest());

        // Assert
        actionResult.Result.Should().BeOfType<OkObjectResult>();

        var okResult = actionResult.Result as OkObjectResult;
        var response = okResult!.Value as StandardApiResponse<object>;

        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.Data.Should().NotBeNull();

        var items = response.Data as List<GetSerhanKitapDto>;
        items.Should().NotBeNull();
        items.Should().BeEmpty();

        response.Metadata.Should().NotBeNull();
        response.Metadata!.TryGetValue("totalCount", out var totalCount).Should().BeTrue();
        totalCount.Should().Be(0);
    }

    [Fact]
    public async Task GetSerhanKitaplarPaginated_ShouldCallMediator_Once()
    {
        // Arrange
        var paginatedList = new PaginatedListDto<GetSerhanKitapDto>
        {
            Items = new List<GetSerhanKitapDto>(),
            TotalCount = 0,
            PageNumber = 1,
            PageSize = 10
        };
        var result = Result<PaginatedListDto<GetSerhanKitapDto>>.Success(SuccessMessage, paginatedList);

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetSerhanKitapListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        await _controller.GetSerhanKitaplar(new GetSerhanKitaplarRequest());

        // Assert
        _mediatorMock.Verify(m => m.Send(It.IsAny<GetSerhanKitapListQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(1, 10)]
    [InlineData(5, 25)]
    [InlineData(10, 50)]
    public async Task GetSerhanKitaplarPaginated_ShouldPassPaginationParameters(int pageNumber, int pageSize)
    {
        // Arrange
        var paginatedList = new PaginatedListDto<GetSerhanKitapDto>
        {
            Items = new List<GetSerhanKitapDto>(),
            TotalCount = 0,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
        var result = Result<PaginatedListDto<GetSerhanKitapDto>>.Success(SuccessMessage, paginatedList);

        GetSerhanKitapListQuery? capturedQuery = null;
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetSerhanKitapListQuery>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<Result<PaginatedListDto<GetSerhanKitapDto>>>, CancellationToken>((q, _) =>
                capturedQuery = q as GetSerhanKitapListQuery)
            .ReturnsAsync(result);

        // Act
        await _controller.GetSerhanKitaplar(new GetSerhanKitaplarRequest
        {
            PageNumber = pageNumber,
            PageSize = pageSize
        });

        // Assert
        capturedQuery.Should().NotBeNull();
        capturedQuery!.PageNumber.Should().Be(pageNumber);
        capturedQuery.PageSize.Should().Be(pageSize);
    }

    [Theory]
    [InlineData(KitapSortField.KitapName, false)]
    [InlineData(KitapSortField.KitapYazar, true)]
    [InlineData(KitapSortField.KitapSayfaSayisi, false)]
    public async Task GetSerhanKitaplarPaginated_ShouldPassSortingParameters(KitapSortField sortBy, bool sortDescending)
    {
        // Arrange
        var paginatedList = new PaginatedListDto<GetSerhanKitapDto>
        {
            Items = new List<GetSerhanKitapDto>(),
            TotalCount = 0,
            PageNumber = 1,
            PageSize = 10
        };
        var result = Result<PaginatedListDto<GetSerhanKitapDto>>.Success(SuccessMessage, paginatedList);

        GetSerhanKitapListQuery? capturedQuery = null;
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetSerhanKitapListQuery>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<Result<PaginatedListDto<GetSerhanKitapDto>>>, CancellationToken>((q, _) =>
                capturedQuery = q as GetSerhanKitapListQuery)
            .ReturnsAsync(result);

        // Act
        await _controller.GetSerhanKitaplar(new GetSerhanKitaplarRequest
        {
            SortBy = sortBy,
            SortDescending = sortDescending
        });

        // Assert
        capturedQuery.Should().NotBeNull();
        capturedQuery!.SortBy.Should().Be(sortBy);
        capturedQuery.SortDescending.Should().Be(sortDescending);
    }

    #endregion

    #region GetSerhanKitapDetail Tests

    [Fact]
    public async Task GetSerhanKitapDetail_ShouldReturnOk_WhenBookExists()
    {
        // Arrange
        var bookId = "test-id-123";
        var book = new GetSerhanKitapDto
        {
            Id = bookId,
            KitapName = "Kürk Mantolu Madonna",
            KitapYazar = "Sabahattin Ali",
            KitapSayfaSayisi = 160
        };
        var result = Result<GetSerhanKitapDto>.Success("Book found", book);

        _mediatorMock.Setup(m => m.Send(It.Is<GetSerhanKitapDetailsQuery>(q => q.Id == bookId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var actionResult = await _controller.GetSerhanKitapDetail(bookId);

        // Assert
        actionResult.Should().NotBeNull();
        actionResult.Result.Should().BeOfType<OkObjectResult>();

        var okResult = actionResult.Result as OkObjectResult;
        var response = okResult!.Value as StandardApiResponse<GetSerhanKitapDto>;

        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.Data.Should().BeEquivalentTo(book);
    }

    [Fact]
    public async Task GetSerhanKitapDetail_ShouldReturnNotFound_WhenBookDoesNotExist()
    {
        // Arrange
        var bookId = "non-existent-id";
        var result = Result<GetSerhanKitapDto>.Failure(BookNotFoundMessage, 404);

        _mediatorMock.Setup(m => m.Send(It.Is<GetSerhanKitapDetailsQuery>(q => q.Id == bookId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var actionResult = await _controller.GetSerhanKitapDetail(bookId);

        // Assert
        actionResult.Should().NotBeNull();
        actionResult.Result.Should().BeOfType<NotFoundObjectResult>();

        var notFoundResult = actionResult.Result as NotFoundObjectResult;
        var response = notFoundResult!.Value as StandardApiResponse<GetSerhanKitapDto>;

        response.Should().NotBeNull();
        response!.Success.Should().BeFalse();
        response.StatusCode.Should().Be(404);
        response.Message.Should().Be(BookNotFoundMessage);
    }

    [Fact]
    public async Task GetSerhanKitapDetail_ShouldPassCorrectId_ToMediator()
    {
        // Arrange
        var bookId = "specific-id";
        var result = Result<GetSerhanKitapDto>.Failure("Not found", 404);
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetSerhanKitapDetailsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        await _controller.GetSerhanKitapDetail(bookId);

        // Assert
        _mediatorMock.Verify(m => m.Send(It.Is<GetSerhanKitapDetailsQuery>(q => q.Id == bookId), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region CreateSerhanKitap Tests

    [Fact]
    public async Task CreateSerhanKitap_ShouldReturnOk_WhenCreationSucceeds()
    {
        // Arrange
        var createDto = new CreateSerhanKitapDto
        {
            KitapName = "Tutunamayanlar",
            KitapYazar = "Oğuz Atay",
            KitapSayfaSayisi = 724
        };
        var createdId = "new-book-id";
        var result = Result<string>.Success("Book created successfully", createdId);

        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateSerhanKitapCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var actionResult = await _controller.CreateSerhanKitap(createDto);

        // Assert
        actionResult.Should().NotBeNull();
        actionResult.Result.Should().BeOfType<OkObjectResult>();

        var okResult = actionResult.Result as OkObjectResult;
        var response = okResult!.Value as StandardApiResponse<string>;

        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.Data.Should().Be(createdId);
        response.Message.Should().Be("Book created successfully");
    }

    [Fact]
    public async Task CreateSerhanKitap_ShouldReturnBadRequest_WhenValidationFails()
    {
        // Arrange
        var createDto = new CreateSerhanKitapDto
        {
            KitapName = "",
            KitapYazar = DefaultAuthor,
            KitapSayfaSayisi = -1
        };
        var result = Result<string>.Failure("Validation failed", 400);

        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateSerhanKitapCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var actionResult = await _controller.CreateSerhanKitap(createDto);

        // Assert
        actionResult.Should().NotBeNull();
        actionResult.Result.Should().BeOfType<BadRequestObjectResult>();

        var badRequestResult = actionResult.Result as BadRequestObjectResult;
        var response = badRequestResult!.Value as StandardApiResponse<string>;

        response.Should().NotBeNull();
        response!.Success.Should().BeFalse();
        response.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task CreateSerhanKitap_ShouldPassDto_ToCommand()
    {
        // Arrange
        var createDto = new CreateSerhanKitapDto
        {
            KitapName = "Test",
            KitapYazar = DefaultAuthor,
            KitapSayfaSayisi = 100
        };
        var result = Result<string>.Success(SuccessMessage, "id");

        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateSerhanKitapCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        await _controller.CreateSerhanKitap(createDto);

        // Assert
        _mediatorMock.Verify(m => m.Send(
            It.Is<CreateSerhanKitapCommand>(cmd => cmd.CreateSerhanKitapDto == createDto),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region EditSerhanKitap Tests

    [Fact]
    public async Task EditSerhanKitap_ShouldReturnOk_WhenUpdateSucceeds()
    {
        // Arrange
        var bookId = "existing-id";
        var editDto = new EditSerhanKitapDto
        {
            KitapName = "İnce Memed",
            KitapYazar = "Yaşar Kemal",
            KitapSayfaSayisi = 436
        };
        var result = Result<Unit>.Success("Book updated successfully", Unit.Value);

        _mediatorMock.Setup(m => m.Send(It.IsAny<EditSerhanKitapCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var actionResult = await _controller.EditSerhanKitap(bookId, editDto);

        // Assert
        actionResult.Should().NotBeNull();
        actionResult.Result.Should().BeOfType<OkObjectResult>();

        var okResult = actionResult.Result as OkObjectResult;
        var response = okResult!.Value as StandardApiResponse<Unit>;

        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.Message.Should().Be("Book updated successfully");
    }

    [Fact]
    public async Task EditSerhanKitap_ShouldReturnNotFound_WhenBookDoesNotExist()
    {
        // Arrange
        var bookId = "non-existent";
        var editDto = new EditSerhanKitapDto
        {
            KitapName = "Book",
            KitapYazar = DefaultAuthor,
            KitapSayfaSayisi = 100
        };
        var result = Result<Unit>.Failure(BookNotFoundMessage, 404);

        _mediatorMock.Setup(m => m.Send(It.IsAny<EditSerhanKitapCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var actionResult = await _controller.EditSerhanKitap(bookId, editDto);

        // Assert
        actionResult.Should().NotBeNull();
        actionResult.Result.Should().BeOfType<NotFoundObjectResult>();

        var notFoundResult = actionResult.Result as NotFoundObjectResult;
        var response = notFoundResult!.Value as StandardApiResponse<Unit>;

        response.Should().NotBeNull();
        response!.Success.Should().BeFalse();
        response.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task EditSerhanKitap_ShouldPassIdAndDto_ToCommand()
    {
        // Arrange
        var bookId = "test-id";
        var editDto = new EditSerhanKitapDto
        {
            KitapName = "Name",
            KitapYazar = DefaultAuthor,
            KitapSayfaSayisi = 150
        };
        var result = Result<Unit>.Success(SuccessMessage, Unit.Value);

        _mediatorMock.Setup(m => m.Send(It.IsAny<EditSerhanKitapCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        await _controller.EditSerhanKitap(bookId, editDto);

        // Assert
        _mediatorMock.Verify(m => m.Send(
            It.Is<EditSerhanKitapCommand>(cmd => cmd.Id == bookId && cmd.EditSerhanKitapDto == editDto),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task EditSerhanKitap_ShouldReturnBadRequest_WhenValidationFails()
    {
        // Arrange
        var bookId = "id";
        var editDto = new EditSerhanKitapDto
        {
            KitapName = "",
            KitapYazar = "",
            KitapSayfaSayisi = -1
        };
        var result = Result<Unit>.Failure("Invalid data", 400);

        _mediatorMock.Setup(m => m.Send(It.IsAny<EditSerhanKitapCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var actionResult = await _controller.EditSerhanKitap(bookId, editDto);

        // Assert
        actionResult.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region DeleteSerhanKitap Tests

    [Fact]
    public async Task DeleteSerhanKitap_ShouldReturnOk_WhenDeletionSucceeds()
    {
        // Arrange
        var bookId = "book-to-delete";
        var result = Result<Unit>.Success("Book deleted successfully", Unit.Value);

        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteSerhanKitapCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var actionResult = await _controller.DeleteSerhanKitap(bookId);

        // Assert
        actionResult.Should().NotBeNull();
        actionResult.Result.Should().BeOfType<OkObjectResult>();

        var okResult = actionResult.Result as OkObjectResult;
        var response = okResult!.Value as StandardApiResponse<Unit>;

        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.Message.Should().Be("Book deleted successfully");
    }

    [Fact]
    public async Task DeleteSerhanKitap_ShouldReturnNotFound_WhenBookDoesNotExist()
    {
        // Arrange
        var bookId = "non-existent";
        var result = Result<Unit>.Failure(BookNotFoundMessage, 404);

        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteSerhanKitapCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var actionResult = await _controller.DeleteSerhanKitap(bookId);

        // Assert
        actionResult.Should().NotBeNull();
        actionResult.Result.Should().BeOfType<NotFoundObjectResult>();

        var notFoundResult = actionResult.Result as NotFoundObjectResult;
        var response = notFoundResult!.Value as StandardApiResponse<Unit>;

        response.Should().NotBeNull();
        response!.Success.Should().BeFalse();
        response.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task DeleteSerhanKitap_ShouldPassCorrectId_ToCommand()
    {
        // Arrange
        var bookId = "specific-book-id";
        var result = Result<Unit>.Success(SuccessMessage, Unit.Value);

        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteSerhanKitapCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        await _controller.DeleteSerhanKitap(bookId);

        // Assert
        _mediatorMock.Verify(m => m.Send(
            It.Is<DeleteSerhanKitapCommand>(cmd => cmd.Id == bookId),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteSerhanKitap_ShouldReturnBadRequest_WhenDeletionFails()
    {
        // Arrange
        var bookId = "id";
        var result = Result<Unit>.Failure("Cannot delete book", 400);

        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteSerhanKitapCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var actionResult = await _controller.DeleteSerhanKitap(bookId);

        // Assert
        actionResult.Result.Should().BeOfType<BadRequestObjectResult>();

        var badRequestResult = actionResult.Result as BadRequestObjectResult;
        var response = badRequestResult!.Value as StandardApiResponse<Unit>;

        response.Should().NotBeNull();
        response!.Success.Should().BeFalse();
        response.Message.Should().Be("Cannot delete book");
    }

    #endregion
}
