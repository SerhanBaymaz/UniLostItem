namespace Application.Features.ItemClaims.Commands.AdminReviewClaim;

public class AdminReviewClaimDto
{
    public required bool IsApproved { get; set; }
    public string? Comment { get; set; }
}
