using MilkiDrugStore.Api;
using MilkiDrugStore.Api.Middlewares;
using MilkiDrugStore.Api.Extensions;
using MilkiDrugStore.Application.Mappings;
using MilkiDrugStore.Infrastructure.BackgroundJobs;
using MilkiDrugStore.Infrastructure.Logging;
using MilkiDrugStore.Persistence.Repositories;
using MilkiDrugStore.Persistence.Context;
using MilkiDrugStore.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddApplicationServices();

LoggerConfiguration.ConfigureLogger(builder.Logging);

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(jwtSettings["Secret"] ?? "SuperSecretKey12345SuperSecretKey12345")),
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DbSeeder.SeedAsync(db);
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseJwtMiddleware();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
