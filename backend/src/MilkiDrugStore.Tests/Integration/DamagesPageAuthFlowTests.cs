using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MilkiDrugStore.Application.DTOs.Auth;
using MilkiDrugStore.Application.Interfaces;
using MilkiDrugStore.Application.Services;
using MilkiDrugStore.Domain.Entities;
using MilkiDrugStore.Domain.Interfaces;
using MilkiDrugStore.Domain.Interfaces.Repositories;
using MilkiDrugStore.Infrastructure.Services;
using MilkiDrugStore.Persistence.Context;
using MilkiDrugStore.Persistence.Repositories;
using Xunit;

namespace MilkiDrugStore.Tests.Integration;

/// <summary>
/// Replays the exact production request set observed failing with 401 on /damages,
/// for an ACTIVE admin and an ACTIVE pharmacist, and asserts the full damage
/// flow end to end. Also locks in that a deleted user is still rejected.
/// </summary>
public class DamagesPageAuthFlowTests : IAsyncLifetime
{
    private WebApplicationFactory<Program>? _factory;
    private HttpClient _client = null!;
    private readonly string _databaseName = "DamagesAuth_" + Guid.NewGuid().ToString("N").Substring(0, 12);
    private int _branchId;
    private int _pharmacistId;

    public async Task InitializeAsync()
    {
        var testDir = Path.Combine(Path.GetTempPath(), "MilkiDamageAuth_" + Guid.NewGuid().ToString("N"));
        Environment.SetEnvironmentVariable("DataDirectory", testDir);
        Environment.SetEnvironmentVariable("BackupDirectory", Path.Combine(testDir, "backups"));
        Environment.SetEnvironmentVariable("JwtSettings__Secret", "test_secret_key_at_least_32_chars_long");
        Environment.SetEnvironmentVariable("JwtSettings__Issuer", "TestIssuer");
        Environment.SetEnvironmentVariable("JwtSettings__Audience", "TestAudience");

        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((_, config) =>
                {
                    config.AddInMemoryCollection(new[]
                    {
                        new KeyValuePair<string, string?>("JwtSettings:Secret", "test_secret_key_at_least_32_chars_long"),
                        new KeyValuePair<string, string?>("JwtSettings:Issuer", "TestIssuer"),
                        new KeyValuePair<string, string?>("JwtSettings:Audience", "TestAudience"),
                        new KeyValuePair<string, string?>("Email:DevMode", "true"),
                        new KeyValuePair<string, string?>("ConnectionStrings:DefaultConnection", string.Empty),
                        new KeyValuePair<string, string?>("DataDirectory", testDir),
                        new KeyValuePair<string, string?>("BackupDirectory", Path.Combine(testDir, "backups"))
                    });
                });

                builder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                    if (descriptor != null)
                        services.Remove(descriptor);

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
                    services.AddScoped<IAuthService, AuthService>();
                    services.AddScoped<IUserActivityService, UserActivityService>();
                    services.AddScoped<IJwtTokenService, JwtTokenService>();
                    services.AddScoped<IEmailService, EmailService>();
                    services.AddScoped<IAuditLogService, AuditLogService>();
                    services.AddScoped<IInventoryService, InventoryService>();
                });
            });

        _client = _factory.CreateClient();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.EnsureCreated();

        if (!await db.Roles.AnyAsync(r => r.Name == "Admin"))
            db.Roles.AddRange(new Role { Name = "Admin" }, new Role { Name = "Pharmacist" });
        if (!await db.Branches.AnyAsync())
            db.Branches.Add(new Branch { BranchName = "Test Branch", IsActive = true, CreatedAt = DateTime.UtcNow });
        await db.SaveChangesAsync();

        var adminRoleId = (await db.Roles.SingleAsync(r => r.Name == "Admin")).RoleId;
        var pharmRoleId = (await db.Roles.SingleAsync(r => r.Name == "Pharmacist")).RoleId;
        _branchId = (await db.Branches.FirstAsync()).BranchId;

        if (!await db.Users.AnyAsync(u => u.Email == "pharmacist@milki.com"))
        {
            db.Users.AddRange(
                new User
                {
                    FullName = "Admin User", Email = "admin@milki.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123"),
                    RoleId = adminRoleId, BranchId = _branchId, IsApproved = true, IsActive = true
                },
                new User
                {
                    FullName = "Pharmacist User", Email = "pharmacist@milki.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pharmacist123"),
                    RoleId = pharmRoleId, BranchId = _branchId, IsApproved = true, IsActive = true
                });
            await db.SaveChangesAsync();
        }

        _pharmacistId = (await db.Users.SingleAsync(u => u.Email == "pharmacist@milki.com")).UserId;
    }

    public Task DisposeAsync()
    {
        _client.Dispose();
        _factory?.Dispose();
        return Task.CompletedTask;
    }

    private async Task<LoginResponse> LoginAsync(string email, string password)
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var res = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest { Email = email, Password = password });
        res.StatusCode.Should().Be(HttpStatusCode.OK, "active user must be able to log in");
        return (await res.Content.ReadFromJsonAsync<LoginResponse>())!;
    }

    private async Task<HttpResponseMessage> GetAs(string token, string path)
    {
        using var req = new HttpRequestMessage(HttpMethod.Get, path);
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await _client.SendAsync(req);
    }

    private async Task SeedBatchesAsync()
    {
        using var scope = _factory!.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var medicine = new Medicine
        {
            BrandName = "Auth Brand", GenericName = "Auth Generic", ProductCode = "AUTH-1",
            CategoryId = -1, UnitTypeId = -1, IsActive = true
        };
        var cosmetic = new Cosmetic
        {
            ProductName = "Test Cosmetic Damage", CategoryId = -1, UnitTypeId = -1,
            Price = 100m, IsActive = true, BranchId = _branchId
        };
        db.Medicines.Add(medicine);
        db.Cosmetics.Add(cosmetic);
        await db.SaveChangesAsync();

        db.MedicineBatches.Add(new MedicineBatch
        {
            ProductId = medicine.ProductId, BranchId = _branchId, BatchNumber = "MED-AUTH-1",
            PurchasePrice = 10m, SellingPrice = 20m, QuantityReceived = 100,
            ExpiryDate = DateTime.UtcNow.AddYears(1)
        });
        db.CosmeticBatches.Add(new CosmeticBatch
        {
            CosmeticId = cosmetic.CosmeticId, BranchId = _branchId, BatchNumber = "COS-DMG-001",
            QuantityReceived = 50, BuyingPrice = 40m, SellingPrice = 100m,
            ExpiryDate = DateTime.UtcNow.AddYears(1)
        });
        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Every endpoint seen returning 401 in production, replayed for an active
    /// pharmacist. None of them may be 401.
    /// </summary>
    [Fact]
    public async Task ActivePharmacist_AllDamagesPageEndpoints_AreNot401()
    {
        var login = await LoginAsync("pharmacist@milki.com", "Pharmacist123");

        foreach (var path in new[]
                 {
                     "/api/medicines", "/api/cosmetics", "/api/lookups/categories",
                     "/api/notifications", "/api/expired", "/api/damages"
                 })
        {
            var res = await GetAs(login.Token, path);
            res.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized,
                $"{path} must not return 401 for an active, logged-in user");
        }
    }

    /// <summary>
    /// The same endpoints for an active ADMIN, including the two Admin-only
    /// endpoints seen in the production screenshot (audit-logs, notifications).
    /// </summary>
    [Fact]
    public async Task ActiveAdmin_AllDamagesPageEndpoints_AreNot401()
    {
        var login = await LoginAsync("admin@milki.com", "Admin123");

        foreach (var path in new[]
                 {
                     "/api/medicines", "/api/cosmetics", "/api/lookups/categories",
                     "/api/notifications", "/api/audit-logs", "/api/expired", "/api/damages"
                 })
        {
            var res = await GetAs(login.Token, path);
            res.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized,
                $"{path} must not return 401 for an active admin");
        }
    }

    /// <summary>
    /// A role must survive a token refresh. This is what the production logins
    /// hit after 7 hours or after any 401-triggered refresh.
    /// </summary>
    [Fact]
    public async Task RefreshedToken_PreservesTheUsersRealRole()
    {
        var adminLogin = await LoginAsync("admin@milki.com", "Admin123");
        adminLogin.Role.Should().Be("Admin");

        _client.DefaultRequestHeaders.Authorization = null;
        var refresh = await _client.PostAsJsonAsync("/api/auth/refresh",
            new RefreshTokenRequest { RefreshToken = adminLogin.RefreshToken });
        refresh.StatusCode.Should().Be(HttpStatusCode.OK);
        var refreshed = (await refresh.Content.ReadFromJsonAsync<LoginResponse>())!;

        refreshed.Role.Should().Be("Admin", "the response role must stay Admin");

        // Assert the actual JWT claim, not just the HTTP outcome.
        var claims = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler()
            .ReadJwtToken(refreshed.Token).Claims
            .Where(c => c.Type is System.Security.Claims.ClaimTypes.Role
                        or System.Security.Claims.ClaimTypes.NameIdentifier)
            .Select(c => $"{c.Type}={c.Value}")
            .ToList();
        claims.Should().Contain(x => x.EndsWith("=Admin"),
            "the refreshed JWT must carry the real role claim; actual claims: " + string.Join(", ", claims));
        claims.Should().Contain(x => x.EndsWith("=" + adminLogin.UserId),
            "the refreshed JWT must carry the correct user id");

        // The decisive check: the REFRESHED token must still be an Admin token.
        var auditLogs = await GetAs(refreshed.Token, "/api/audit-logs");
        auditLogs.StatusCode.Should().Be(HttpStatusCode.OK,
            "a refreshed admin token must still pass [Authorize(Roles = \"Admin\")]");

        var notifications = await GetAs(refreshed.Token, "/api/notifications");
        notifications.StatusCode.Should().Be(HttpStatusCode.OK,
            "a refreshed admin token must still pass [Authorize(Roles = \"Admin,Pharmacist\")]");
    }

    /// <summary>Full medicine + cosmetic damage flow through the HTTP API.</summary>
    [Fact]
    public async Task ActiveUser_CanRecordMedicineAndCosmeticDamage_AndHistoryPersists()
    {
        await SeedBatchesAsync();
        var login = await LoginAsync("admin@milki.com", "Admin123");

        int medicineBatchId, cosmeticBatchId;
        using (var scope = _factory!.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            medicineBatchId = (await db.MedicineBatches.SingleAsync(b => b.BatchNumber == "MED-AUTH-1")).BatchId;
            cosmeticBatchId = (await db.CosmeticBatches.SingleAsync(b => b.BatchNumber == "COS-DMG-001")).BatchId;
        }

        // Medicine damage: 100 -> 90
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.Token);
        var medRes = await _client.PostAsJsonAsync("/api/damages", new
        {
            batchId = medicineBatchId, quantity = 10, reason = "Test damage"
        });
        medRes.StatusCode.Should().Be(HttpStatusCode.OK, "medicine damage must succeed");

        // Cosmetic damage: 50 -> 45
        var cosRes = await _client.PostAsJsonAsync("/api/damages", new
        {
            cosmeticBatchId, quantity = 5, reason = "Damaged item"
        });
        cosRes.StatusCode.Should().Be(HttpStatusCode.OK, "cosmetic damage must succeed");

        using (var scope = _factory!.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var medBatch = await db.MedicineBatches.FindAsync(medicineBatchId);
            medBatch!.RemainingQuantity.Should().Be(90);
            medBatch.QuantityDamaged.Should().Be(10);

            var cosBatch = await db.CosmeticBatches.FindAsync(cosmeticBatchId);
            cosBatch!.Balance.Should().Be(45);
            cosBatch.QuantityDamaged.Should().Be(5);

            var records = await db.DamageRecords.ToListAsync();
            records.Should().HaveCount(2);
            records.Should().Contain(r => r.BatchId == medicineBatchId && r.Quantity == 10);
            records.Should().Contain(r => r.CosmeticBatchId == cosmeticBatchId && r.Quantity == 5);

            var transactions = await db.InventoryTransactions
                .Where(t => t.ReferenceType == "DAMAGE").ToListAsync();
            transactions.Should().HaveCount(2);
        }

        // Reload the page: history must still be returned by GET /api/damages.
        var history = await GetAs(login.Token, "/api/damages");
        history.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await history.Content.ReadAsStringAsync();
        body.Should().Contain("Test damage");
        body.Should().Contain("Damaged item");
        body.Should().Contain("COS-DMG-001");
        body.Should().Contain("MED-AUTH-1");
        body.Should().Contain("cosmetic");
    }

    /// <summary>Regression guard: the deleted-user protection must remain.</summary>
    [Fact]
    public async Task DeletedUser_OldJwtIsStillRejected()
    {
        var login = await LoginAsync("pharmacist@milki.com", "Pharmacist123");

        var adminLogin = await LoginAsync("admin@milki.com", "Admin123");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminLogin.Token);
        (await _client.DeleteAsync($"/api/users/{_pharmacistId}")).StatusCode
            .Should().Be(HttpStatusCode.NoContent);

        foreach (var path in new[] { "/api/medicines", "/api/cosmetics", "/api/expired", "/api/damages" })
        {
            var res = await GetAs(login.Token, path);
            res.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
                $"{path} must reject a token belonging to a deleted user");
        }

        _client.DefaultRequestHeaders.Authorization = null;
        var refresh = await _client.PostAsJsonAsync("/api/auth/refresh",
            new RefreshTokenRequest { RefreshToken = login.RefreshToken });
        refresh.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
