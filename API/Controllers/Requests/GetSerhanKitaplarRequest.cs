using System.ComponentModel.DataAnnotations;
using Application.Features.SerhanKitaplar;

namespace API.Controllers.Requests;

/// <summary>
/// Request DTO for getting paginated SerhanKitap list
/// </summary>
public class GetSerhanKitaplarRequest
{
    /// <summary>
    /// Page number (default: 1, min: 1)
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Sayfa numarası en az 1 olmalıdır")]
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Page size (default: 10, min: 1, max: 100)
    /// </summary>
    [Range(1, 100, ErrorMessage = "Sayfa boyutu 1 ile 100 arasında olmalıdır")]
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// Sort field (default: KitapName)
    /// </summary>
    public KitapSortField? SortBy { get; set; }

    /// <summary>
    /// Sort descending (default: false)
    /// </summary>
    public bool SortDescending { get; set; } = false;

    /// <summary>
    /// Search in both book name and author
    /// </summary>
    [MaxLength(200, ErrorMessage = "Arama terimi en fazla 200 karakter olabilir")]
    public string? SearchTerm { get; set; }

    /// <summary>
    /// Filter by book name (contains)
    /// </summary>
    [MaxLength(200, ErrorMessage = "Kitap adı en fazla 200 karakter olabilir")]
    public string? KitapName { get; set; }

    /// <summary>
    /// Filter by author (contains)
    /// </summary>
    [MaxLength(200, ErrorMessage = "Yazar adı en fazla 200 karakter olabilir")]
    public string? KitapYazar { get; set; }

    /// <summary>
    /// Minimum page count
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Minimum sayfa sayısı en az 1 olmalıdır")]
    public int? MinPageCount { get; set; }

    /// <summary>
    /// Maximum page count
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Maksimum sayfa sayısı en az 1 olmalıdır")]
    public int? MaxPageCount { get; set; }
}
