using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace MilkiDrugStore.Persistence.Context;

/// <summary>
/// Resolves which database provider is in use for a given configuration and
/// configures the <see cref="AppDbContext"/> accordingly.
///
/// Resolution order:
///   1. Explicit override via the "DatabaseProvider" configuration key
///      (values: postgres/postgresql/npgsql, sqlserver/mssql/sql, sqlite).
///   2. The resolved connection string markers:
///        - Host=, Port=5432, Username=, postgres:// or postgresql://  => PostgreSQL
///        - Server=                                                    => SQL Server
///        - everything else                                            => SQLite
///   3. Empty connection string defaults to SQLite.
///
/// A Render-provided "DATABASE_URL" is honoured when no explicit
/// ConnectionStrings:DefaultConnection is configured.
/// </summary>
public static class DbProviderResolver
{
    public const string Sqlite = "sqlite";
    public const string SqlServer = "sqlserver";
    public const string PostgreSql = "postgresql";

    public const string PostgresMigrationsAssembly = "MilkiDrugStore.Persistence.Postgres";
    public const string DefaultMigrationsAssembly = "MilkiDrugStore.Persistence";

    public static string DetectProvider(IConfiguration configuration)
    {
        var overrideValue = configuration["DatabaseProvider"];
        if (!string.IsNullOrWhiteSpace(overrideValue))
        {
            return overrideValue.Trim().ToLowerInvariant() switch
            {
                "postgres" or "postgresql" or "npgsql" => PostgreSql,
                "sqlserver" or "mssql" or "sql" => SqlServer,
                "sqlite" or "sqlite3" or "sqliteprovider" => Sqlite,
                _ => PostgreSql
            };
        }

        var connectionString = ResolveConnectionString(configuration);
        if (string.IsNullOrWhiteSpace(connectionString))
            return Sqlite;

        if (LooksLikePostgres(connectionString))
            return PostgreSql;

        if (connectionString.Contains("Server=", StringComparison.OrdinalIgnoreCase))
            return SqlServer;

        return Sqlite;
    }

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

    public static bool LooksLikePostgres(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            return false;

        return connectionString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) ||
               connectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase) ||
               connectionString.Contains("Host=", StringComparison.OrdinalIgnoreCase) ||
               connectionString.Contains("Port=5432", StringComparison.OrdinalIgnoreCase) ||
               connectionString.Contains("Username=", StringComparison.OrdinalIgnoreCase) ||
               connectionString.Contains("SslMode=", StringComparison.OrdinalIgnoreCase);
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

        // Render managed PostgreSQL enforces TLS; honor an "sslmode" query
        // parameter on the URL if present.
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

    public static void ConfigureAppDbContext(DbContextOptionsBuilder options, IConfiguration configuration)
    {
        var connectionString = ResolveConnectionString(configuration);
        var provider = DetectProvider(configuration);
        ConfigureAppDbContext(options, provider, connectionString);
    }

    public static void ConfigureAppDbContext(
        DbContextOptionsBuilder options,
        string provider,
        string connectionString)
    {
        switch (provider)
        {
            case PostgreSql:
                options.UseNpgsql(connectionString, npgsql =>
                {
                    npgsql.MigrationsAssembly(PostgresMigrationsAssembly);
                    npgsql.EnableRetryOnFailure(3);
                });
                break;
            case SqlServer:
                options.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(DefaultMigrationsAssembly));
                break;
            default:
                options.UseSqlite(connectionString, sql => sql.MigrationsAssembly(DefaultMigrationsAssembly));
                break;
        }
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