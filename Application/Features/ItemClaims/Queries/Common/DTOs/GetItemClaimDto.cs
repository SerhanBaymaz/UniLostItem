using Domain.Common.Enums;

namespace Application.Features.ItemClaims.Queries.Common.DTOs;

public class GetItemClaimDto
{
    public string Id { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ClaimStatus Status { get; set; }
    public DateTime ExpiresAt { get; set; }
    public int ExtensionCount { get; set; }
    public string LostItemId { get; set; } = string.Empty;
    public string LostItemTitle { get; set; } = string.Empty;
    public string ClaimantId { get; set; } = string.Empty;
    public string ClaimantFullName { get; set; } = string.Empty;
    public string? OwnerComment { get; set; }
    public DateTime? OwnerResponseDate { get; set; }
    public string? ReviewedBy { get; set; }
    public DateTime? ReviewedDate { get; set; }
    public string? AdminComment { get; set; }
    public DateTime CreatedDate { get; set; }
}
