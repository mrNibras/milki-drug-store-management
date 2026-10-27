using Microsoft.Extensions.DependencyInjection;
using MilkiDrugStore.Application.Services;
using MilkiDrugStore.Application.Interfaces;
using MilkiDrugStore.Application.Validators;
using MilkiDrugStore.Application.Behaviors;
using MilkiDrugStore.Domain.Interfaces.Repositories;
using MilkiDrugStore.Domain.Interfaces;
using MilkiDrugStore.Persistence.Repositories;
using MilkiDrugStore.Infrastructure.Services;
using MilkiDrugStore.Infrastructure.Logging;
using MediatR;
using FluentValidation;

namespace MilkiDrugStore.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IMedicineService, MedicineService>();
        services.AddScoped<ISaleService, SaleService>();
        services.AddScoped<IPurchaseService, PurchaseService>();
        services.AddScoped<ISupplierService, SupplierService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IInventoryService, InventoryService>();
        services.AddScoped<IUserActivityService, UserActivityService>();
        services.AddScoped<IBranchService, BranchService>();
        services.AddScoped<IBackupService, BackupService>();
        services.AddScoped<ICosmeticService, CosmeticService>();
        services.AddScoped<ICatalogService, CatalogService>();
        services.AddScoped<ISupplierPaymentService, SupplierPaymentService>();

        services.AddScoped<IFileStorageService>(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var provider = config.GetValue<string>("FileStorage:Provider") ?? "local";
            return provider.ToLowerInvariant() switch
            {
                "s3" => (IFileStorageService)new S3FileStorageService(
                    config, sp.GetRequiredService<ILogger<S3FileStorageService>>()),
                _ => new LocalFileStorageService(
                    config, sp.GetRequiredService<ILogger<LocalFileStorageService>>())
            };
        });

        services.AddScoped<IDatabaseMonitoringService, DatabaseMonitoringService>();
        services.AddScoped<IRetentionService, RetentionService>();

        // FluentValidation validators
        // services.AddValidatorsFromAssemblyContaining<CreateMedicineRequestValidator>();

        // MediatR pipeline behavior for command/query validation
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(BackupService).Assembly);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IMedicineRepository, MedicineRepository>();
        services.AddScoped<ISaleRepository, SaleRepository>();
        services.AddScoped<IPurchaseRepository, PurchaseRepository>();
        services.AddScoped<IInventoryTransactionRepository, InventoryTransactionRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<ICosmeticRepository, CosmeticRepository>();
        services.AddScoped<ISupplierPaymentRepository, SupplierPaymentRepository>();

        return services;
    }
}
