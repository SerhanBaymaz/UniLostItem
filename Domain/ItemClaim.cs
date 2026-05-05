using Domain.Common;
using Domain.Common.Enums;

namespace Domain;

public class ItemClaim : BaseEntity
{
    public required string Description { get; set; }
    public ClaimStatus Status { get; set; } = ClaimStatus.Pending;

    public DateTime ExpiresAt { get; set; }
    public int ExtensionCount { get; set; }

    public string LostItemId { get; set; } = string.Empty;
    public LostItem LostItem { get; set; } = null!; //FK
    public string ClaimantId { get; set; } = string.Empty; //FK
    public ApplicationUser Claimant { get; set; } = null!;

    public string? OwnerComment { get; set; }
    public DateTime? OwnerResponseDate { get; set; }

    public string? ReviewedBy { get; set; }
    public DateTime? ReviewedDate { get; set; }
    public string? AdminComment { get; set; }
}
