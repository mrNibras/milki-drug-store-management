using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace MilkiDrugStore.Persistence.Context;

public static class DbProviderResolver
{
    public const string PostgreSql = "postgresql";
    public const string DefaultMigrationsAssembly = "MilkiDrugStore.Persistence";

    public static string ResolveConnectionString(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (!string.IsNullOrWhiteSpace(connectionString))
            return connectionString;

        var databaseUrl = configuration["DATABASE_URL"];
        if (!string.IsNullOrWhiteSpace(databaseUrl))
            return PostgresUrlToConnectionString(databaseUrl);

        return string.Empty;
    }

    /// <summary>
    /// Converts a standard "postgres://user:password@host:port/database" URL
    /// (as provided by Render) into an Npgsql connection string.
    /// </summary>
    public static string PostgresUrlToConnectionString(string databaseUrl)
    {
        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = "localhost",
            Database = "milki_drug_store"
        };

        var uri = new Uri(databaseUrl);
        builder.Host = uri.Host;
        builder.Port = uri.Port;
        builder.Database = uri.AbsolutePath.TrimStart('/');
        if (!string.IsNullOrEmpty(uri.UserInfo))
        {
            var parts = uri.UserInfo.Split(':', 2);
            builder.Username = Uri.UnescapeDataString(parts[0]);
            if (parts.Length > 1)
                builder.Password = Uri.UnescapeDataString(parts[1]);
        }

        try
        {
            if (!string.IsNullOrEmpty(uri.Query))
            {
                var queryParams = uri.Query.TrimStart('?')
                    .Split('&', StringSplitOptions.RemoveEmptyEntries)
                    .Select(kv => kv.Split('=', 2));
                var pairs = queryParams.Where(kv => kv.Length == 2)
                    .ToDictionary(kv => kv[0], kv => Uri.UnescapeDataString(kv[1]), StringComparer.OrdinalIgnoreCase);
                if (pairs.TryGetValue("sslmode", out var sslMode) && !string.IsNullOrWhiteSpace(sslMode))
                {
                    builder.SslMode = (SslMode)Enum.Parse(typeof(SslMode), sslMode, ignoreCase: true);
                }
            }
        }
        catch
        {
            // Ignore malformed query strings; default TLS behavior applies.
        }

        return builder.ConnectionString;
    }

    public static void ConfigureAppDbContext(
        DbContextOptionsBuilder options,
        IConfiguration configuration)
    {
        var connectionString = ResolveConnectionString(configuration);
        options.UseNpgsql(connectionString, npgsql =>
        {
            npgsql.MigrationsAssembly(DefaultMigrationsAssembly);
        });
    }

    public static string MaskConnectionString(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            return "Not configured";

        string masked = connectionString;
        foreach (var key in new[] { "Password=", "User Id=", "UserID=" })
        {
            var idx = masked.IndexOf(key, StringComparison.OrdinalIgnoreCase);
            if (idx < 0) continue;
            var valueStart = idx + key.Length;
            var valueEnd = masked.IndexOf(';', valueStart);
            if (valueEnd < 0) valueEnd = masked.Length;
            if (valueEnd <= valueStart) continue;
            masked = masked.Remove(valueStart, valueEnd - valueStart)
                           .Insert(valueStart, "***masked***");
        }

        if (masked.Contains("@", StringComparison.Ordinal) &&
            (masked.Contains("://", StringComparison.Ordinal)))
        {
            masked = System.Text.RegularExpressions.Regex.Replace(
                masked, @"(://[^:/?]+:)[^@/]+(@)", "$1***masked***$2");
        }

        return masked;
    }
}
