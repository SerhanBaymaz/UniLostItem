using Application.Core;
using Application.Interfaces;
using AutoMapper;
using Domain;
using MediatR;
using Persistence;

namespace Application.Features.LostItems.Commands.CreateLostItem;

public class CreateLostItemCommandHandler(
    IAppDbContext context,
    IMapper mapper,
    ICurrentUserService currentUserService,
    IImageStorageService imageStorageService) : IRequestHandler<CreateLostItemCommand, Result<string>>
{
    private readonly IAppDbContext _context = context;
    private readonly IMapper _mapper = mapper;
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly IImageStorageService _imageStorageService = imageStorageService;

    public async Task<Result<string>> Handle(CreateLostItemCommand request, CancellationToken cancellationToken)
    {
        var lostItem = _mapper.Map<LostItem>(request.CreateLostItemDto);

        lostItem.CreatedDate = DateTime.UtcNow;
        lostItem.CreatedBy = _currentUserService.UserId;
        lostItem.UserId = _currentUserService.UserId!;

        if (request.CreateLostItemDto.ImageStream != null
            && !string.IsNullOrEmpty(request.CreateLostItemDto.ImageFileName))
        {
            var uploadResult = await _imageStorageService.UploadImageAsync(
                request.CreateLostItemDto.ImageStream,
                request.CreateLostItemDto.ImageFileName,
                cancellationToken);
            lostItem.ImageUrl = uploadResult.Url;
            lostItem.ImagePublicId = uploadResult.PublicId;
        }

        _context.LostItems.Add(lostItem);

        var result = await _context.SaveChangesAsync(cancellationToken) > 0;

        if (!result)
        {
            return Result<string>.Failure("Kayıt oluşturulamadı", 400);
        }

        return Result<string>.Success("Kayıt başarıyla oluşturuldu", lostItem.Id);
    }
}
