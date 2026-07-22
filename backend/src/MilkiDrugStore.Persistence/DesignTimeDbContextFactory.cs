using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using MilkiDrugStore.Persistence.Context;

namespace MilkiDrugStore.Persistence;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var basePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "MilkiDrugStore.Api");
        
        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection") 
                               ?? "Data Source=DesignTime.db";

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        
        if (connectionString.Contains("Server=", StringComparison.OrdinalIgnoreCase))
        {
            optionsBuilder.UseSqlServer(connectionString);
        }
        else
        {
            optionsBuilder.UseSqlite(connectionString);
        }

        return new AppDbContext(optionsBuilder.Options);
    }
}
