using System;
using API.Responses;
using Application.SerhanKitaplar.Commands;
using Application.SerhanKitaplar.DTOs;
using Application.SerhanKitaplar.Queries;
using Application.Core;
using Domain;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class SerhanKitaplarController : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<StandardApiResponse<List<SerhanKitap>>>> GetSerhanKitaplar()
    {
        var serhanKitaplar = await Mediator.Send(new GetSerhanKitapList.Query());
        return Success(serhanKitaplar, "SerhanKitaplar retrieved successfully");
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<StandardApiResponse<SerhanKitap>>> GetSerhanKitapDetail(string id)
    {
        return HandleResult(await Mediator.Send(new GetSerhanKitapDetails.Query { Id = id }));
    }

    [HttpPost]
    public async Task<ActionResult<StandardApiResponse<string>>> CreateSerhanKitap([FromBody] CreateSerhanKitapDto serhanKitapDto)
    {
        return HandleResult(await Mediator.Send(new CreateSerhanKitap.Command { SerhanKitapDto = serhanKitapDto }));
    }

    [HttpPut]
    public async Task<ActionResult<StandardApiResponse<Unit>>> EditSerhanKitap([FromBody] EditSerhanKitap.Command command)
    {
        return HandleResult(await Mediator.Send(command));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<StandardApiResponse<Unit>>> DeleteSerhanKitap(string id)
    {
        return HandleResult(await Mediator.Send(new DeleteSerhanKitap.Command { Id = id }));
    }
}
