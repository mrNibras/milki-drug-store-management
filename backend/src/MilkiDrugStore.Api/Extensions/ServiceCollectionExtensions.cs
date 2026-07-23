using Microsoft.Extensions.DependencyInjection;
using MilkiDrugStore.Application.Services;
using MilkiDrugStore.Application.Interfaces;
using MilkiDrugStore.Domain.Interfaces.Repositories;
using MilkiDrugStore.Domain.Interfaces;
using MilkiDrugStore.Persistence.Repositories;
using MilkiDrugStore.Infrastructure.Services;
using MilkiDrugStore.Infrastructure.Logging;
using MediatR;

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
        services.AddScoped<IBranchService, BranchService>();
        services.AddScoped<IBackupService, BackupService>();
        services.AddScoped<ICosmeticService, CosmeticService>();
        services.AddScoped<ICosmeticCategoryService, CosmeticCategoryService>();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(BackupService).Assembly));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IMedicineRepository, MedicineRepository>();
        services.AddScoped<ISaleRepository, SaleRepository>();
        services.AddScoped<IPurchaseRepository, PurchaseRepository>();
        services.AddScoped<IInventoryTransactionRepository, InventoryTransactionRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<ICosmeticRepository, CosmeticRepository>();
        services.AddScoped<ICosmeticCategoryRepository, CosmeticCategoryRepository>();

        return services;
    }
}
