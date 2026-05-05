namespace Domain.Common.Enums;

public enum ClaimStatus
{
    Pending = 0,
    ApprovedByOwner = 1,
    RejectedByOwner = 2,
    ApprovedByAdmin = 3,
    RejectedByAdmin = 4,
    Cancelled = 5
}
