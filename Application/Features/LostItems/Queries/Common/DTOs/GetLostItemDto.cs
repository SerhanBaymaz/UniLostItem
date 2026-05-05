using Domain.Common.Enums;

namespace Application.Features.LostItems.Queries.Common.DTOs;

public class GetLostItemDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ItemCategory Category { get; set; }
    public ItemType ItemType { get; set; }
    public ItemStatus Status { get; set; }
    public DateTime IncidentDate { get; set; }
    public string? ImageUrl { get; set; }
    public string LocationLabel { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserFullName { get; set; } = string.Empty;
    public int ClaimCount { get; set; }
    public DateTime CreatedDate { get; set; }
}
