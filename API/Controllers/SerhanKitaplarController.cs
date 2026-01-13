using System;
using API.Responses;
using Application.Core.Pagination;
using Application.Features.SerhanKitaplar.Commands.CreateSerhanKitap;
using Application.Features.SerhanKitaplar.Commands.DeleteSerhanKitap;
using Application.Features.SerhanKitaplar.Commands.EditSerhanKitap;
using Application.Features.SerhanKitaplar.Queries.Common.DTOs;
using Application.Features.SerhanKitaplar.Queries.GetSerhanKitapDetails;
using Application.Features.SerhanKitaplar.Queries.GetSerhanKitapList;
using Application.Features.SerhanKitaplar.Queries.GetSerhanKitapPaginatedList;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/v1/serhan-kitaplar")]
public class SerhanKitaplarController : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<StandardApiResponse<List<GetSerhanKitapDto>>>> GetSerhanKitaplar()
    {
        return HandleResult(await Mediator.Send(new GetSerhanKitapListQuery()));
    }

    /// <summary>
    /// Gets a paginated list of books with optional sorting
    /// </summary>
    /// <param name="pageNumber">Page number (default: 1, min: 1)</param>
    /// <param name="pageSize">Page size (default: 10, min: 1, max: 100)</param>
    /// <param name="sortBy">Sort field: kitapname/name/ad, kitapyazar/yazar/author, kitapsayfasayisi/sayfasayisi/pagecount (default: kitapname)</param>
    /// <param name="sortDescending">Sort descending (default: false)</param>
    /// <returns>Paginated list of books with metadata</returns>
    /// <response code="200">Returns the paginated list of books</response>
    [HttpGet("paginated")]
    [ProducesResponseType(typeof(StandardApiResponse<PaginatedListDto<GetSerhanKitapDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<StandardApiResponse<PaginatedListDto<GetSerhanKitapDto>>>> GetSerhanKitaplarPaginated(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false)
    {
        var query = new GetSerhanKitapPaginatedListQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            SortBy = sortBy,
            SortDescending = sortDescending
        };
        return HandleResult(await Mediator.Send(query));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<StandardApiResponse<GetSerhanKitapDto>>> GetSerhanKitapDetail(string id)
    {
        return HandleResult(await Mediator.Send(new GetSerhanKitapDetailsQuery { Id = id }));
    }

    [HttpPost]
    public async Task<ActionResult<StandardApiResponse<string>>> CreateSerhanKitap([FromBody] CreateSerhanKitapDto createSerhanKitapDto)
    {
        var command = new CreateSerhanKitapCommand { CreateSerhanKitapDto = createSerhanKitapDto };
        return HandleResult(await Mediator.Send(command));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<StandardApiResponse<Unit>>> EditSerhanKitap(string id, [FromBody] EditSerhanKitapDto editSerhanKitapDto)
    {
        var command = new EditSerhanKitapCommand { Id = id, EditSerhanKitapDto = editSerhanKitapDto };
        return HandleResult(await Mediator.Send(command));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<StandardApiResponse<Unit>>> DeleteSerhanKitap(string id)
    {
        return HandleResult(await Mediator.Send(new DeleteSerhanKitapCommand { Id = id }));
    }
}
