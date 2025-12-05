using System;
using Domain;

namespace Persistence;

public static class DbInitializer
{
    public static async Task SeedData(AppDbContext context)
    {
        // Seed SerhanKitaplar
        if (!context.SerhanKitaplar.Any())
        {
            var kitaplar = new List<SerhanKitap>
            {
                new()
                {
                    KitapName = "Suç ve Ceza",
                    KitapYazar = "Fyodor Dostoyevski",
                    KitapSayfaSayisi = 671
                },
                new()
                {
                    KitapName = "1984",
                    KitapYazar = "George Orwell",
                    KitapSayfaSayisi = 352
                },
                new()
                {
                    KitapName = "Simyacı",
                    KitapYazar = "Paulo Coelho",
                    KitapSayfaSayisi = 184
                },
                new()
                {
                    KitapName = "Küçük Prens",
                    KitapYazar = "Antoine de Saint-Exupéry",
                    KitapSayfaSayisi = 96
                },
                new()
                {
                    KitapName = "Savaş ve Barış",
                    KitapYazar = "Lev Tolstoy",
                    KitapSayfaSayisi = 1225
                },
                new()
                {
                    KitapName = "İnce Memed",
                    KitapYazar = "Yaşar Kemal",
                    KitapSayfaSayisi = 420
                },
                new()
                {
                    KitapName = "Tutunamayanlar",
                    KitapYazar = "Oğuz Atay",
                    KitapSayfaSayisi = 724
                }
            };

            context.SerhanKitaplar.AddRange(kitaplar);
            await context.SaveChangesAsync();
        }

    }
}
