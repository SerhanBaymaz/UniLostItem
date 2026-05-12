using API.Controllers.Requests;
using API.Responses;
using Application.Core.Pagination;
using Application.Features.LostItems.Commands.CreateLostItem;
using Application.Features.LostItems.Commands.DeleteLostItem;
using Application.Features.LostItems.Commands.UpdateLostItem;
using Application.Features.LostItems.Queries.Common.DTOs;
using Application.Features.LostItems.Queries.GetLostItemDetails;
using Application.Features.LostItems.Queries.GetLostItemList;
using Application.Features.LostItems.Queries.GetMyLostItems;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/v1/items")]
[ApiController]
public class LostItemsController : BaseApiController
{
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(StandardApiResponse<PaginatedListDto<GetLostItemDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<StandardApiResponse<PaginatedListDto<GetLostItemDto>>>> GetItems(
        [FromQuery] GetLostItemsRequest request)
    {
        var query = new GetLostItemListQuery
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            SortBy = request.SortBy,
            SortDescending = request.SortDescending,
            SearchTerm = request.SearchTerm,
            ItemType = request.ItemType,
            Category = request.Category,
            Status = request.Status
        };
        return HandleResult(await Mediator.Send(query));
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<StandardApiResponse<GetLostItemDetailDto>>> GetItemDetails(string id)
    {
        return HandleResult(await Mediator.Send(new GetLostItemDetailsQuery { Id = id }));
    }

    [HttpGet("my-items")]
    [Authorize]
    [ProducesResponseType(typeof(StandardApiResponse<PaginatedListDto<GetLostItemDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<StandardApiResponse<PaginatedListDto<GetLostItemDto>>>> GetMyItems(
        [FromQuery] GetLostItemsRequest request)
    {
        var query = new GetMyLostItemsQuery
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            SortBy = request.SortBy,
            SortDescending = request.SortDescending,
            SearchTerm = request.SearchTerm,
            ItemType = request.ItemType,
            Category = request.Category,
            Status = request.Status
        };
        return HandleResult(await Mediator.Send(query));
    }

    [HttpPost]
    [Authorize]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<StandardApiResponse<string>>> CreateItem(
        [FromForm] CreateLostItemDto createLostItemDto,
        IFormFile? image)
    {
        if (image != null)
        {
            createLostItemDto.ImageStream = image.OpenReadStream();
            createLostItemDto.ImageFileName = image.FileName;
        }
        var command = new CreateLostItemCommand { CreateLostItemDto = createLostItemDto };
        return HandleResult(await Mediator.Send(command));
    }

    [HttpPut("{id}")]
    [Authorize]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<StandardApiResponse<Unit>>> UpdateItem(
        string id,
        [FromForm] UpdateLostItemDto updateLostItemDto,
        IFormFile? image)
    {
        if (image != null)
        {
            updateLostItemDto.ImageStream = image.OpenReadStream();
            updateLostItemDto.ImageFileName = image.FileName;
        }
        var command = new UpdateLostItemCommand { Id = id, UpdateLostItemDto = updateLostItemDto };
        return HandleResult(await Mediator.Send(command));
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<ActionResult<StandardApiResponse<Unit>>> DeleteItem(string id)
    {
        return HandleResult(await Mediator.Send(new DeleteLostItemCommand { Id = id }));
    }
}
