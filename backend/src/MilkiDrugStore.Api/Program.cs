using MilkiDrugStore.Api;
using MilkiDrugStore.Api.Configuration;
using MilkiDrugStore.Api.Extensions;
using MilkiDrugStore.Api.Middleware;
using MilkiDrugStore.Application.Mappings;
using MilkiDrugStore.Infrastructure.BackgroundJobs;
using MilkiDrugStore.Infrastructure.Logging;
using MilkiDrugStore.Persistence.Repositories;
using MilkiDrugStore.Persistence.Context;
using MilkiDrugStore.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Ensure data directory exists for SQLite and Data Protection keys.
var dataDir = builder.Configuration.GetValue<string>("DataDirectory") ?? "/var/data";
try
{
    Directory.CreateDirectory(dataDir);
}
catch
{
    dataDir = Path.Combine(Path.GetTempPath(), "MilkiDrugStore_Data");
    Directory.CreateDirectory(dataDir);
}

var keysDir = Path.Combine(dataDir, "keys");
try
{
    Directory.CreateDirectory(keysDir);
}
catch
{
    keysDir = Path.Combine(Path.GetTempPath(), "MilkiDrugStore_Keys");
    Directory.CreateDirectory(keysDir);
}

// Data Protection: persist keys to persistent disk so they survive container restarts.
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(keysDir))
    .SetApplicationName("MilkiDrugStore")
    .SetDefaultKeyLifetime(TimeSpan.FromDays(90));

// Strongly typed configuration.
builder.Services.AddOptions<JwtSettings>()
    .Bind(builder.Configuration.GetSection("JwtSettings"))
    .ValidateOnStart();

builder.Services.AddOptions<EmailSettings>()
    .Bind(builder.Configuration.GetSection("Email"))
    .ValidateOnStart();

builder.Services.AddOptions<CorsSettings>()
    .Bind(builder.Configuration.GetSection("Cors"))
    .ValidateOnStart();

// Database provider auto-detection.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (!string.IsNullOrEmpty(connectionString) && connectionString.Contains("Server=", StringComparison.OrdinalIgnoreCase))
    {
        options.UseSqlServer(connectionString, sql =>
        {
            sql.MigrationsAssembly("MilkiDrugStore.Persistence");
        });
    }
    else
    {
        options.UseSqlite(connectionString, sql =>
        {
            sql.MigrationsAssembly("MilkiDrugStore.Persistence");
        });
    }
});

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddApplicationServices();

// JWT with strongly typed configuration.
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>() 
                  ?? new JwtSettings();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(jwtSettings.Secret)),
            ValidateIssuer = !string.IsNullOrEmpty(jwtSettings.Issuer),
            ValidateAudience = !string.IsNullOrEmpty(jwtSettings.Audience),
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            ClockSkew = TimeSpan.Zero,
            RoleClaimType = System.Security.Claims.ClaimTypes.Role,
            NameClaimType = System.Security.Claims.ClaimTypes.NameIdentifier
        };
    });

builder.Services.AddAuthorization(options =>
{
});

// CORS from configuration.
var corsSettings = builder.Configuration.GetSection("Cors").Get<CorsSettings>() 
                   ?? new CorsSettings();

if (corsSettings.AllowedOrigins.Count > 0)
{
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowFrontend", policy =>
        {
            policy.WithOrigins(corsSettings.AllowedOrigins.ToArray())
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
    });
}

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var jwtScheme = new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter the JWT token as: Bearer {your token}",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new Microsoft.OpenApi.Models.OpenApiReference
        {
            Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
            Id = "Bearer"
        }
    };
    options.AddSecurityDefinition("Bearer", jwtScheme);
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        { jwtScheme, Array.Empty<string>() }
    });
});

builder.Services.AddHostedService<MilkiDrugStore.Infrastructure.BackgroundJobs.ExpiryCheckBackgroundService>();
builder.Services.AddHostedService<MilkiDrugStore.Infrastructure.BackgroundJobs.NotificationCheckBackgroundService>();

// Global exception filter.
builder.Services.AddControllers(options =>
{
    options.Filters.Add<MilkiDrugStore.Api.Middleware.GlobalExceptionFilter>();
});

var app = builder.Build();

// Apply migrations and seed database.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    try
    {
        var contextType = db.GetType().FullName;
        var provider = db.Database.ProviderName ?? "Unknown";
        
        logger.LogInformation("=== Database Initialization Starting ===");
        logger.LogInformation("Context Type: {ContextType}", contextType);
        logger.LogInformation("Database Provider: {Provider}", provider);
        logger.LogInformation("Migration Assembly: MilkiDrugStore.Persistence");
        logger.LogInformation("Connection String: {ConnectionString}", 
            MaskConnectionString(connectionString));
        
        var pendingMigrations = db.Database.GetPendingMigrations();
        var pendingCount = pendingMigrations.Count();
        
        logger.LogInformation("Pending Migrations Count: {PendingCount}", pendingCount);
        
        if (pendingCount > 0)
        {
            logger.LogInformation("Applying {Count} pending migration(s)...", pendingCount);
            foreach (var migrationId in pendingMigrations)
            {
                logger.LogInformation("  Applying Migration: {MigrationId}", migrationId);
            }
        }
        
        logger.LogInformation("Starting database migration...");
        db.Database.Migrate();
        logger.LogInformation("Database migration completed successfully.");
        
        var appliedMigrations = db.Database.GetAppliedMigrations();
        logger.LogInformation("Total Applied Migrations: {Count}", appliedMigrations.Count());
        
        logger.LogInformation("Starting database seeding...");
        await DbSeeder.SeedAsync(db, logger);
        logger.LogInformation("Database seeded successfully.");
        await CatalogMigrator.MigrateAsync(db, logger);
        logger.LogInformation("=== Database Initialization Complete ===");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while migrating or seeding the database.");
        throw;
    }
}

static string MaskConnectionString(string? connectionString)
{
    if (string.IsNullOrEmpty(connectionString))
        return "Not configured";
    
    return connectionString.Contains("Password=", StringComparison.OrdinalIgnoreCase) ||
           connectionString.Contains("User Id=", StringComparison.OrdinalIgnoreCase) ||
           connectionString.Contains("UserID=", StringComparison.OrdinalIgnoreCase)
           ? "***masked***"
           : connectionString;
}

// Forwarded headers for Render proxy.
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor |
                       Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto
});

// Security headers.
app.UseMiddleware<SecurityHeadersMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Enable HTTPS redirection only when explicitly configured or in Development.
// Render terminates TLS at the proxy, so internal traffic is HTTP.
var httpsPort = builder.Configuration["HttpsPort"];
if (!string.IsNullOrEmpty(httpsPort) || app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Urls.Add($"http://0.0.0.0:{port}");

// Log Data Protection and HTTPS configuration.
var startupLogger = app.Services.GetRequiredService<ILogger<Program>>();
startupLogger.LogInformation("Data Protection keys stored at: {KeysPath}", keysDir);
startupLogger.LogInformation("Data Protection Application Name: MilkiDrugStore");
startupLogger.LogInformation("Forwarded headers enabled: XForwardedFor, XForwardedProto");
startupLogger.LogInformation("HTTPS redirection enabled: {HttpsEnabled}", !string.IsNullOrEmpty(httpsPort) || app.Environment.IsDevelopment());
if (!string.IsNullOrEmpty(httpsPort))
{
    startupLogger.LogInformation("HTTPS port: {HttpsPort}", httpsPort);
}
startupLogger.LogInformation("Application starting in {Environment} mode on port {Port}", 
    app.Environment.EnvironmentName, port);

app.Run();

public partial class Program { }
