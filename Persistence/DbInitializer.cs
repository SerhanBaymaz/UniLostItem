using System;
using Domain;
using Microsoft.AspNetCore.Identity;

namespace Persistence;

public static class DbInitializer
{
    public static async Task SeedData(AppDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        // Seed Roles
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new IdentityRole("Admin"));
        }
        if (!await roleManager.RoleExistsAsync("BaseUser"))
        {
            await roleManager.CreateAsync(new IdentityRole("BaseUser"));
        }

        // Seed Admin User
        if (await userManager.FindByEmailAsync("admin@admin.com") == null)
        {
            var admin = new ApplicationUser
            {
                UserName = "admin",
                Email = "admin@admin.com",
                FirstName = "System",
                LastName = "Admin",
                EmailConfirmed = true,
                CreatedDate = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(admin, "Pa$$w0rd");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, "Admin");
            }
        }

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
                ,
                new()
                {
                    KitapName = "Karamazov Kardeşler",
                    KitapYazar = "Fyodor Dostoyevski",
                    KitapSayfaSayisi = 824
                },
                new()
                {
                    KitapName = "Anna Karenina",
                    KitapYazar = "Lev Tolstoy",
                    KitapSayfaSayisi = 864
                },
                new()
                {
                    KitapName = "Madame Bovary",
                    KitapYazar = "Gustave Flaubert",
                    KitapSayfaSayisi = 329
                },
                new()
                {
                    KitapName = "Fahrenheit 451",
                    KitapYazar = "Ray Bradbury",
                    KitapSayfaSayisi = 194
                },
                new()
                {
                    KitapName = "Brave New World",
                    KitapYazar = "Aldous Huxley",
                    KitapSayfaSayisi = 268
                },
                new()
                {
                    KitapName = "The Catcher in the Rye",
                    KitapYazar = "J.D. Salinger",
                    KitapSayfaSayisi = 214
                },
                new()
                {
                    KitapName = "To Kill a Mockingbird",
                    KitapYazar = "Harper Lee",
                    KitapSayfaSayisi = 281
                },
                new()
                {
                    KitapName = "The Great Gatsby",
                    KitapYazar = "F. Scott Fitzgerald",
                    KitapSayfaSayisi = 180
                },
                new()
                {
                    KitapName = "Moby Dick",
                    KitapYazar = "Herman Melville",
                    KitapSayfaSayisi = 635
                },
                new()
                {
                    KitapName = "Yeraltından Notlar",
                    KitapYazar = "Fyodor Dostoyevski",
                    KitapSayfaSayisi = 88
                },
                new()
                {
                    KitapName = "Aşk",
                    KitapYazar = "Elif Şafak",
                    KitapSayfaSayisi = 408
                },
                new()
                {
                    KitapName = "Puslu Kıtalar Atlası",
                    KitapYazar = "İhsan Oktay Anar",
                    KitapSayfaSayisi = 584
                },
                new()
                {
                    KitapName = "Kürk Mantolu Madonna",
                    KitapYazar = "Sabahattin Ali",
                    KitapSayfaSayisi = 160
                },
                new()
                {
                    KitapName = "Sefiller",
                    KitapYazar = "Victor Hugo",
                    KitapSayfaSayisi = 1463
                },
                new()
                {
                    KitapName = "The Hobbit",
                    KitapYazar = "J.R.R. Tolkien",
                    KitapSayfaSayisi = 310
                },
                new()
                {
                    KitapName = "Lord of the Flies",
                    KitapYazar = "William Golding",
                    KitapSayfaSayisi = 224
                },
                new()
                {
                    KitapName = "One Hundred Years of Solitude",
                    KitapYazar = "Gabriel García Márquez",
                    KitapSayfaSayisi = 417
                },
                new()
                {
                    KitapName = "Walden",
                    KitapYazar = "Henry David Thoreau",
                    KitapSayfaSayisi = 224
                },
                new()
                {
                    KitapName = "The Road",
                    KitapYazar = "Cormac McCarthy",
                    KitapSayfaSayisi = 287
                },
                new()
                {
                    KitapName = "Beloved",
                    KitapYazar = "Toni Morrison",
                    KitapSayfaSayisi = 324
                }
            };

            context.SerhanKitaplar.AddRange(kitaplar);
            await context.SaveChangesAsync();
        }

    }
}