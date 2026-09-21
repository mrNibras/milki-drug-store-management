using Microsoft.Extensions.Configuration;
using MilkiDrugStore.Persistence.Context;
using Xunit;

namespace MilkiDrugStore.Tests.Integration;

public class ProviderDetectionTests
{
    [Theory]
    [InlineData("postgres://user:pass@host:5432/dbname")]
    [InlineData("postgresql://user:pass@host:5432/dbname")]
    [InlineData("Host=localhost;Port=5432;Database=mydb;Username=postgres;Password=secret")]
    [InlineData("Host=pg-host;Port=5432;Database=testdb;Username=user;Password=pass;SslMode=Disable")]
    [InlineData("Host=server;Port=5432;Database=db;Username=u;Password=p;SslMode=Require")]
    public void ResolveConnectionString_PostgreSQL_Formats(string connectionString)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string?>("ConnectionStrings:DefaultConnection", connectionString)
            })
            .Build();

        var conn = DbProviderResolver.ResolveConnectionString(config);
        Assert.NotNull(conn);
        Assert.NotEmpty(conn);
    }

    [Fact]
    public void ResolveConnectionString_Empty_Returns_EmptyString()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(Array.Empty<KeyValuePair<string, string?>>())
            .Build();

        var conn = DbProviderResolver.ResolveConnectionString(config);
        Assert.Equal(string.Empty, conn);
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
                new KeyValuePair<string, string?>("ConnectionStrings:DefaultConnection", "Host=localhost;Port=5432;Database=db"),
                new KeyValuePair<string, string?>("DATABASE_URL", "postgres://user:pass@host:5432/dbname")
            })
            .Build();

        var conn = DbProviderResolver.ResolveConnectionString(config);
        Assert.Equal("Host=localhost;Port=5432;Database=db", conn);
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

    [Theory]
    [InlineData("postgres://user:password@localhost:5432/milki_drug_store")]
    [InlineData("postgresql://user:password@localhost:5432/milki_drug_store")]
    public void NormalizePostgresConnectionString_UrlFormat_ConvertsToKeyValue(string url)
    {
        var conn = DbProviderResolver.NormalizePostgresConnectionString(url);
        var dict = ParseConnectionString(conn);

        Assert.Equal("localhost", dict["Host"]);
        Assert.Equal("5432", dict["Port"]);
        Assert.Equal("milki_drug_store", dict["Database"]);
        Assert.Equal("user", dict["Username"]);
        Assert.Equal("password", dict["Password"]);
    }

    [Fact]
    public void NormalizePostgresConnectionString_KeyValueFormat_PassThrough()
    {
        var kv = "Host=localhost;Port=5432;Database=milki_drug_store;Username=user;Password=password";
        var conn = DbProviderResolver.NormalizePostgresConnectionString(kv);

        Assert.Equal(kv, conn);
    }

    [Theory]
    [InlineData("postgresql://us%40er:p%40ss@host:5432/mydb")]
    [InlineData("postgres://us%40er:p%40ss@host:5432/mydb")]
    public void NormalizePostgresConnectionString_UrlEncodedCredentials_Decodes(string url)
    {
        var conn = DbProviderResolver.NormalizePostgresConnectionString(url);
        var dict = ParseConnectionString(conn);

        Assert.Equal("us@er", dict["Username"]);
        Assert.Equal("p@ss", dict["Password"]);
        Assert.Equal("mydb", dict["Database"]);
    }

    [Theory]
    [InlineData("postgres://user:p%40ss%3Aw0rd@host:5432/db")]
    [InlineData("postgresql://user:p%40ss%3Aw0rd@host:5432/db")]
    public void NormalizePostgresConnectionString_PasswordWithSpecialChars_Decodes(string url)
    {
        var conn = DbProviderResolver.NormalizePostgresConnectionString(url);
        var dict = ParseConnectionString(conn);

        Assert.Equal("p@ss:w0rd", dict["Password"]);
    }

    [Theory]
    [InlineData("postgres://user:pass@host:5432/db?sslmode=Require")]
    [InlineData("postgresql://user:pass@host:5432/db?sslmode=Require")]
    public void NormalizePostgresConnectionString_WithSslMode_QueryParam_Parsed(string url)
    {
        var conn = DbProviderResolver.NormalizePostgresConnectionString(url);
        Assert.Contains("Ssl Mode=Require", conn, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void NormalizePostgresConnectionString_EmptyOrNull_ReturnsEmpty()
    {
        Assert.Equal(string.Empty, DbProviderResolver.NormalizePostgresConnectionString(string.Empty));
        Assert.Equal(string.Empty, DbProviderResolver.NormalizePostgresConnectionString(null!));
    }

    [Fact]
    public void ResolveConnectionString_UrlFromDefaultConnection_ConvertsToKeyValue()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string?>("ConnectionStrings:DefaultConnection", "postgresql://user:pass@host:5432/milki_drug_store")
            })
            .Build();

        var conn = DbProviderResolver.ResolveConnectionString(config);
        var dict = ParseConnectionString(conn);

        Assert.Equal("host", dict["Host"]);
        Assert.Equal("5432", dict["Port"]);
        Assert.Equal("milki_drug_store", dict["Database"]);
        Assert.Equal("user", dict["Username"]);
        Assert.Equal("pass", dict["Password"]);
    }

    private static Dictionary<string, string> ParseConnectionString(string cs)
    {
        return cs.Split(';', StringSplitOptions.RemoveEmptyEntries)
            .Select(p => p.Split('=', 2))
            .ToDictionary(p => p[0].Trim(), p => p[1].Trim(), StringComparer.OrdinalIgnoreCase);
    }
}
