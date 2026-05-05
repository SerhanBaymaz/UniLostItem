namespace Application.Features.ItemClaims.Commands.CreateItemClaim;

public class CreateItemClaimDto
{
    public required string LostItemId { get; set; }
    public required string Description { get; set; }
}
