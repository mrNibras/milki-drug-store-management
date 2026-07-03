using MilkiDrugStore.Domain.Entities;
using MilkiDrugStore.Domain.Enums;
using MilkiDrugStore.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace MilkiDrugStore.Api;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        await context.Database.EnsureCreatedAsync();

        if (!context.Roles.Any())
        {
            context.Roles.AddRange(
                new Role { Name = "Admin" },
                new Role { Name = "Pharmacist" }
            );
            await context.SaveChangesAsync();
        }

        if (!context.Users.Any())
        {
            var adminRole = context.Roles.First(r => r.Name == "Admin");
            context.Users.Add(new User
            {
                FullName = "System Admin",
                Email = "admin@milki.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123"),
                RoleId = adminRole.RoleId,
                IsApproved = true,
                IsActive = true,
                CreatedAt = DateTime.Now
            });
            await context.SaveChangesAsync();
        }

        if (!context.Categories.Any())
        {
            context.Categories.AddRange(
                new Category { Name = "Antibiotic" },
                new Category { Name = "Pain Killer" },
                new Category { Name = "Vitamin" },
                new Category { Name = "Respiratory" },
                new Category { Name = "Dermatology" },
                new Category { Name = "Cosmetics" },
                new Category { Name = "Baby Supplies" }
            );
            await context.SaveChangesAsync();
        }

        if (!context.Settings.Any())
        {
            context.Settings.Add(new Settings
            {
                PharmacyName = "Milki Drug Store",
                Address = "",
                Phone = "",
                Email = "",
                Language = "English",
                LowStockThreshold = 10,
                ExpiryAlertMonths = 6,
                Currency = "ETB"
            });
            await context.SaveChangesAsync();
        }
    }
}
