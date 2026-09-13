using BuildLedger.Application.Interfaces;
using BuildLedger.Infrastructure.Backup;
using BuildLedger.Infrastructure.Persistence;
using BuildLedger.Infrastructure.Security;
using BuildLedger.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BuildLedger.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBuildLedgerInfrastructure(this IServiceCollection services, string dbPath, string backupFolder)
    {
        services.AddDbContext<BuildLedgerDbContext>(opt => opt.UseSqlite($"Data Source={dbPath}"));

        services.AddSingleton<ICurrentUserContext, CurrentUserContext>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ISupplierService, SupplierService>();
        services.AddScoped<IPurchaseService, PurchaseService>();
        services.AddScoped<ISalesService, SalesService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IInventoryService, InventoryService>();
        services.AddScoped<IExpenseService, ExpenseService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ISettingsService, SettingsService>();
        services.AddScoped<IBackupService>(sp => new BackupService(sp.GetRequiredService<BuildLedgerDbContext>(), backupFolder));

        return services;
    }
}
