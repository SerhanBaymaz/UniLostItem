using System;
using API.Responses;
using Application.Core.Extensions;
using Application.Core.Pagination;
using Application.Features.SerhanKitaplar.Commands.CreateSerhanKitap;
using Application.Features.SerhanKitaplar.Commands.DeleteSerhanKitap;
using Application.Features.SerhanKitaplar.Commands.EditSerhanKitap;
using Application.Features.SerhanKitaplar.Queries.Common.DTOs;
using Application.Features.SerhanKitaplar.Queries.GetSerhanKitapDetails;
using Application.Features.SerhanKitaplar.Queries.GetSerhanKitapList;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/v1/serhan-kitaplar")]
[Authorize]
public class SerhanKitaplarController : BaseApiController
{
    [HttpGet]
    [ProducesResponseType(typeof(StandardApiResponse<PaginatedListDto<GetSerhanKitapDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<StandardApiResponse<PaginatedListDto<GetSerhanKitapDto>>>> GetSerhanKitaplar(
        [FromQuery] Requests.GetSerhanKitaplarRequest request)
    {
        var query = new GetSerhanKitapListQuery
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            SortBy = request.SortBy,
            SortDescending = request.SortDescending,
            SearchTerm = request.SearchTerm,
            KitapName = request.KitapName,
            KitapYazar = request.KitapYazar,
            MinPageCount = request.MinPageCount,
            MaxPageCount = request.MaxPageCount
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
