using Microsoft.Extensions.Configuration;
using MilkiDrugStore.Persistence.Context;
using Xunit;

namespace MilkiDrugStore.Tests.Integration;

public class ProviderDetectionTests
{
    [Theory]
    [InlineData("postgres://user:pass@host:5432/dbname", "postgresql")]
    [InlineData("postgresql://user:pass@host:5432/dbname", "postgresql")]
    [InlineData("Host=localhost;Port=5432;Database=mydb;Username=postgres;Password=secret", "postgresql")]
    [InlineData("Host=pg-host;Port=5432;Database=testdb;Username=user;Password=pass;SslMode=Disable", "postgresql")]
    [InlineData("Host=server;Port=5432;Database=db;Username=u;Password=p;SslMode=Require", "postgresql")]
    public void DetectProvider_PostgreSQL_Formats(string connectionString, string expected)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string?>("ConnectionStrings:DefaultConnection", connectionString)
            })
            .Build();

        var provider = DbProviderResolver.DetectProvider(config);
        Assert.Equal(expected, provider);
    }

    [Theory]
    [InlineData("Data Source=/var/data/test.db", "sqlite")]
    [InlineData("Data Source=test.db;Cache=Shared", "sqlite")]
    [InlineData("", "sqlite")]
    public void DetectProvider_SQLite_Formats(string connectionString, string expected)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string?>("ConnectionStrings:DefaultConnection", connectionString)
            })
            .Build();

        var provider = DbProviderResolver.DetectProvider(config);
        Assert.Equal(expected, provider);
    }

    [Theory]
    [InlineData("Server=localhost;Database=test;User Id=sa;Password=pass", "sqlserver")]
    [InlineData("Server=tcp:server.database.windows.net;Database=MyDb;User Id=user;Password=pass;TrustServerCertificate=true", "sqlserver")]
    public void DetectProvider_SQLServer_Formats(string connectionString, string expected)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string?>("ConnectionStrings:DefaultConnection", connectionString)
            })
            .Build();

        var provider = DbProviderResolver.DetectProvider(config);
        Assert.Equal(expected, provider);
    }

    [Fact]
    public void DetectProvider_ExplicitOverride()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string?>("DatabaseProvider", "sqlite"),
                new KeyValuePair<string, string?>("ConnectionStrings:DefaultConnection", "Host=localhost;Port=5432;Database=db")
            })
            .Build();

        var provider = DbProviderResolver.DetectProvider(config);
        Assert.Equal("sqlite", provider);
    }

    [Fact]
    public void ResolveConnectionString_DatabaseUrl()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string?>("DATABASE_URL", "postgres://user:pass@host:5432/dbname")
            })
            .Build();

        var conn = DbProviderResolver.ResolveConnectionString(config);
        Assert.NotNull(conn);
        Assert.Contains("Host=host", conn, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Port=5432", conn, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Database=dbname", conn, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Username=user", conn, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Password=pass", conn, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ResolveConnectionString_FallbackToDefaultConnection()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string?>("ConnectionStrings:DefaultConnection", "Data Source=/var/data/test.db"),
                new KeyValuePair<string, string?>("DATABASE_URL", "postgres://user:pass@host:5432/dbname")
            })
            .Build();

        var conn = DbProviderResolver.ResolveConnectionString(config);
        Assert.Equal("Data Source=/var/data/test.db", conn);
    }

    [Theory]
    [InlineData("Password=secret123;User Id=admin;Host=localhost", "Password=***masked***;User Id=***masked***;Host=localhost")]
    [InlineData("postgres://user:secret@host:5432/dbname", "postgres://user:***masked***@host:5432/dbname")]
    [InlineData("", "Not configured")]
    public void MaskConnectionString(string input, string expected)
    {
        var result = DbProviderResolver.MaskConnectionString(input);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void PostgresUrlToConnectionString_AllParameters()
    {
        var url = "postgres://myuser:mypassword@dbhost.example.com:5433/mydb?sslmode=Require";
        var conn = DbProviderResolver.PostgresUrlToConnectionString(url);

        var dict = ParseConnectionString(conn);
        Assert.Equal("dbhost.example.com", dict["Host"]);
        Assert.Equal("5433", dict["Port"]);
        Assert.Equal("mydb", dict["Database"]);
        Assert.Equal("myuser", dict["Username"]);
        Assert.Equal("mypassword", dict["Password"]);
    }

    private static Dictionary<string, string> ParseConnectionString(string cs)
    {
        return cs.Split(';', StringSplitOptions.RemoveEmptyEntries)
            .Select(p => p.Split('=', 2))
            .ToDictionary(p => p[0].Trim(), p => p[1].Trim(), StringComparer.OrdinalIgnoreCase);
    }
}
