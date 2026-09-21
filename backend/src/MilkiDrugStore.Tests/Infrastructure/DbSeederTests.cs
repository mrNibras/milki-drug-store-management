using Xunit;
using FluentAssertions;
using MilkiDrugStore.Api;
using MilkiDrugStore.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace MilkiDrugStore.Tests.Infrastructure;

public class DbSeederTests
{
    [Fact]
    public async Task SeedAsync_Should_Seed_Roles_When_Empty()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"DbSeeder_Test_{Guid.NewGuid()}")
            .Options;

        using (var context = new AppDbContext(options))
        {
            context.Database.EnsureCreated();
        }

        using (var context = new AppDbContext(options))
        {
            var logger = Mock.Of<ILogger>();
            await DbSeeder.SeedAsync(context, logger);
        }

        using (var context = new AppDbContext(options))
        {
            var roles = await context.Roles.ToListAsync();
            roles.Should().HaveCount(2);
            roles.Select(r => r.Name).Should().Contain("Admin", "Pharmacist");
        }
    }

    [Fact]
    public async Task SeedAsync_Should_Not_Duplicate_Data_When_Run_Twice()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"DbSeeder_Test_{Guid.NewGuid()}")
            .Options;

        using (var context = new AppDbContext(options))
        {
            context.Database.EnsureCreated();
        }

        using (var context = new AppDbContext(options))
        {
            var logger = Mock.Of<ILogger>();
            await DbSeeder.SeedAsync(context, logger);
        }

        using (var context2 = new AppDbContext(options))
        {
            var logger = Mock.Of<ILogger>();
            await DbSeeder.SeedAsync(context2, logger);
        }

        using (var context3 = new AppDbContext(options))
        {
            var roles = await context3.Roles.ToListAsync();
            roles.Should().HaveCount(2);
        }
    }
}
