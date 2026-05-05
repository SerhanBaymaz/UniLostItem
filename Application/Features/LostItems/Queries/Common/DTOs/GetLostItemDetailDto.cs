namespace Application.Features.LostItems.Queries.Common.DTOs;

public class GetLostItemDetailDto : GetLostItemDto
{
    public string? ContactInfo { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
}
