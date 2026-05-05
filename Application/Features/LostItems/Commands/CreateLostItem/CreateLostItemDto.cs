using Domain.Common.Enums;

namespace Application.Features.LostItems.Commands.CreateLostItem;

public class CreateLostItemDto
{
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required ItemCategory Category { get; set; }
    public required ItemType ItemType { get; set; }
    public required DateTime IncidentDate { get; set; }
    public string? ImageUrl { get; set; }
    public string? ContactInfo { get; set; }
    public required string LocationLabel { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}
