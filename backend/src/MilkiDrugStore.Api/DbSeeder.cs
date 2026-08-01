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

            var adminEmail = "admin@milki.com";
            var existingAdmin = await context.Users.FirstOrDefaultAsync(u => u.Email == adminEmail);
            if (existingAdmin == null)
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
                    Email = adminEmail,
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
            else
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

                var needsUpdate = false;
                if (existingAdmin.RoleId != adminRole.RoleId)
                {
                    existingAdmin.RoleId = adminRole.RoleId;
                    needsUpdate = true;
                }
                if (existingAdmin.BranchId != mainBranch.BranchId)
                {
                    existingAdmin.BranchId = mainBranch.BranchId;
                    needsUpdate = true;
                }
                if (!existingAdmin.IsApproved)
                {
                    existingAdmin.IsApproved = true;
                    needsUpdate = true;
                }
                if (!existingAdmin.IsActive)
                {
                    existingAdmin.IsActive = true;
                    needsUpdate = true;
                }

                if (needsUpdate)
                {
                    await context.SaveChangesAsync();
                    logger.LogInformation("Default admin user updated successfully.");
                }
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
