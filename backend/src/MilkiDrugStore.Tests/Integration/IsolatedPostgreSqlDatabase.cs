using Microsoft.EntityFrameworkCore;
using MilkiDrugStore.Persistence.Context;
using Npgsql;
using Xunit;

namespace MilkiDrugStore.Tests.Integration;

/// <summary>
/// A dedicated PostgreSQL database for a single test class, so the class does not
/// share schema with the other PostgreSQL suites (which drop and re-create tables
/// while running in parallel).
///
/// Uses the same Testcontainers/external-server source as
/// <see cref="PostgreSqlFixture"/>, but owns its own database name.
/// </summary>
public class IsolatedPostgreSqlDatabase : IAsyncLifetime
{
    private readonly PostgreSqlFixture _fixture = new();
    private string _databaseName = string.Empty;

    /// <summary>Connection string pointing at this class's own database.</summary>
    public string ConnectionString { get; private set; } = string.Empty;

    /// <summary>Connection string for the maintenance database, used to create/drop databases.</summary>
    public NpgsqlConnectionStringBuilder AdminConnectionStringBuilder { get; private set; } = new();

    public async Task InitializeAsync()
    {
        await _fixture.InitializeAsync();

        _databaseName = "damage_expiry_" + Guid.NewGuid().ToString("N").Substring(0, 16);
        AdminConnectionStringBuilder = new NpgsqlConnectionStringBuilder(_fixture.ConnectionString)
        {
            Database = "postgres"
        };
        var dbCs = new NpgsqlConnectionStringBuilder(_fixture.ConnectionString) { Database = _databaseName };
        ConnectionString = dbCs.ConnectionString;

        // The database name is generated here, so quoting is safe.
        var quotedName = "\"" + _databaseName + "\"";
        await using var admin = new NpgsqlConnection(AdminConnectionStringBuilder.ConnectionString);
        await admin.OpenAsync();
        await using var create = new NpgsqlCommand("CREATE DATABASE " + quotedName, admin);
        await create.ExecuteNonQueryAsync();
    }

    public async Task DisposeAsync()
    {
        NpgsqlConnection.ClearAllPools();
        var quotedName = "\"" + _databaseName + "\"";
        try
        {
            await using var admin = new NpgsqlConnection(AdminConnectionStringBuilder.ConnectionString);
            await admin.OpenAsync();
            await using var drop = new NpgsqlCommand("DROP DATABASE IF EXISTS " + quotedName + " WITH (FORCE)", admin);
            await drop.ExecuteNonQueryAsync();
        }
        catch (NpgsqlException)
        {
            // Best effort: the server may already be gone.
        }

        await _fixture.DisposeAsync();
    }
}
