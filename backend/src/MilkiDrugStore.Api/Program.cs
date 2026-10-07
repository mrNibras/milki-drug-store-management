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

// Ensure data directory exists for Data Protection keys and file uploads.
// Render mounts the Persistent Disk at /var/data, so everything stored
// under this path survives container restarts and redeployments.
var dataDir = builder.Configuration.GetValue<string>("DataDirectory") ?? "/var/data";

// Validate and prepare the data directory.
var dataDirInfo = new DirectoryInfo(dataDir);
try
{
    dataDirInfo.Create();
    dataDirInfo.Refresh();
}
catch (Exception ex)
{
    Console.Error.WriteLine($"FATAL: Failed to create data directory at {dataDir}. Application cannot start. Error: {ex.Message}");
    throw;
}

// Validate that the data directory is writable.
var dataDirTestPath = Path.Combine(dataDir, ".write-test");
try
{
    File.WriteAllText(dataDirTestPath, string.Empty);
    File.Delete(dataDirTestPath);
}
catch (Exception ex)
{
    Console.Error.WriteLine($"FATAL: Data directory at {dataDir} is not writable. Application cannot start. Error: {ex.Message}");
    throw;
}

// Prepare Data Protection key directory under the persistent disk.
var keysDir = Path.Combine(dataDir, "keys");
var keysDirInfo = new DirectoryInfo(keysDir);
try
{
    keysDirInfo.Create();
    keysDirInfo.Refresh();
}
catch (Exception ex)
{
    Console.Error.WriteLine($"FATAL: Failed to create Data Protection key directory at {keysDir}. Application cannot start. Error: {ex.Message}");
    throw;
}

// Validate that the key directory is writable.
var keysDirTestPath = Path.Combine(keysDir, ".write-test");
try
{
    File.WriteAllText(keysDirTestPath, string.Empty);
    File.Delete(keysDirTestPath);
}
catch (Exception ex)
{
    Console.Error.WriteLine($"FATAL: Data Protection key directory at {keysDir} is not writable. Application cannot start. Error: {ex.Message}");
    throw;
}

// Data Protection configuration.
// Keys are persisted to the Render Persistent Disk at /var/data/keys.
// This ensures JWT authentication cookies, CSRF tokens, and other
// protected payloads survive container restarts.
//
// Warning: No XML encryptor is configured. This means keys are stored
// in unencrypted form on disk. This is acceptable for this deployment
// because:
//   1. Render Persistent Disks are encrypted at rest by the Render platform.
//   2. The keys protect ASP.NET Core Data Protection payloads (cookies,
//      CSRF tokens), not direct authentication secrets.
//   3. The main security boundary is Render's platform-level access control.
//
// If you need key encryption (e.g., for compliance requirements), the
// recommended production solution is to provision an X.509 certificate
// and call .ProtectKeysWithCertificate(cert) here. For Render, this would
// require mounting the certificate as a secret file or environment variable.
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(keysDirInfo)
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

    builder.Services.AddOptions<DatabaseMonitoringOptions>()
        .Bind(builder.Configuration.GetSection("DatabaseMonitoring"))
        .ValidateOnStart();

    builder.Services.AddOptions<RetentionOptions>()
        .Bind(builder.Configuration.GetSection("Retention"))
        .ValidateOnStart();

    // Database provider auto-detection.
    var connectionString = DbProviderResolver.ResolveConnectionString(builder.Configuration);
    builder.Services.AddDbContext<AppDbContext>(options =>
    {
        DbProviderResolver.ConfigureAppDbContext(options, builder.Configuration);
    });

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddApplicationServices();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<MilkiDrugStore.Application.Interfaces.ICurrentUserService, MilkiDrugStore.Api.Services.CurrentUserService>();

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
                System.Text.Encoding.UTF8.GetBytes(jwtSettings.Secret))
            {
                KeyId = "milki-signing-key"
            },
            ValidateIssuer = !string.IsNullOrEmpty(jwtSettings.Issuer),
            ValidateAudience = !string.IsNullOrEmpty(jwtSettings.Audience),
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            ClockSkew = TimeSpan.Zero,
            RoleClaimType = System.Security.Claims.ClaimTypes.Role,
            NameClaimType = System.Security.Claims.ClaimTypes.NameIdentifier
        };
        options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                // Structured so a 401 seen in production can be attributed to a
                // concrete reason (expired / bad signature / bad issuer / unknown
                // key). Never logs the token or any secret.
                var logger = context.HttpContext.RequestServices
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger("MilkiDrugStore.Auth");
                logger.LogWarning("JWT rejected. Reason={Reason} Path={Path} Message={Message}",
                    context.Exception?.GetType().Name,
                    context.HttpContext.Request.Path,
                    context.Exception?.Message);
                return System.Threading.Tasks.Task.CompletedTask;
            },
            // Single centralized revocation point: an already-issued JWT stays
            // cryptographically valid until expiry, so every authenticated
            // request re-checks the authoritative User.IsActive state.
            OnTokenValidated = async context =>
            {
                var principal = context.Principal;
                if (principal?.Identity?.IsAuthenticated != true)
                    return;

                var logger = context.HttpContext.RequestServices
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger("MilkiDrugStore.Auth");

                var userIdClaim = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out var userId))
                {
                    logger.LogWarning("JWT rejected: subject claim missing or not an integer. Path={Path}",
                        context.HttpContext.Request.Path);
                    context.Fail("Invalid token subject.");
                    return;
                }

                var userActivity = context.HttpContext.RequestServices
                    .GetRequiredService<MilkiDrugStore.Application.Interfaces.IUserActivityService>();

                if (!await userActivity.IsActiveAsync(userId, context.HttpContext.RequestAborted))
                {
                    logger.LogWarning("JWT rejected: user {UserId} is not active/approved. Path={Path}",
                        userId, context.HttpContext.Request.Path);
                    context.Fail("Account is no longer active.");
                }
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("PharmacistAllowed", policy => policy.RequireRole("Admin", "Pharmacist"));
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
builder.Services.AddHostedService<MilkiDrugStore.Infrastructure.BackgroundJobs.RetentionCleanupBackgroundService>();

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
        var isRelational = db.Database.IsRelational();

        logger.LogInformation("=== Database Initialization Starting ===");
        logger.LogInformation("Context Type: {ContextType}", contextType);
        logger.LogInformation("Database Provider: {Provider}", provider);
        logger.LogInformation("Migration Assembly: MilkiDrugStore.Persistence");
        logger.LogInformation("Connection String: {ConnectionString}",
            MaskConnectionString(connectionString));

        if (isRelational)
        {
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
        }
        else
        {
            logger.LogInformation("Non-relational database provider detected. Skipping migrations.");
        }

        logger.LogInformation("Starting database seeding...");
        await DbSeeder.SeedAsync(db, logger);
        logger.LogInformation("Database seeded successfully.");
        await CatalogMigrator.MigrateAsync(db, logger);
        Task backfillMedicineTask = Task.CompletedTask;
        if (isRelational)
        {
            backfillMedicineTask = BackfillMedicineFieldsAsync(db, logger);
        }
        await backfillMedicineTask;
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

static async Task BackfillMedicineFieldsAsync(AppDbContext db, ILogger logger)
{
    var medicines = await db.Medicines
        .Where(m => string.IsNullOrEmpty(m.ProductCode) || m.UpdatedDate == null)
        .ToListAsync();

    if (medicines.Count == 0)
    {
        logger.LogInformation("No medicine fields to backfill.");
        return;
    }

    logger.LogInformation("Backfilling fields for {Count} medicine(s)...", medicines.Count);

    foreach (var medicine in medicines)
    {
        if (string.IsNullOrEmpty(medicine.ProductCode))
            medicine.ProductCode = $"MED-{medicine.ProductId:D6}";

        if (medicine.UpdatedDate == null)
            medicine.UpdatedDate = medicine.CreatedDate;
    }

    await db.SaveChangesAsync();
    logger.LogInformation("Medicine field backfill completed.");
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
startupLogger.LogInformation("=== Data Protection Configuration ===");
startupLogger.LogInformation("Data Protection key path: {KeysPath}", keysDir);
startupLogger.LogInformation("Data Protection key directory exists: {Exists}", keysDirInfo.Exists);
startupLogger.LogInformation("Data Protection Application Name: MilkiDrugStore");
startupLogger.LogInformation("Data Protection key lifetime: 90 days");
startupLogger.LogInformation("Data Protection key encryption: None (unencrypted XML)");
startupLogger.LogInformation("Data Directory: {DataDirectory} (exists: {Exists}, writable: true)", dataDir, dataDirInfo.Exists);
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
