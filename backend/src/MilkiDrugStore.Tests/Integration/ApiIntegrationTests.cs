using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using MilkiDrugStore.Application.Interfaces;
using MilkiDrugStore.Application.Services;
using MilkiDrugStore.Persistence.Context;
using MilkiDrugStore.Persistence.Repositories;
using MilkiDrugStore.Domain.Interfaces;
using MilkiDrugStore.Domain.Interfaces.Repositories;
using MilkiDrugStore.Domain.Entities;
using MilkiDrugStore.Infrastructure.Services;
using MilkiDrugStore.Application.DTOs.Auth;
using MilkiDrugStore.Application.DTOs.Medicine;
using MilkiDrugStore.Domain.Enums;
using Xunit;

namespace MilkiDrugStore.Tests.Integration;

public class ApiIntegrationTests : IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private string _adminToken = string.Empty;
    private string _pharmacistToken = string.Empty;

    public ApiIntegrationTests()
    {
        var testDir = Path.Combine(Path.GetTempPath(), "MilkiDrugStore_Test_Data_" + Guid.NewGuid().ToString("N"));
        var keysDir = Path.Combine(testDir, "keys");
        Environment.SetEnvironmentVariable("DataDirectory", testDir);
        Environment.SetEnvironmentVariable("BackupDirectory", Path.Combine(testDir, "backups"));
        Environment.SetEnvironmentVariable("JwtSettings__Secret", "test_secret_key_at_least_32_chars_long");
        Environment.SetEnvironmentVariable("JwtSettings__Issuer", "TestIssuer");
        Environment.SetEnvironmentVariable("JwtSettings__Audience", "TestAudience");
        Environment.SetEnvironmentVariable("JwtSettings__ExpiryMinutes", "60");

        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, config) =>
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
                        new KeyValuePair<string, string?>("AdminEmail", "admin@test.com"),
                        new KeyValuePair<string, string?>("FrontendUrl", "http://localhost:5173"),
                        new KeyValuePair<string, string?>("Cors:AllowedOrigins:0", "http://localhost:5173"),
                        new KeyValuePair<string, string?>("ConnectionStrings:DefaultConnection", string.Empty),
                        new KeyValuePair<string, string?>("DataDirectory", Path.Combine(Path.GetTempPath(), "MilkiDrugStore_Test_Data"))
                    });
                });

                builder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                    if (descriptor != null)
                        services.Remove(descriptor);

                    services.AddDbContext<AppDbContext>(options =>
                        options.UseInMemoryDatabase("MilkiDrugStore_Api_Test"));

                    services.AddScoped<IUnitOfWork, UnitOfWork>();
                    services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
                    services.AddScoped<IMedicineRepository, MedicineRepository>();
                    services.AddScoped<ISaleRepository, SaleRepository>();
                    services.AddScoped<IPurchaseRepository, PurchaseRepository>();
                    services.AddScoped<IInventoryTransactionRepository, InventoryTransactionRepository>();
                    services.AddScoped<INotificationRepository, NotificationRepository>();
                    services.AddScoped<IAuditLogRepository, AuditLogRepository>();
                    services.AddScoped<IAuthService, MilkiDrugStore.Application.Services.AuthService>();
                    services.AddScoped<IJwtTokenService, MilkiDrugStore.Infrastructure.Services.JwtTokenService>();
                    services.AddScoped<IEmailService, MilkiDrugStore.Infrastructure.Services.EmailService>();
                    services.AddScoped<IAuditLogService, MilkiDrugStore.Application.Services.AuditLogService>();
                    services.AddScoped<IBranchService, MilkiDrugStore.Application.Services.BranchService>();
                    services.AddScoped<ICosmeticService, MilkiDrugStore.Application.Services.CosmeticService>();
                    services.AddScoped<IBackupService, MilkiDrugStore.Application.Services.BackupService>();
                });
            });

        _client = _factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.EnsureCreated();

        if (!await db.Roles.AnyAsync())
        {
            db.Roles.AddRange(
                new Role { Name = "Admin" },
                new Role { Name = "Pharmacist" }
            );
            await db.SaveChangesAsync();
        }

        if (!await db.Branches.AnyAsync())
        {
            db.Branches.Add(new Branch
            {
                BranchName = "Test Branch",
                Location = "Test Location",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }

        if (!await db.Users.AnyAsync())
        {
            var adminRole = await db.Roles.SingleAsync(r => r.Name == "Admin");
            var pharmacistRole = await db.Roles.SingleAsync(r => r.Name == "Pharmacist");
            var branch = await db.Branches.FirstAsync();
            db.Users.AddRange(
                new User
                {
                    FullName = "Admin User",
                    Email = "admin@milki.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123"),
                    RoleId = adminRole.RoleId,
                    BranchId = branch.BranchId,
                    IsApproved = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    FullName = "Pharmacist User",
                    Email = "pharmacist@milki.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pharmacist123"),
                    RoleId = pharmacistRole.RoleId,
                    BranchId = branch.BranchId,
                    IsApproved = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            );
            await db.SaveChangesAsync();
        }
        else
        {
            // Ensure both users exist even if seeding ran first
            var adminRole = await db.Roles.SingleAsync(r => r.Name == "Admin");
            var pharmacistRole = await db.Roles.SingleAsync(r => r.Name == "Pharmacist");
            var branch = await db.Branches.FirstAsync();

            if (!await db.Users.AnyAsync(u => u.Email == "admin@milki.com"))
            {
                db.Users.Add(new User
                {
                    FullName = "Admin User",
                    Email = "admin@milki.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123"),
                    RoleId = adminRole.RoleId,
                    BranchId = branch.BranchId,
                    IsApproved = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                });
            }

            if (!await db.Users.AnyAsync(u => u.Email == "pharmacist@milki.com"))
            {
                db.Users.Add(new User
                {
                    FullName = "Pharmacist User",
                    Email = "pharmacist@milki.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pharmacist123"),
                    RoleId = pharmacistRole.RoleId,
                    BranchId = branch.BranchId,
                    IsApproved = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                });
            }
            await db.SaveChangesAsync();
        }

        if (!await db.UnitTypes.AnyAsync())
        {
            db.UnitTypes.AddRange(
                new UnitType { Name = "Tablet", IsActive = true },
                new UnitType { Name = "Capsule", IsActive = true },
                new UnitType { Name = "Other", IsActive = true }
            );
            await db.SaveChangesAsync();
        }

        if (!await db.Categories.AnyAsync())
        {
            db.Categories.AddRange(
                new Category { Name = "Antibiotics", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Category { Name = "Antivirals", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Category { Name = "Other", IsActive = true, CreatedAt = DateTime.UtcNow }
            );
            await db.SaveChangesAsync();
        }

        // Seed Settings
        if (!await db.Settings.AnyAsync())
        {
            var branch = await db.Branches.FirstAsync();
            db.Settings.Add(new Settings
            {
                PharmacyName = "Test Pharmacy",
                Address = "Test Address",
                Phone = "0912345678",
                Email = "test@pharmacy.com",
                Language = "English",
                LowStockThreshold = 10,
                ExpiryAlertMonths = 6,
                Currency = "ETB",
                BatchSelectionMode = BatchSelectionMode.AutomaticFefo,
                BranchId = branch.BranchId
            });
            await db.SaveChangesAsync();
        }

        var adminLogin = new LoginRequest { Email = "admin@milki.com", Password = "Admin123" };
        var adminResponse = await _client.PostAsJsonAsync("/api/auth/login", adminLogin);
        if (adminResponse.StatusCode == HttpStatusCode.OK)
        {
            var data = await adminResponse.Content.ReadFromJsonAsync<LoginResponse>();
            _adminToken = data?.Token ?? string.Empty;
        }

        var pharmacistLogin = new LoginRequest { Email = "pharmacist@milki.com", Password = "Pharmacist123" };
        var pharmacistResponse = await _client.PostAsJsonAsync("/api/auth/login", pharmacistLogin);
        if (pharmacistResponse.StatusCode == HttpStatusCode.OK)
        {
            var data = await pharmacistResponse.Content.ReadFromJsonAsync<LoginResponse>();
            _pharmacistToken = data?.Token ?? string.Empty;
        }
    }

    public Task DisposeAsync()
    {
        _client.Dispose();
        _factory.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task Register_Should_Create_New_User()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var request = new RegisterRequest
        {
            FullName = "Test User",
            Email = "testuser@test.com",
            Password = "Test123!"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/register", request);
        var content = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Login_Should_Return_Token_For_Valid_Credentials()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var loginRequest = new LoginRequest
        {
            Email = "admin@milki.com",
            Password = "Admin123"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetSettings_Should_Require_Authentication()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.GetAsync("/api/settings");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Backup_Should_Require_Authentication()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.GetAsync("/api/settings/backup");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ForgotPassword_Should_Return_Success_Message()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var request = new ForgotPasswordRequest { Email = "admin@milki.com" };
        var response = await _client.PostAsJsonAsync("/api/auth/forgot-password", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task HealthCheck_Should_Return_Healthy()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.GetAsync("/api/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetCategories_Should_Return_BuiltIn_And_Custom()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/catalog/categories");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _adminToken);
        var response = await _client.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotEmpty(content);
        Assert.Contains("Antibiotics", content);
        Assert.Contains("Antivirals", content);
        Assert.Contains("Other", content);
        Assert.Contains("isSystem", content);
        Assert.Contains("\"isSystem\":true", content);
    }

    [Fact]
    public async Task GetUnitTypes_Should_Return_BuiltIn_And_Custom()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/catalog/unit-types");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _adminToken);
        var response = await _client.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotEmpty(content);
        Assert.Contains("Tablet", content);
        Assert.Contains("Capsule", content);
        Assert.Contains("Other", content);
        Assert.Contains("isSystem", content);
        Assert.Contains("\"isSystem\":true", content);
    }

    [Fact]
    public async Task CustomCategoryAndUnitType_Should_Persist_And_Not_Duplicate()
    {
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _adminToken);
        var create = new CreateMedicineRequest
        {
            BrandName = "Test Supplements",
            GenericName = "Multivitamin",
            CategoryId = 0,
            NewCategoryName = "  Supplements  ",
            UnitTypeId = 0,
            NewUnitTypeName = "  Sachet  ",
            ReorderLevel = 5
        };
        var createResponse = await _client.PostAsJsonAsync("/api/medicines", create);
        if (createResponse.StatusCode != HttpStatusCode.OK)
        {
            var dbg = await createResponse.Content.ReadAsStringAsync();
            throw new Exception($"POST /api/medicines failed with {createResponse.StatusCode}: {dbg}");
        }

        var categoriesResponse = await _client.GetAsync("/api/catalog/categories");
        var categoriesContent = await categoriesResponse.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, categoriesResponse.StatusCode);
        Assert.Contains("Supplements", categoriesContent);

        var unitTypesResponse = await _client.GetAsync("/api/catalog/unit-types");
        var unitTypesContent = await unitTypesResponse.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, unitTypesResponse.StatusCode);
        Assert.Contains("Sachet", unitTypesContent);

        var duplicate = new CreateMedicineRequest
        {
            BrandName = "Test Supplements Duplicate",
            GenericName = "Multivitamin 2",
            CategoryId = 0,
            NewCategoryName = "SUPPLEMENTS",
            UnitTypeId = 0,
            NewUnitTypeName = "sachet",
            ReorderLevel = 5
        };
        var duplicateResponse = await _client.PostAsJsonAsync("/api/medicines", duplicate);
        Assert.Equal(HttpStatusCode.OK, duplicateResponse.StatusCode);

        var categoriesAfter = await (await _client.GetAsync("/api/catalog/categories")).Content.ReadAsStringAsync();
        Assert.Single(System.Text.RegularExpressions.Regex.Matches(categoriesAfter, "Supplements"));

        var unitTypesAfter = await (await _client.GetAsync("/api/catalog/unit-types")).Content.ReadAsStringAsync();
        Assert.Single(System.Text.RegularExpressions.Regex.Matches(unitTypesAfter, "Sachet"));
    }

    [Fact]
    public async Task GetPublicSettings_AsAdmin_Should_Return_200()
    {
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _adminToken);
        var response = await _client.GetAsync("/api/settings/public");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetPublicSettings_AsPharmacist_Should_Return_200()
    {
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _pharmacistToken);
        var response = await _client.GetAsync("/api/settings/public");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetPublicSettings_WithoutAuth_Should_Return_401()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.GetAsync("/api/settings/public");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetSettings_AsPharmacist_Should_Return_403()
    {
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _pharmacistToken);
        var response = await _client.GetAsync("/api/settings");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetSettings_AsAdmin_Should_Return_200()
    {
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _adminToken);
        var response = await _client.GetAsync("/api/settings");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetPublicSettings_Response_Contains_Only_Public_Fields()
    {
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _adminToken);
        var response = await _client.GetAsync("/api/settings/public");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content).RootElement;

        Assert.True(json.TryGetProperty("pharmacyName", out _), "Response must contain pharmacyName");
        Assert.True(json.TryGetProperty("language", out _), "Response must contain language");
        Assert.True(json.TryGetProperty("currency", out _), "Response must contain currency");

        // Verify NO sensitive fields are present
        Assert.False(json.TryGetProperty("address", out _), "Response must NOT contain address");
        Assert.False(json.TryGetProperty("phone", out _), "Response must NOT contain phone");
        Assert.False(json.TryGetProperty("email", out _), "Response must NOT contain email");
        Assert.False(json.TryGetProperty("lowStockThreshold", out _), "Response must NOT contain lowStockThreshold");
        Assert.False(json.TryGetProperty("expiryAlertMonths", out _), "Response must NOT contain expiryAlertMonths");
        Assert.False(json.TryGetProperty("batchSelectionMode", out _), "Response must NOT contain batchSelectionMode");
        Assert.False(json.TryGetProperty("branchId", out _), "Response must NOT contain branchId");
    }
}
