using System;
using Domain;
using Domain.Common.Enums;
using Microsoft.AspNetCore.Identity;

namespace Persistence;

public static class DbInitializer
{
    private const string AdminRole = "Admin";
    private const string BaseUserRole = "BaseUser";

    public static async Task SeedData(AppDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
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
                await userManager.AddToRoleAsync(admin, AdminRole);
            }
        }

        // Seed Test Admin User
        if (await userManager.FindByEmailAsync("test-admin@unilost.com") == null)
        {
            var testAdmin = new ApplicationUser
            {
                UserName = "test-admin",
                Email = "test-admin@unilost.com",
                FirstName = "AdminNameTest",
                LastName = "AdminSurnameTest",
                EmailConfirmed = true,
                CreatedDate = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(testAdmin, "Test1234!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(testAdmin, AdminRole);
            }
        }

        // Seed Test Base User
        if (await userManager.FindByEmailAsync("test-user@unilost.com") == null)
        {
            var testUser = new ApplicationUser
            {
                UserName = "test-user",
                Email = "test-user@unilost.com",
                FirstName = "UserNameTest",
                LastName = "UserSurnameTest",
                EmailConfirmed = true,
                CreatedDate = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(testUser, "Test1234!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(testUser, BaseUserRole);
            }
        }

        // Seed LostItems
        if (!context.LostItems.Any())
        {
            var testUser = await userManager.FindByEmailAsync("test-user@unilost.com");

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
                    ContactInfo = "serhan@uni.edu.tr",
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
                    ContactInfo = "Ahmet - 0555 123 4567",
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
                    UserId = testUser!.Id
                }
            };

            context.LostItems.AddRange(lostItems);
            await context.SaveChangesAsync();
        }

    }
}
