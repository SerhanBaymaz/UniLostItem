using System.ComponentModel;

namespace Application.Features.ItemClaims.Queries.Common.Enums;

public enum ItemClaimSortField
{
    [Description("createddate")]
    CreatedDate,

    [Description("status")]
    Status,

    [Description("expiresat")]
    ExpiresAt
}
