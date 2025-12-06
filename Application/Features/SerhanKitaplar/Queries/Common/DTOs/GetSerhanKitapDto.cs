using System;

namespace Application.Features.SerhanKitaplar.Queries.Common.DTOs;

public class GetSerhanKitapDto
{
    public string Id { get; set; } = string.Empty;
    public string KitapName { get; set; } = string.Empty;
    public string KitapYazar { get; set; } = string.Empty;
    public int KitapSayfaSayisi { get; set; }
}
