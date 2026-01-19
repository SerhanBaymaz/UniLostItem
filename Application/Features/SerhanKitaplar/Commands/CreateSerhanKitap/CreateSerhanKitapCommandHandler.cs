using System.Threading;
using System.Threading.Tasks;
using Application.Core;
using Application.Interfaces;
using AutoMapper;
using Domain;
using MediatR;
using Persistence;

namespace Application.Features.SerhanKitaplar.Commands.CreateSerhanKitap;

public class CreateSerhanKitapCommandHandler : IRequestHandler<CreateSerhanKitapCommand, Result<string>>
{
    private readonly IAppDbContext _context;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public CreateSerhanKitapCommandHandler(IAppDbContext context, IMapper mapper, ICurrentUserService currentUserService)
    {
        _context = context;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<Result<string>> Handle(CreateSerhanKitapCommand request, CancellationToken cancellationToken)
    {
        var serhanKitap = _mapper.Map<SerhanKitap>(request.CreateSerhanKitapDto);

        // Set audit fields
        serhanKitap.CreatedDate = System.DateTime.UtcNow;
        serhanKitap.CreatedBy = _currentUserService.UserId;

        _context.SerhanKitaplar.Add(serhanKitap);

        var result = await _context.SaveChangesAsync(cancellationToken) > 0;

        if (!result)
        {
            return Result<string>.Failure("Failed to create the serhan kitap", 400);
        }

        return Result<string>.Success("Serhan kitap created successfully", serhanKitap.Id);
    }
}
