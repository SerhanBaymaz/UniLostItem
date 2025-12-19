using System.Collections.Generic;
using Application.Core;
using Application.Features.SerhanKitaplar.Queries.Common.DTOs;
using MediatR;

namespace Application.Features.SerhanKitaplar.Queries.GetSerhanKitapList;

public class GetSerhanKitapListQuery : IRequest<Result<List<GetSerhanKitapDto>>> { }
