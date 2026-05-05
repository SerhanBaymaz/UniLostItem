using System.ComponentModel;

namespace Application.Features.LostItems.Queries.Common.Enums;

public enum LostItemSortField
{
    [Description("title")]
    Title,

    [Description("category")]
    Category,

    [Description("incidentdate")]
    IncidentDate,

    [Description("createddate")]
    CreatedDate
}
