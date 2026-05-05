using Application.Core;
using Application.Interfaces;
using AutoMapper;
using Domain;
using MediatR;
using Persistence;

namespace Application.Features.LostItems.Commands.CreateLostItem;

public class CreateLostItemCommandHandler : IRequestHandler<CreateLostItemCommand, Result<string>>
{
    private readonly IAppDbContext _context;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public CreateLostItemCommandHandler(IAppDbContext context, IMapper mapper, ICurrentUserService currentUserService)
    {
        _context = context;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<Result<string>> Handle(CreateLostItemCommand request, CancellationToken cancellationToken)
    {
        var lostItem = _mapper.Map<LostItem>(request.CreateLostItemDto);

        lostItem.CreatedDate = DateTime.UtcNow;
        lostItem.CreatedBy = _currentUserService.UserId;
        lostItem.UserId = _currentUserService.UserId!;

        _context.LostItems.Add(lostItem);

        var result = await _context.SaveChangesAsync(cancellationToken) > 0;

        if (!result)
        {
            return Result<string>.Failure("Kayıt oluşturulamadı", 400);
        }

        return Result<string>.Success("Kayıt başarıyla oluşturuldu", lostItem.Id);
    }
}
