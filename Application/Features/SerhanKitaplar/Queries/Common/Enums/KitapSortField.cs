using System.ComponentModel;

namespace Application.Features.SerhanKitaplar.Queries.Common.Enums;

/// <summary>
/// Defines valid sort fields for SerhanKitap queries with corresponding database field names
/// </summary>
public enum KitapSortField
{
    /// <summary>
    /// Sort by kitap name (book name)
    /// </summary>
    [Description("kitapname")]
    KitapName,

    /// <summary>
    /// Sort by kitap yazar (author name)
    /// </summary>
    [Description("kitapyazar")]
    KitapYazar,

    /// <summary>
    /// Sort by kitap sayfa sayisi (page count)
    /// </summary>
    [Description("kitapsayfasayisi")]
    KitapSayfaSayisi
}
