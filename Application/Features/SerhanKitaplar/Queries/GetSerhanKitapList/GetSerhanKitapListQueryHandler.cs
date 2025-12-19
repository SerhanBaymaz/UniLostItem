using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Core;
using Application.Features.SerhanKitaplar.Queries.Common.DTOs;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Features.SerhanKitaplar.Queries.GetSerhanKitapList;

public class GetSerhanKitapListQueryHandler : IRequestHandler<GetSerhanKitapListQuery, Result<List<GetSerhanKitapDto>>>
{
    private readonly IAppDbContext _context;
    private readonly IMapper _mapper;

    public GetSerhanKitapListQueryHandler(IAppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<List<GetSerhanKitapDto>>> Handle(GetSerhanKitapListQuery request, CancellationToken cancellationToken)
    {
        var serhanKitaplar = await _context.SerhanKitaplar.ToListAsync(cancellationToken);
        var dtos = _mapper.Map<List<GetSerhanKitapDto>>(serhanKitaplar);
        return Result<List<GetSerhanKitapDto>>.Success("SerhanKitaplar retrieved successfully", dtos);
    }
}
