using System.Threading;
using System.Threading.Tasks;
using Application.Core;
using Application.Features.SerhanKitaplar.Queries.Common.DTOs;
using AutoMapper;
using MediatR;
using Persistence;

namespace Application.Features.SerhanKitaplar.Queries.GetSerhanKitapDetails;

public class GetSerhanKitapDetailsQueryHandler : IRequestHandler<GetSerhanKitapDetailsQuery, Result<GetSerhanKitapDto>>
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public GetSerhanKitapDetailsQueryHandler(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<GetSerhanKitapDto>> Handle(GetSerhanKitapDetailsQuery request, CancellationToken cancellationToken)
    {
        var serhanKitap = await _context.SerhanKitaplar.FindAsync([request.Id], cancellationToken);

        if (serhanKitap == null)
        {
            return Result<GetSerhanKitapDto>.Failure("SerhanKitap not found", 404);
        }

        var dto = _mapper.Map<GetSerhanKitapDto>(serhanKitap);
        return Result<GetSerhanKitapDto>.Success("SerhanKitap retrieved successfully", dto);
    }
}
