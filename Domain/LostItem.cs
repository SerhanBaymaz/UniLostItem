using Domain.Common;
using Domain.Common.Enums;

namespace Domain;

public class LostItem : BaseEntity
{
    public required string Title { get; set; }
    public required string Description { get; set; }
    public ItemCategory Category { get; set; }
    public ItemType ItemType { get; set; }
    public ItemStatus Status { get; set; } = ItemStatus.Active;
    public DateTime IncidentDate { get; set; }
    public string? ImageUrl { get; set; }
    public string? ImagePublicId { get; set; }
    public required string ContactInfo { get; set; }

    public required string LocationLabel { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }

    public required string UserId { get; set; } //FK

    //Navigation Properities
    public ApplicationUser User { get; set; } = null!;
    public ICollection<ItemClaim> Claims { get; set; } = new List<ItemClaim>();
}
