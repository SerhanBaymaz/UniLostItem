using System;

namespace Domain;

public class SerhanKitap
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public required string KitapName { get; set; }
    public required string KitapYazar { get; set; }
    public int KitapSayfaSayisi { get; set; }
}
