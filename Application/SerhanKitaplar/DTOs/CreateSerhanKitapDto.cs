using System;

namespace Application.SerhanKitaplar.DTOs;

public class CreateSerhanKitapDto
{
    public string KitapName { get; set; } = string.Empty;
    public string KitapYazar { get; set; } = string.Empty;
    public int KitapSayfaSayisi { get; set; }
}
