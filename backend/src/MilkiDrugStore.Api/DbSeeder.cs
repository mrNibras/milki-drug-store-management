using MilkiDrugStore.Domain.Entities;
using MilkiDrugStore.Domain.Enums;
using MilkiDrugStore.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MilkiDrugStore.Api;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext context, ILogger logger)
    {
        try
        {
            logger.LogInformation("Starting database seeding...");

            if (!context.Roles.Any())
            {
                context.Roles.AddRange(
                    new Role { Name = "Admin" },
                    new Role { Name = "Pharmacist" }
                );
                await context.SaveChangesAsync();
                logger.LogInformation("Roles seeded successfully.");
            }

            if (!context.Users.Any())
            {
                var adminRole = context.Roles.First(r => r.Name == "Admin");
                var mainBranch = context.Branches.FirstOrDefault(b => b.BranchName == "Main Branch");
                if (mainBranch == null)
                {
                    mainBranch = new Branch { BranchName = "Main Branch", Location = "Main Store", IsActive = true, CreatedAt = DateTime.Now };
                    context.Branches.Add(mainBranch);
                    await context.SaveChangesAsync();
                    logger.LogInformation("Main Branch created.");
                }

                context.Users.Add(new User
                {
                    FullName = "System Admin",
                    Email = "admin@milki.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123"),
                    RoleId = adminRole.RoleId,
                    BranchId = mainBranch.BranchId,
                    IsApproved = true,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                });
                await context.SaveChangesAsync();
                logger.LogInformation("Default admin user seeded successfully.");
            }

            if (!context.UnitTypes.Any())
            {
                context.UnitTypes.AddRange(
                    new UnitType { Name = "Tablet", Description = "Solid dosage form" },
                    new UnitType { Name = "Capsule", Description = "Gelatinous shell" },
                    new UnitType { Name = "Bottle", Description = "Liquid container" },
                    new UnitType { Name = "Tube", Description = "Ointment/cream tube" },
                    new UnitType { Name = "Piece", Description = "Single item" },
                    new UnitType { Name = "Strip", Description = "Blister strip" },
                    new UnitType { Name = "Box", Description = "Box packaging" },
                    new UnitType { Name = "Packet", Description = "Packet packaging" },
                    new UnitType { Name = "Vial", Description = "Injectable vial" },
                    new UnitType { Name = "Injection", Description = "Injectable form" },
                    new UnitType { Name = "Cream", Description = "Topical cream" },
                    new UnitType { Name = "Drops", Description = "Eye/ear drops" },
                    new UnitType { Name = "Inhaler", Description = "Inhalation device" },
                    new UnitType { Name = "Patch", Description = "Transdermal patch" },
                    new UnitType { Name = "Syrup", Description = "Oral liquid" }
                );
                await context.SaveChangesAsync();
                logger.LogInformation("Unit types seeded successfully.");
            }

            if (!context.Categories.Any())
            {
                var tablet = await context.UnitTypes.FirstAsync(u => u.Name == "Tablet");
                var capsule = await context.UnitTypes.FirstAsync(u => u.Name == "Capsule");
                var bottle = await context.UnitTypes.FirstAsync(u => u.Name == "Bottle");
                var tube = await context.UnitTypes.FirstAsync(u => u.Name == "Tube");
                var piece = await context.UnitTypes.FirstAsync(u => u.Name == "Piece");

                context.Categories.AddRange(
                    new Category { Name = "Antibiotic", UnitTypeId = tablet.UnitTypeId },
                    new Category { Name = "Pain Killer", UnitTypeId = tablet.UnitTypeId },
                    new Category { Name = "Vitamin", UnitTypeId = tablet.UnitTypeId },
                    new Category { Name = "Respiratory", UnitTypeId = capsule.UnitTypeId },
                    new Category { Name = "Dermatology", UnitTypeId = tube.UnitTypeId },
                    new Category { Name = "Cosmetics", UnitTypeId = tube.UnitTypeId },
                    new Category { Name = "Baby Supplies", UnitTypeId = piece.UnitTypeId }
                );
                await context.SaveChangesAsync();
                logger.LogInformation("Categories seeded successfully.");
            }

            if (!context.Settings.Any())
            {
                var mainBranch = await context.Branches.FirstOrDefaultAsync(b => b.BranchName == "Main Branch");
                if (mainBranch == null)
                {
                    mainBranch = new Branch { BranchName = "Main Branch", Location = "Main Store", IsActive = true, CreatedAt = DateTime.Now };
                    context.Branches.Add(mainBranch);
                    await context.SaveChangesAsync();
                }

                context.Settings.Add(new Settings
                {
                    BranchId = mainBranch.BranchId,
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
                logger.LogInformation("Settings seeded successfully.");
            }

            logger.LogInformation("Database seeding completed successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }
}
