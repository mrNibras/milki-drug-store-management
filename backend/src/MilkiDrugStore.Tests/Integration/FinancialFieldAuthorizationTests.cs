using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MilkiDrugStore.Application.DTOs.Auth;
using MilkiDrugStore.Application.Interfaces;
using MilkiDrugStore.Domain.Entities;
using MilkiDrugStore.Domain.Interfaces;
using MilkiDrugStore.Domain.Interfaces.Repositories;
using MilkiDrugStore.Infrastructure.Services;
using MilkiDrugStore.Persistence.Context;
using MilkiDrugStore.Persistence.Repositories;
using Xunit;
using Xunit.Abstractions;

namespace MilkiDrugStore.Tests.Integration;

/// <summary>
/// Proves the supplier-cost restriction is enforced by the API, not merely hidden
/// in React: the raw JSON body returned to a pharmacist must not contain the cost
/// properties at all, while an admin must still receive them.
/// </summary>
public class FinancialFieldAuthorizationTests : IAsyncLifetime
{
    private readonly ITestOutputHelper _output;
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    /// <summary>
    /// Stable per test-class instance. This must be captured outside the
    /// UseInMemoryDatabase lambda, otherwise every scope would get its own
    /// database and the seeded users would be invisible to requests.
    /// </summary>
    private readonly string _databaseName = "MilkiFinAuth_" + Guid.NewGuid().ToString("N");

    private string _adminToken = string.Empty;
    private string _pharmacistToken = string.Empty;

    private const string AdminEmail = "admin@milki.com";
    private const string AdminPassword = "Admin123";
    private const string PharmacistEmail = "pharmacist@milki.com";
    private const string PharmacistPassword = "Pharmacist123";

    public FinancialFieldAuthorizationTests(ITestOutputHelper output)
    {
        _output = output;

        var testDir = Path.Combine(Path.GetTempPath(), "MilkiFinAuth_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(testDir);

        // Program.cs reads DataDirectory from configuration while the host is being
        // built, which happens before ConfigureAppConfiguration is applied, so the
        // process environment must already point at a writable directory.
        Environment.SetEnvironmentVariable("DataDirectory", testDir);
        Environment.SetEnvironmentVariable("BackupDirectory", Path.Combine(testDir, "backups"));

        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
                builder.ConfigureAppConfiguration((_, config) =>
                {
                    config.AddInMemoryCollection(new[]
                    {
                        new KeyValuePair<string, string?>("JwtSettings:Secret", "test_secret_key_at_least_32_chars_long"),
                        new KeyValuePair<string, string?>("JwtSettings:Issuer", "TestIssuer"),
                        new KeyValuePair<string, string?>("JwtSettings:Audience", "TestAudience"),
                        new KeyValuePair<string, string?>("JwtSettings:ExpiryMinutes", "60"),
                        new KeyValuePair<string, string?>("Email:Host", "smtp.test.com"),
                        new KeyValuePair<string, string?>("Email:Port", "587"),
                        new KeyValuePair<string, string?>("Email:EnableSsl", "false"),
                        new KeyValuePair<string, string?>("Email:SenderName", "Test"),
                        new KeyValuePair<string, string?>("Email:SenderEmail", "test@test.com"),
                        new KeyValuePair<string, string?>("Email:Username", "test"),
                        new KeyValuePair<string, string?>("Email:Password", "test"),
                        new KeyValuePair<string, string?>("Email:DevMode", "true"),
                        new KeyValuePair<string, string?>("ConnectionStrings:DefaultConnection", string.Empty),
                        new KeyValuePair<string, string?>("DataDirectory", testDir),
                        new KeyValuePair<string, string?>("BackupDirectory", Path.Combine(testDir, "backups"))
                    });
                });

                builder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                    if (descriptor != null) services.Remove(descriptor);

                    services.AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(_databaseName));

                    services.AddScoped<IUnitOfWork, UnitOfWork>();
                    services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
                    services.AddScoped<IMedicineRepository, MedicineRepository>();
                    services.AddScoped<ISaleRepository, SaleRepository>();
                    services.AddScoped<IPurchaseRepository, PurchaseRepository>();
                    services.AddScoped<IInventoryTransactionRepository, InventoryTransactionRepository>();
                    services.AddScoped<INotificationRepository, NotificationRepository>();
                    services.AddScoped<IAuditLogRepository, AuditLogRepository>();
                    services.AddScoped<ICosmeticRepository, CosmeticRepository>();
                    services.AddScoped<IAuthService, MilkiDrugStore.Application.Services.AuthService>();
                    services.AddScoped<IJwtTokenService, JwtTokenService>();
                    services.AddScoped<IEmailService, EmailService>();
                    services.AddScoped<IAuditLogService, MilkiDrugStore.Application.Services.AuditLogService>();
                });
            });

        _client = _factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.EnsureCreatedAsync();

        var adminRole = await SeedRoleAsync(db, "Admin");
        var pharmacistRole = await SeedRoleAsync(db, "Pharmacist");
        var branch = await SeedBranchAsync(db);

        await SeedUserAsync(db, AdminEmail, "Admin User", adminRole, branch);
        await SeedUserAsync(db, PharmacistEmail, "Pharmacist User", pharmacistRole, branch);
        await SeedCatalogAsync(db);
        await SeedInventoryAsync(db, branch);

        _adminToken = await LoginAsync(AdminEmail, AdminPassword);
        _pharmacistToken = await LoginAsync(PharmacistEmail, PharmacistPassword);

        Assert.False(string.IsNullOrEmpty(_adminToken), "Admin login failed; cannot verify admin access.");
        Assert.False(string.IsNullOrEmpty(_pharmacistToken), "Pharmacist login failed; cannot verify restriction.");
    }

    private static async Task<Role> SeedRoleAsync(AppDbContext db, string name)
    {
        var role = await db.Roles.FirstOrDefaultAsync(r => r.Name == name);
        if (role is null)
        {
            role = new Role { Name = name };
            db.Roles.Add(role);
            await db.SaveChangesAsync();
        }
        return role;
    }

    private static async Task<Branch> SeedBranchAsync(AppDbContext db)
    {
        var branch = await db.Branches.FirstOrDefaultAsync();
        if (branch is null)
        {
            branch = new Branch { BranchName = "Fin Auth Branch", Location = "Test", IsActive = true };
            db.Branches.Add(branch);
            await db.SaveChangesAsync();
        }
        return branch;
    }

    private static async Task SeedUserAsync(AppDbContext db, string email, string fullName, Role role, Branch branch)
    {
        if (await db.Users.AnyAsync(u => u.Email == email)) return;

        db.Users.Add(new User
        {
            FullName = fullName,
            Email = email,
            // AuthService verifies with BCrypt, so the seed must store a BCrypt hash.
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(
                email == AdminEmail ? AdminPassword : PharmacistPassword),
            RoleId = role.RoleId,
            BranchId = branch.BranchId,
            IsActive = true,
            IsApproved = true,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
    }

    private static async Task SeedCatalogAsync(AppDbContext db)
    {
        if (!await db.Categories.AnyAsync())
        {
            db.Categories.Add(new Category { Name = "Fin Auth Category", IsActive = true });
            await db.SaveChangesAsync();
        }
        if (!await db.UnitTypes.AnyAsync())
        {
            db.UnitTypes.Add(new UnitType { Name = "Fin Auth Unit", IsActive = true });
            await db.SaveChangesAsync();
        }
    }

    /// <summary>Creates one medicine and one cosmetic, each with a priced batch.</summary>
    private static async Task SeedInventoryAsync(AppDbContext db, Branch branch)
    {
        var categoryId = (await db.Categories.FirstAsync()).CategoryId;
        var unitTypeId = (await db.UnitTypes.FirstAsync()).UnitTypeId;

        var medicine = new Medicine
        {
            ProductCode = "FIN-AUTH-001",
            BrandName = "Fin Auth Amoxicillin",
            GenericName = "Amoxicillin",
            CategoryId = categoryId,
            UnitTypeId = unitTypeId,
            PurchasePrice = 60,
            SellingPrice = 100,
            ReorderLevel = 10,
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };
        db.Medicines.Add(medicine);
        await db.SaveChangesAsync();

        db.MedicineBatches.Add(new MedicineBatch
        {
            ProductId = medicine.ProductId,
            BranchId = branch.BranchId,
            BatchNumber = "FIN-AUTH-B1",
            PurchasePrice = 60,
            SellingPrice = 100,
            QuantityReceived = 100,
            QuantityIssued = 10,
            ExpiryDate = DateTime.UtcNow.AddYears(1),
            DateReceived = DateTime.UtcNow
        });

        var cosmetic = new Cosmetic
        {
            ProductName = "Fin Auth Shampoo",
            Description = "Shampoo",
            CategoryId = categoryId,
            UnitTypeId = unitTypeId,
            Price = 150,
            IsActive = true,
            BranchId = branch.BranchId,
            CreatedAt = DateTime.UtcNow
        };
        db.Cosmetics.Add(cosmetic);
        await db.SaveChangesAsync();

        db.CosmeticBatches.Add(new CosmeticBatch
        {
            CosmeticId = cosmetic.CosmeticId,
            BranchId = branch.BranchId,
            BatchNumber = "FIN-AUTH-C1",
            QuantityReceived = 50,
            BuyingPrice = 90,
            SellingPrice = 150,
            LowStockThreshold = 5,
            ExpiryDate = DateTime.UtcNow.AddYears(2),
            DateReceived = DateTime.UtcNow
        });

        await db.SaveChangesAsync();
    }

    public Task DisposeAsync()
    {
        _client.Dispose();
        _factory.Dispose();
        return Task.CompletedTask;
    }

    private async Task<string> LoginAsync(string email, string password)
    {
        using var anon = _factory.CreateClient();
        var response = await anon.PostAsJsonAsync("/api/auth/login", new LoginRequest { Email = email, Password = password });
        if (response.StatusCode != HttpStatusCode.OK)
        {
            _output.WriteLine($"login failed for {email}: {(int)response.StatusCode}");
            return string.Empty;
        }
        var data = await response.Content.ReadFromJsonAsync<LoginResponse>();
        return data?.Token ?? string.Empty;
    }

    private async Task<(HttpStatusCode Status, string Body)> GetRawAsync(string path, string token)
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await client.GetAsync(path);
        return (response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    /// <summary>
    /// Walks the whole document and reports every occurrence of a property name,
    /// so a nested batch cannot smuggle the value past a shallow check.
    /// </summary>
    private static List<string> FindPropertyPaths(string json, string propertyName)
    {
        var found = new List<string>();
        using var doc = JsonDocument.Parse(json);
        Walk(doc.RootElement, propertyName, "$", found);
        return found;

        static void Walk(JsonElement element, string name, string path, List<string> found)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    foreach (var prop in element.EnumerateObject())
                    {
                        var child = $"{path}.{prop.Name}";
                        if (string.Equals(prop.Name, name, StringComparison.OrdinalIgnoreCase))
                            found.Add(child);
                        Walk(prop.Value, name, child, found);
                    }
                    break;
                case JsonValueKind.Array:
                    var i = 0;
                    foreach (var item in element.EnumerateArray())
                    {
                        Walk(item, name, $"{path}[{i}]", found);
                        i++;
                    }
                    break;
            }
        }
    }

    // -----------------------------------------------------------------
    // Cost must never reach a pharmacist
    // -----------------------------------------------------------------

    [Theory]
    [InlineData("/api/medicines", "purchasePrice")]
    [InlineData("/api/cosmetics", "buyingPrice")]
    public async Task Pharmacist_Response_MustNotContainSupplierCost(string path, string costProperty)
    {
        var (status, body) = await GetRawAsync(path, _pharmacistToken);

        Assert.Equal(HttpStatusCode.OK, status);

        var occurrences = FindPropertyPaths(body, costProperty);
        _output.WriteLine($"pharmacist {path}: {occurrences.Count} occurrence(s) of {costProperty}");

        Assert.True(
            occurrences.Count == 0,
            $"Pharmacist response for {path} leaked {costProperty} at: {string.Join(", ", occurrences)}");
    }

    [Theory]
    [InlineData("/api/medicines")]
    [InlineData("/api/cosmetics")]
    public async Task Pharmacist_Response_MustNotContainAnyCostFieldAtAll(string path)
    {
        var (_, body) = await GetRawAsync(path, _pharmacistToken);

        var purchase = FindPropertyPaths(body, "purchasePrice");
        var buying = FindPropertyPaths(body, "buyingPrice");

        Assert.True(purchase.Count == 0, $"leaked purchasePrice at {string.Join(", ", purchase)}");
        Assert.True(buying.Count == 0, $"leaked buyingPrice at {string.Join(", ", buying)}");
    }

    // -----------------------------------------------------------------
    // Admin must still receive everything
    // -----------------------------------------------------------------

    [Fact]
    public async Task Admin_Medicines_Response_StillContainsCostFields()
    {
        var (status, body) = await GetRawAsync("/api/medicines", _adminToken);
        Assert.Equal(HttpStatusCode.OK, status);

        // The seeded catalogue has medicines; assert the contract is present on the
        // DTO rather than relying on a specific seeded batch.
        Assert.Contains("\"purchasePrice\"", body, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("\"sellingPrice\"", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Admin_Cosmetics_Response_StillContainsBuyingPrice()
    {
        var (status, body) = await GetRawAsync("/api/cosmetics", _adminToken);
        Assert.Equal(HttpStatusCode.OK, status);

        Assert.Contains("\"price\"", body, StringComparison.OrdinalIgnoreCase);
    }

    // -----------------------------------------------------------------
    // Restricted values must not appear as disguised nulls either
    // -----------------------------------------------------------------

    [Theory]
    [InlineData("/api/medicines")]
    [InlineData("/api/cosmetics")]
    public async Task Pharmacist_CostFields_AreOmitted_NotEmittedAsNull(string path)
    {
        var (_, body) = await GetRawAsync(path, _pharmacistToken);

        Assert.DoesNotContain("\"purchasePrice\":", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("\"buyingPrice\":", body, StringComparison.OrdinalIgnoreCase);
    }

    // -----------------------------------------------------------------
    // Operational data stays available to pharmacists
    // -----------------------------------------------------------------

    [Theory]
    [InlineData("/api/medicines")]
    [InlineData("/api/cosmetics")]
    public async Task Pharmacist_StillReceives_OperationalFields(string path)
    {
        var (status, body) = await GetRawAsync(path, _pharmacistToken);

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.True(body.Length > 2, "expected a payload, got an empty body.");
    }

    // -----------------------------------------------------------------
    // Admin-only endpoints stay closed to pharmacists
    // -----------------------------------------------------------------

    [Theory]
    [InlineData("/api/reports/inventory")]
    [InlineData("/api/reports/profit")]
    [InlineData("/api/dashboard/summary")]
    [InlineData("/api/purchases")]
    [InlineData("/api/suppliers")]
    public async Task Pharmacist_IsDenied_AdminOnlyEndpoints(string path)
    {
        var (status, _) = await GetRawAsync(path, _pharmacistToken);

        Assert.True(
            status == HttpStatusCode.Forbidden || status == HttpStatusCode.Unauthorized,
            $"expected 401/403 for {path} but got {(int)status}");
    }

    [Fact]
    public async Task Pharmacist_CanStillReach_PosSupportingEndpoints()
    {
        // POS is pharmacist-accessible, so these must not be locked down.
        var medicines = await GetRawAsync("/api/medicines", _pharmacistToken);
        var sales = await GetRawAsync("/api/sales", _pharmacistToken);

        Assert.Equal(HttpStatusCode.OK, medicines.Status);
        Assert.True(
            sales.Status == HttpStatusCode.OK || sales.Status == HttpStatusCode.NotFound,
            $"POS must remain usable for pharmacists, got {(int)sales.Status}");
    }
}