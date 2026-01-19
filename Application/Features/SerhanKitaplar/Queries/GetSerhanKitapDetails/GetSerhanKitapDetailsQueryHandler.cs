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
    private readonly IAppDbContext _context;
    private readonly IMapper _mapper;

    public GetSerhanKitapDetailsQueryHandler(IAppDbContext context, IMapper mapper)
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

        if (serhanKitap.IsDeleted)
        {
            return Result<GetSerhanKitapDto>.Failure("SerhanKitap has been deleted", 410);
        }

        if (!serhanKitap.IsActive)
        {
            return Result<GetSerhanKitapDto>.Failure("SerhanKitap is not active", 423);
        }

        var dto = _mapper.Map<GetSerhanKitapDto>(serhanKitap);
        return Result<GetSerhanKitapDto>.Success("SerhanKitap retrieved successfully", dto);
    }
}
