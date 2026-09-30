using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MilkiDrugStore.Application.DTOs.Auth;
using MilkiDrugStore.Application.Services;
using MilkiDrugStore.Application.Interfaces;
using MilkiDrugStore.Domain.Entities;
using MilkiDrugStore.Domain.Enums;
using MilkiDrugStore.Domain.Interfaces;
using MilkiDrugStore.Domain.Interfaces.Repositories;
using MilkiDrugStore.Infrastructure.Services;
using MilkiDrugStore.Persistence.Context;
using MilkiDrugStore.Persistence.Repositories;
using Xunit;

namespace MilkiDrugStore.Tests.Integration;

/// <summary>
/// End-to-end coverage for deleted/deactivated user access revocation
/// (tests 1-8 of the deleted-user requirement).
/// </summary>
public class DeletedUserAccessTests : IAsyncLifetime
{
    private WebApplicationFactory<Program>? _factory;
    private HttpClient _client = null!;
    private int _branchId;
    private int _pharmacistAId;
    private int _pharmacistBId;

    // Fixed for the lifetime of this test instance: the host builder may run
    // ConfigureServices more than once, and each run must target the same store.
    private readonly string _databaseName = "MilkiAuthTest_" + Guid.NewGuid().ToString("N");

    public async Task InitializeAsync()
    {
        var testDir = Path.Combine(Path.GetTempPath(), "MilkiAuth_" + Guid.NewGuid().ToString("N"));

        // Program.cs builds the JWT validator from the ambient configuration, so
        // the signing secret is supplied through environment variables before the
        // host is created. Values match the other integration test fixtures.
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
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                    if (descriptor != null)
                        services.Remove(descriptor);

                    services.AddDbContext<AppDbContext>(options =>
                        options.UseInMemoryDatabase(_databaseName));

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

        var adminRole = await db.Roles.SingleAsync(r => r.Name == "Admin");
        var pharmacistRole = await db.Roles.SingleAsync(r => r.Name == "Pharmacist");
        _branchId = (await db.Branches.FirstAsync()).BranchId;

        if (!await db.Users.AnyAsync(u => u.Email == "pharmacista@milki.com"))
        {
            db.Users.AddRange(
                new User
                {
                    FullName = "Admin User",
                    Email = "admin@milki.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123"),
                    RoleId = adminRole.RoleId,
                    BranchId = _branchId,
                    IsApproved = true,
                    IsActive = true
                },
                new User
                {
                    FullName = "Pharmacist A",
                    Email = "pharmacista@milki.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pharmacist123"),
                    RoleId = pharmacistRole.RoleId,
                    BranchId = _branchId,
                    IsApproved = true,
                    IsActive = true
                },
                new User
                {
                    FullName = "Pharmacist B",
                    Email = "pharmacistb@milki.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pharmacist123"),
                    RoleId = pharmacistRole.RoleId,
                    BranchId = _branchId,
                    IsApproved = true,
                    IsActive = true
                });
            await db.SaveChangesAsync();
        }

        _pharmacistAId = (await db.Users.SingleAsync(u => u.Email == "pharmacista@milki.com")).UserId;
        _pharmacistBId = (await db.Users.SingleAsync(u => u.Email == "pharmacistb@milki.com")).UserId;
    }

    public Task DisposeAsync()
    {
        _client.Dispose();
        _factory?.Dispose();
        return Task.CompletedTask;
    }

    private async Task<string> LoginAsync(string email, string password = "Pharmacist123")
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest { Email = email, Password = password });
        response.StatusCode.Should().Be(HttpStatusCode.OK, "login should succeed for an active account");
        var login = await response.Content.ReadFromJsonAsync<LoginResponse>();
        return login!.Token;
    }

    private async Task DeleteUserAsync(int userId)
    {
        var adminToken = await LoginAsync("admin@milki.com", "Admin123");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var response = await _client.DeleteAsync($"/api/users/{userId}");
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    private async Task<HttpResponseMessage> GetProtectedAsync(string token)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/settings/public");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await _client.SendAsync(request);
    }

    // Test 1
    [Fact]
    public async Task Test1_ActivePharmacist_CanLoginAndAccessProtectedEndpoint()
    {
        var token = await LoginAsync("pharmacista@milki.com");

        var response = await GetProtectedAsync(token);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // Tests 2, 3, 4, 5 and 8
    [Fact]
    public async Task Tests2To5And8_DeletedUser_IsFullyRevoked_AndAdminUnaffected()
    {
        // Test 1: active pharmacist holds a valid access token and refresh token.
        _client.DefaultRequestHeaders.Authorization = null;
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest { Email = "pharmacista@milki.com", Password = "Pharmacist123" });
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        var oldAccessToken = login!.Token;
        var oldRefreshToken = login.RefreshToken!;

        var beforeDelete = await GetProtectedAsync(oldAccessToken);
        beforeDelete.StatusCode.Should().Be(HttpStatusCode.OK);

        // Test 2: admin deletes the pharmacist -> IsActive = false.
        await DeleteUserAsync(_pharmacistAId);

        using (var scope = _factory!.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var user = await db.Users.SingleAsync(u => u.UserId == _pharmacistAId);
            user.IsActive.Should().BeFalse();
        }

        // Test 3: the deleted pharmacist can no longer log in.
        _client.DefaultRequestHeaders.Authorization = null;
        var relogin = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest { Email = "pharmacista@milki.com", Password = "Pharmacist123" });
        relogin.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        // The failure must not reveal that the account exists but is deactivated.
        var reloginBody = await relogin.Content.ReadAsStringAsync();
        reloginBody.Should().NotContain("IsActive");
        reloginBody.Should().NotContain("inactive");

        // Test 4: the previously issued access token is rejected immediately.
        var afterDelete = await GetProtectedAsync(oldAccessToken);
        afterDelete.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        // Test 5: the previously issued refresh token cannot mint a new access token.
        var refresh = await _client.PostAsJsonAsync("/api/auth/refresh",
            new RefreshTokenRequest { RefreshToken = oldRefreshToken });
        refresh.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        // The refresh token is retained as revoked history, not deleted.
        using (var scope = _factory!.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var stored = await db.RefreshTokens.SingleAsync(rt => rt.Token == oldRefreshToken);
            stored.IsRevoked.Should().BeTrue();
        }

        // Test 8: deleting a pharmacist does not invalidate the admin.
        var adminToken = await LoginAsync("admin@milki.com", "Admin123");
        var adminCall = await GetProtectedAsync(adminToken);
        adminCall.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // Test 6
    [Fact]
    public async Task Test6_ReactivatedAccount_CanLoginAndAccessProtectedEndpoints()
    {
        await DeleteUserAsync(_pharmacistAId);

        // Re-enable the account the way an administrator re-activates a user.
        using (var scope = _factory!.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var user = await db.Users.SingleAsync(u => u.UserId == _pharmacistAId);
            user.IsActive = true;
            await db.SaveChangesAsync();
        }

        var token = await LoginAsync("pharmacista@milki.com");
        var response = await GetProtectedAsync(token);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Test6_NewlyCreatedPharmacist_CanLoginAndAccessProtectedEndpoints()
    {
        var adminToken = await LoginAsync("admin@milki.com", "Admin123");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var create = await _client.PostAsJsonAsync("/api/users", new CreateUserRequest
        {
            FullName = "Pharmacist C",
            Email = "pharmacistc@milki.com",
            Password = "Pharmacist123",
            RoleId = (await GetPharmacistRoleIdAsync()),
            BranchId = _branchId
        });
        create.StatusCode.Should().Be(HttpStatusCode.OK);

        var token = await LoginAsync("pharmacistc@milki.com");
        var response = await GetProtectedAsync(token);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // Test 7
    [Fact]
    public async Task Test7_DeletingOnePharmacist_LeavesOtherPharmacistActive()
    {
        await DeleteUserAsync(_pharmacistAId);

        var tokenB = await LoginAsync("pharmacistb@milki.com");
        var responseB = await GetProtectedAsync(tokenB);

        responseB.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeactivatingViaUpdate_AlsoRevokesAccessAndRefreshTokens()
    {
        var oldAccessToken = await LoginAsync("pharmacista@milki.com");
        _client.DefaultRequestHeaders.Authorization = null;
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest { Email = "pharmacista@milki.com", Password = "Pharmacist123" });
        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        var adminToken = await LoginAsync("admin@milki.com", "Admin123");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var update = await _client.PutAsJsonAsync($"/api/users/{_pharmacistAId}", new UpdateUserRequest
        {
            FullName = "Pharmacist A",
            Email = "pharmacista@milki.com",
            IsActive = false
        });
        update.StatusCode.Should().Be(HttpStatusCode.OK);

        var response = await GetProtectedAsync(oldAccessToken);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        _client.DefaultRequestHeaders.Authorization = null;
        var refresh = await _client.PostAsJsonAsync("/api/auth/refresh",
            new RefreshTokenRequest { RefreshToken = login!.RefreshToken! });
        refresh.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private async Task<int> GetPharmacistRoleIdAsync()
    {
        using var scope = _factory!.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return (await db.Roles.SingleAsync(r => r.Name == "Pharmacist")).RoleId;
    }
}
