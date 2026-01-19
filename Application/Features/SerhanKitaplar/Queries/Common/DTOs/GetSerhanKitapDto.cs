using System;

namespace Application.Features.SerhanKitaplar.Queries.Common.DTOs;

public class GetSerhanKitapDto
{
    public string Id { get; set; } = string.Empty;
    public string KitapName { get; set; } = string.Empty;
    public string KitapYazar { get; set; } = string.Empty;
    public int KitapSayfaSayisi { get; set; }

    // Audit fields
    public DateTime CreatedDate { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public bool IsActive { get; set; }
}
