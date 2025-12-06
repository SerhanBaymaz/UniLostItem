using System;
using API.Responses;
using Application.Features.SerhanKitaplar.Commands.CreateSerhanKitap;
using Application.Features.SerhanKitaplar.Commands.DeleteSerhanKitap;
using Application.Features.SerhanKitaplar.Commands.EditSerhanKitap;
using Application.Features.SerhanKitaplar.Queries.Common.DTOs;
using Application.Features.SerhanKitaplar.Queries.GetSerhanKitapDetails;
using Application.Features.SerhanKitaplar.Queries.GetSerhanKitapList;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/v1/serhan-kitaplar")]
public class SerhanKitaplarController : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<StandardApiResponse<List<GetSerhanKitapDto>>>> GetSerhanKitaplar()
    {
        var serhanKitaplar = await Mediator.Send(new GetSerhanKitapListQuery());
        return Success(serhanKitaplar, "SerhanKitaplar retrieved successfully");
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
