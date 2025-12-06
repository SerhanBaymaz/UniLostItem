using System;
using Application.Core;
using Application.Features.SerhanKitaplar.Queries.Common.DTOs;
using MediatR;

namespace Application.Features.SerhanKitaplar.Queries.GetSerhanKitapDetails;

public class GetSerhanKitapDetailsQuery : IRequest<Result<GetSerhanKitapDto>>
{
    public required string Id { get; set; }
}
