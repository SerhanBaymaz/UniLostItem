using Application.Core;
using Application.Features.LostItems.Queries.Common.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Features.LostItems.Queries.GetLostItemDetails;

public class GetLostItemDetailsQueryHandler : IRequestHandler<GetLostItemDetailsQuery, Result<GetLostItemDetailDto>>
{
    private readonly IAppDbContext _context;

    public GetLostItemDetailsQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<GetLostItemDetailDto>> Handle(GetLostItemDetailsQuery request, CancellationToken cancellationToken)
    {
        var lostItem = await _context.LostItems
            .Where(x => x.Id == request.Id)
            .Select(x => new GetLostItemDetailDto
            {
                Id = x.Id,
                Title = x.Title,
                Description = x.Description,
                Category = x.Category,
                ItemType = x.ItemType,
                Status = x.Status,
                IncidentDate = x.IncidentDate,
                ImageUrl = x.ImageUrl,
                ContactInfo = x.ContactInfo,
                LocationLabel = x.LocationLabel,
                Latitude = x.Latitude,
                Longitude = x.Longitude,
                UserId = x.UserId,
                UserFullName = x.User.FirstName + " " + x.User.LastName,
                ClaimCount = x.Claims.Count,
                CreatedDate = x.CreatedDate,
                CreatedBy = x.CreatedBy,
                UpdatedDate = x.UpdatedDate,
                IsActive = x.IsActive,
                IsDeleted = x.IsDeleted
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (lostItem == null)
        {
            return Result<GetLostItemDetailDto>.Failure("Kayıt bulunamadı", 404);
        }

        if (lostItem.IsDeleted)
        {
            return Result<GetLostItemDetailDto>.Failure("Kayıt silinmiş", 410);
        }

        if (!lostItem.IsActive)
        {
            return Result<GetLostItemDetailDto>.Failure("Kayıt aktif değil", 423);
        }

        return Result<GetLostItemDetailDto>.Success("Kayıt başarıyla getirildi", lostItem);
    }
}
