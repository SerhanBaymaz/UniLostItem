using System.ComponentModel.DataAnnotations;
using Application.Features.ItemClaims.Queries.Common.Enums;

namespace API.Controllers.Requests;

public class GetPendingClaimsRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Sayfa numarası en az 1 olmalıdır")]
    public int PageNumber { get; set; } = 1;

    [Range(1, 100, ErrorMessage = "Sayfa boyutu 1 ile 100 arasında olmalıdır")]
    public int PageSize { get; set; } = 10;

    public ItemClaimSortField? SortBy { get; set; }

    public bool SortDescending { get; set; } = false;

    [MaxLength(200, ErrorMessage = "Arama terimi en fazla 200 karakter olabilir")]
    public string? SearchTerm { get; set; }
}
