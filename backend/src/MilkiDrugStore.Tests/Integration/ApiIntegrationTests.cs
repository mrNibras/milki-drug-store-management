using System.Net;
using System.Net.Http.Json;
using System.Text;
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
using Xunit;

namespace MilkiDrugStore.Tests.Integration;

public class ApiIntegrationTests : IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly string _connectionString;

    public ApiIntegrationTests()
    {
        _connectionString = $"Data Source={Path.GetTempFileName()}";
        var dataDir = Path.Combine(Path.GetTempPath(), "MilkiDrugStore_Test_Data");
        Environment.SetEnvironmentVariable("DataDirectory", dataDir);
        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", _connectionString);
        
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
                        new KeyValuePair<string, string?>("Cors:AllowedOrigins:0", "http://localhost:5173")
                    });
                });
            });

        _client = _factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
        
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
                CreatedAt = DateTime.Now 
            });
            await db.SaveChangesAsync();
        }
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        _factory.Dispose();
        
        try
        {
            if (File.Exists(_connectionString.Replace("Data Source=", "")))
            {
                File.Delete(_connectionString.Replace("Data Source=", ""));
            }
        }
        catch { }
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
}
