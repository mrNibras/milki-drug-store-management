using System.Reflection;
using Microsoft.EntityFrameworkCore;
using MilkiDrugStore.Persistence.Context;

var sourceConn = args.FirstOrDefault(a => a == "--source") is not null
    ? args[Array.IndexOf(args, "--source") + 1]
    : Environment.GetEnvironmentVariable("SOURCE_CONNECTION_STRING");

var targetConn = args.FirstOrDefault(a => a == "--target") is not null
    ? args[Array.IndexOf(args, "--target") + 1]
    : Environment.GetEnvironmentVariable("TARGET_CONNECTION_STRING");

if (string.IsNullOrWhiteSpace(sourceConn) || string.IsNullOrWhiteSpace(targetConn))
{
    Console.Error.WriteLine("Usage: MigrationTool --source <sqlite-connection> --target <postgres-connection>");
    Console.Error.WriteLine("   Or set SOURCE_CONNECTION_STRING and TARGET_CONNECTION_STRING env vars.");
    Environment.Exit(1);
}

Console.WriteLine("=== SQLite -> PostgreSQL Migration Tool ===");
Console.WriteLine($"Source: {MaskConnectionString(sourceConn)}");
Console.WriteLine($"Target: {MaskConnectionString(targetConn)}");

var sourceOptions = new DbContextOptionsBuilder<AppDbContext>();
DbProviderResolver.ConfigureAppDbContext(sourceOptions, DbProviderResolver.Sqlite, sourceConn);
using var sourceContext = new AppDbContext(sourceOptions.Options);

var targetOptions = new DbContextOptionsBuilder<AppDbContext>();
DbProviderResolver.ConfigureAppDbContext(targetOptions, DbProviderResolver.PostgreSql, targetConn);

Console.WriteLine("\n1. Applying migrations to target...");
await using (var targetContext = new AppDbContext(targetOptions.Options))
{
    await targetContext.Database.MigrateAsync();
    Console.WriteLine("   Migrations applied successfully.");

    Console.WriteLine("\n2. Disabling foreign key checks...");
    await targetContext.Database.ExecuteSqlRawAsync("SET session_replication_role = 'replica'");

    var entityTypes = targetContext.Model.GetEntityTypes()
        .Select(t => t.ClrType)
        .ToList();

    Console.WriteLine($"\n3. Migrating {entityTypes.Count} entity types...");

    foreach (var entityType in entityTypes)
    {
        try
        {
            var entities = LoadAllEntities(sourceContext, entityType);

            if (entities.Count == 0)
            {
                Console.WriteLine($"   {entityType.Name}: 0 rows (skipped)");
                continue;
            }

            foreach (var entity in entities)
                targetContext.Entry(entity).State = EntityState.Added;

            await targetContext.SaveChangesAsync();
            Console.WriteLine($"   {entityType.Name}: {entities.Count} rows migrated");

            targetContext.ChangeTracker.Clear();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   {entityType.Name}: ERROR - {ex.Message}");
            targetContext.ChangeTracker.Clear();
        }
    }

    Console.WriteLine("\n4. Re-enabling foreign key checks...");
    await targetContext.Database.ExecuteSqlRawAsync("SET session_replication_role = 'DEFAULT'");

    Console.WriteLine("\n5. Resetting identity sequences...");
    await ResetIdentitySequencesAsync(targetContext);

    Console.WriteLine("\n6. Verifying row counts...");
    var mismatches = 0;
    foreach (var entityType in entityTypes)
    {
        var sourceCount = CountAllEntities(sourceContext, entityType);
        var targetCount = CountAllEntities(targetContext, entityType);

        if (sourceCount != targetCount)
        {
            Console.WriteLine($"   MISMATCH {entityType.Name}: source={sourceCount}, target={targetCount}");
            mismatches++;
        }
    }

    if (mismatches == 0)
    {
        Console.WriteLine("\n=== Migration completed successfully! All row counts verified. ===");
    }
    else
    {
        Console.WriteLine($"\n=== Migration completed with {mismatches} mismatch(es). Review output above. ===");
    }
}

await sourceContext.DisposeAsync();

static List<object> LoadAllEntities(DbContext context, Type entityType)
{
    var setMethod = typeof(DbContext).GetMethods(BindingFlags.Public | BindingFlags.Instance)
        .FirstOrDefault(m => m.Name == "Set" && m.GetGenericArguments().Length == 1 && m.GetParameters().Length == 0);

    if (setMethod == null)
        throw new InvalidOperationException("Could not find DbContext.Set<T>() method");

    var genericMethod = setMethod.MakeGenericMethod(entityType);
    var dbSet = genericMethod.Invoke(context, null);

    return ((IQueryable)dbSet).Cast<object>().ToList();
}

static int CountAllEntities(DbContext context, Type entityType)
{
    var setMethod = typeof(DbContext).GetMethods(BindingFlags.Public | BindingFlags.Instance)
        .FirstOrDefault(m => m.Name == "Set" && m.GetGenericArguments().Length == 1 && m.GetParameters().Length == 0);

    if (setMethod == null)
        throw new InvalidOperationException("Could not find DbContext.Set<T>() method");

    var genericMethod = setMethod.MakeGenericMethod(entityType);
    var dbSet = genericMethod.Invoke(context, null);

    return ((IQueryable)dbSet).Cast<object>().Count();
}

static async Task ResetIdentitySequencesAsync(AppDbContext context)
{
    var tables = context.Model.GetEntityTypes()
        .Where(t => t.FindPrimaryKey() != null)
        .Select(t => new
        {
            TableName = t.GetTableName(),
            Schema = t.GetSchema(),
            KeyName = t.FindPrimaryKey()?.Properties[0].Name
        })
        .ToList();

    foreach (var table in tables)
    {
        var quotedTable = $"\"{table.TableName}\"";
        if (!string.IsNullOrEmpty(table.Schema))
            quotedTable = $"\"{table.Schema}\".\"{table.TableName}\"";

        var maxId = await GetMaxIdAsync(context, table.TableName, table.KeyName);
        var seqSql = $"ALTER TABLE {quotedTable} ALTER COLUMN \"{table.KeyName}\" RESTART WITH {maxId + 1}";

        try
        {
            await context.Database.ExecuteSqlRawAsync(seqSql);
            Console.WriteLine($"   Reset sequence for {table.TableName}.{table.KeyName}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   Failed to reset sequence for {table.TableName}: {ex.Message}");
        }
    }
}

static async Task<int> GetMaxIdAsync(AppDbContext context, string tableName, string keyName)
{
    try
    {
        var sql = $"SELECT COALESCE(MAX(\"{keyName}\"), 0) FROM \"{tableName}\"";
        var result = await context.Database.SqlQueryRaw<int>(sql).FirstOrDefaultAsync();
        return result;
    }
    catch
    {
        return 0;
    }
}

static string MaskConnectionString(string connectionString)
{
    if (string.IsNullOrWhiteSpace(connectionString)) return "(not configured)";
    if (connectionString.Contains("Password=", StringComparison.OrdinalIgnoreCase) ||
        connectionString.Contains("User Id=", StringComparison.OrdinalIgnoreCase))
    {
        var masked = connectionString;
        foreach (var key in new[] { "Password=", "User Id=" })
        {
            var idx = masked.IndexOf(key, StringComparison.OrdinalIgnoreCase);
            if (idx < 0) continue;
            var valueStart = idx + key.Length;
            var valueEnd = masked.IndexOf(';', valueStart);
            if (valueEnd < 0) valueEnd = masked.Length;
            if (valueEnd <= valueStart) continue;
            masked = masked.Remove(valueStart, valueEnd - valueStart).Insert(valueStart, "***");
        }
        return masked;
    }
    return connectionString;
}
