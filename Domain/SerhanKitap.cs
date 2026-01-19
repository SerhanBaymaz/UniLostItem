using System;
using Domain.Common;

namespace Domain;

public class SerhanKitap : BaseEntity
{
    public required string KitapName { get; set; }
    public required string KitapYazar { get; set; }
    public int KitapSayfaSayisi { get; set; }
}
