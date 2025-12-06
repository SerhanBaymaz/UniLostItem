using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Features.SerhanKitaplar.Queries.Common.DTOs;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Features.SerhanKitaplar.Queries.GetSerhanKitapList;

public class GetSerhanKitapListQueryHandler : IRequestHandler<GetSerhanKitapListQuery, List<GetSerhanKitapDto>>
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public GetSerhanKitapListQueryHandler(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<GetSerhanKitapDto>> Handle(GetSerhanKitapListQuery request, CancellationToken cancellationToken)
    {
        var serhanKitaplar = await _context.SerhanKitaplar.ToListAsync(cancellationToken);
        return _mapper.Map<List<GetSerhanKitapDto>>(serhanKitaplar);
    }
}
