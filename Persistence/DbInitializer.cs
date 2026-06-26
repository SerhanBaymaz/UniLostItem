using System;
using Domain;
using Domain.Common.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Persistence;

public static class DbInitializer
{
    private const string AdminRole = "Admin";
    private const string BaseUserRole = "BaseUser";
    private const string TestUserEmail = "ahmetkuyuldar@gmail.com";

    public static async Task SeedData(AppDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
    {
        // Seed Roles
        if (!await roleManager.RoleExistsAsync(AdminRole))
        {
            await roleManager.CreateAsync(new IdentityRole(AdminRole));
        }
        if (!await roleManager.RoleExistsAsync(BaseUserRole))
        {
            await roleManager.CreateAsync(new IdentityRole(BaseUserRole));
        }

        // Seed Admin User
        if (await userManager.FindByEmailAsync("admin@admin.com") == null)
        {
            var admin = new ApplicationUser
            {
                UserName = "admin@admin.com",
                Email = "admin@admin.com",
                FirstName = "System",
                LastName = "Admin",
                EmailConfirmed = true,
                CreatedDate = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(admin, "Admin.1234");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, AdminRole);
            }
        }


        // Seed Test Base User
        if (await userManager.FindByEmailAsync(TestUserEmail) == null)
        {
            var testUser = new ApplicationUser
            {
                UserName = TestUserEmail,
                Email = TestUserEmail,
                FirstName = "Ahmet",
                LastName = "Kuyuldar",
                EmailConfirmed = true,
                CreatedDate = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(testUser, "Ahmet.1234");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(testUser, BaseUserRole);
            }
        }

        // Seed LostItems
        if (!await context.LostItems.AnyAsync())
        {
            var testUser = await userManager.FindByEmailAsync(TestUserEmail);

            var iphoneImage = configuration["SeedData:Images:iPhone"];
            var galaxyBudsImage = configuration["SeedData:Images:GalaxyBuds"];
            var studentIdImage = configuration["SeedData:Images:StudentId"];
            var nikeBagImage = configuration["SeedData:Images:NikeBag"];

            var lostItems = new List<LostItem>
            {
                new()
                {
                    Title = "iPhone 15 Pro Max",
                    Description = "Siyah renk, kılıfı yok. Kütüphane civarında düşmüş olabilir.",
                    Category = ItemCategory.Electronics,
                    ItemType = ItemType.Lost,
                    Status = ItemStatus.Active,
                    IncidentDate = DateTime.UtcNow.AddDays(-1),
                    LocationLabel = "Kütüphane B Blok",
                    Latitude = 41.0082,
                    Longitude = 28.9784,
                    ContactInfo = "05554443322",
                    ImageUrl = iphoneImage,
                    UserId = testUser!.Id
                },
                new()
                {
                    Title = "Samsung Galaxy Buds",
                    Description = "Beyaz kulaklık, şarj kutusu ile birlikte.",
                    Category = ItemCategory.Electronics,
                    ItemType = ItemType.Found,
                    Status = ItemStatus.Active,
                    IncidentDate = DateTime.UtcNow.AddDays(-2),
                    LocationLabel = "Yemekhane Giriş",
                    Latitude = 41.0090,
                    Longitude = 28.9795,
                    ContactInfo = "0555 123 4567",
                    ImageUrl = galaxyBudsImage,
                    UserId = testUser!.Id
                },
                new()
                {
                    Title = "Öğrenci Kimliği",
                    Description = "Mühendislik Fakültesi öğrenci kimliği, ad soyad görünmüyor.",
                    Category = ItemCategory.IdentificationCard,
                    ItemType = ItemType.Lost,
                    Status = ItemStatus.Active,
                    IncidentDate = DateTime.UtcNow.AddDays(-3),
                    LocationLabel = "Spor Salonu",
                    Latitude = 41.0100,
                    Longitude = 28.9800,
                    ContactInfo = "İletişim için Öğrenci İşleri",
                    ImageUrl = studentIdImage,
                    UserId = testUser!.Id
                },
                new()
                {
                    Title = "Siyah Sırt Çantası",
                    Description = "Nike marka, içinde dizüstü bilgisayar bulunabilir.",
                    Category = ItemCategory.BagWallet,
                    ItemType = ItemType.Found,
                    Status = ItemStatus.Active,
                    IncidentDate = DateTime.UtcNow,
                    LocationLabel = "Otobüs Durağı",
                    Latitude = 41.0075,
                    Longitude = 28.9760,
                    ContactInfo = "Güvenlikdeki abi - 05559876543",
                    ImageUrl = nikeBagImage,
                    UserId = testUser!.Id
                }
            };

            context.LostItems.AddRange(lostItems);
            await context.SaveChangesAsync();
        }

    }
}
