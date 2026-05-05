namespace Application.Features.ItemClaims.Commands.RespondToClaim;

public class RespondToClaimDto
{
    public required bool IsApproved { get; set; }
    public string? Comment { get; set; }
}
