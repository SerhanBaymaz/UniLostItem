using System;

namespace Application.Features.SerhanKitaplar.Commands.EditSerhanKitap;

public class EditSerhanKitapDto
{
    public required string KitapName { get; set; }
    public required string KitapYazar { get; set; }
    public required int KitapSayfaSayisi { get; set; }
}
