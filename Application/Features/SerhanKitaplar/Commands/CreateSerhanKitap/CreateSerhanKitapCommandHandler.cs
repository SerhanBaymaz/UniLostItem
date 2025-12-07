using System.Threading;
using System.Threading.Tasks;
using Application.Core;
using AutoMapper;
using Domain;
using MediatR;
using Persistence;

namespace Application.Features.SerhanKitaplar.Commands.CreateSerhanKitap;

public class CreateSerhanKitapCommandHandler : IRequestHandler<CreateSerhanKitapCommand, Result<string>>
{
    private readonly IAppDbContext _context;
    private readonly IMapper _mapper;

    public CreateSerhanKitapCommandHandler(IAppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<string>> Handle(CreateSerhanKitapCommand request, CancellationToken cancellationToken)
    {
        var serhanKitap = _mapper.Map<SerhanKitap>(request.CreateSerhanKitapDto);

        _context.SerhanKitaplar.Add(serhanKitap);

        var result = await _context.SaveChangesAsync(cancellationToken) > 0;

        if (!result)
        {
            return Result<string>.Failure("Failed to create the serhan kitap", 400);
        }

        return Result<string>.Success("Serhan kitap created successfully", serhanKitap.Id);
    }
}
