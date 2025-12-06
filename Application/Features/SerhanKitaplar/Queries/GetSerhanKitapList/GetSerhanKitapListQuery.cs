using System.Collections.Generic;
using Application.Features.SerhanKitaplar.Queries.Common.DTOs;
using MediatR;

namespace Application.Features.SerhanKitaplar.Queries.GetSerhanKitapList;

public class GetSerhanKitapListQuery : IRequest<List<GetSerhanKitapDto>> { }
