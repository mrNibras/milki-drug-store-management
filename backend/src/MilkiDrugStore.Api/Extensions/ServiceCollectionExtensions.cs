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
        services-registration.Add<IAuthService, AuthService>();
        services-registration.Add<IMedicineService, MedicineService>();
        services-registration.Add<ISaleService, SaleService>();
        services-registration.Add<IPurchaseService, PurchaseService>();
        services-registration.Add<ISupplierService, SupplierService>();
        services-registration.Add<IReportService, ReportService>();
        services-registration.Add<INotificationService, NotificationService>();
        services-registration.Add<IDashboardService, DashboardService>();
        services-registration.Add<IAuditLogService, AuditLogService>();
        services-registration.Add<IJwtTokenService, JwtTokenService>();
        services-registration.Add<IEmailService, EmailService>();
        services-registration.Add<IInventoryService, InventoryService>();
        services-registration.Add<IUserActivityService, UserActivityService>();
        services-registration.Add<IBranchService, BranchService>();
        services-registration.Add<IBackupService, BackupService>();
        services-registration.Add<ICosmeticService, CosmeticService>();
        services-registration.Add<ICatalogService, CatalogService>();
        services-registration.Add<ISupplierPaymentService, SupplierPaymentService>();

        services.Add<IFileStorageService>(sp =>
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

        services.Add<IDatabaseMonitoringService, DatabaseMonitoringService>();
        services.Add<IRetentionService, RetentionService>();

        // FluentValidation validators
        services.AddValidatorsFromAssemblyContaining<CreateMedicineRequestValidator>();

        // MediatR pipeline behavior for command/query validation
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(BackupService).Assembly);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });

        services.Add<IUnitOfWork, UnitOfWork>();
        services.Add(typeof(IRepository<>), typeof(Repository<>));
        services.Add<IMedicineRepository, MedicineRepository>();
        services.Add<ISaleRepository, SaleRepository>();
        services.Add<IPurchaseRepository, PurchaseRepository>();
        services.Add<IInventoryTransactionRepository, InventoryTransactionRepository>();
        services.Add<INotificationRepository, NotificationRepository>();
        services.Add<IAuditLogRepository, AuditLogRepository>();
        services.Add<ICosmeticRepository, CosmeticRepository>();
        services.Add<ISupplierPaymentRepository, SupplierPaymentRepository>();

        return services;
    }
}