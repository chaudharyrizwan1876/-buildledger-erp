using BuildLedger.Application.Common;
using BuildLedger.Application.DTOs;
using BuildLedger.Core.Enums;

namespace BuildLedger.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResultDto> LoginAsync(string username, string password, CancellationToken ct = default);
    Task<ServiceResult> ChangePasswordAsync(int userId, string currentPassword, string newPassword, CancellationToken ct = default);
}

public interface ICustomerService
{
    Task<IReadOnlyList<CustomerSearchResultDto>> SearchAsync(string query, CancellationToken ct = default);
    Task<CustomerDetailDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ServiceResult<int>> UpsertAsync(CustomerUpsertDto dto, CancellationToken ct = default);
    Task<ServiceResult> SetActiveAsync(int id, bool isActive, CancellationToken ct = default);
    Task<ServiceResult<int>> UpsertSiteAsync(SiteUpsertDto dto, CancellationToken ct = default);
    Task<IReadOnlyList<SiteDto>> GetSitesForCustomerAsync(int customerId, CancellationToken ct = default);
    Task<IReadOnlyList<LedgerRowDto>> GetLedgerAsync(int customerId, int? siteId, DateRange? range, CancellationToken ct = default);
    Task<decimal> GetBalanceAsync(int customerId, int? siteId = null, CancellationToken ct = default);
}

public interface IProductService
{
    Task<IReadOnlyList<ProductDto>> GetAllAsync(bool includeInactive = false, CancellationToken ct = default);
    Task<ServiceResult<int>> UpsertProductAsync(ProductUpsertDto dto, CancellationToken ct = default);
    Task<ServiceResult> SetProductActiveAsync(int id, bool isActive, CancellationToken ct = default);
    Task<IReadOnlyList<UnitDto>> GetUnitsAsync(CancellationToken ct = default);
    Task<ServiceResult<int>> UpsertUnitAsync(UnitUpsertDto dto, CancellationToken ct = default);
    Task<IReadOnlyList<ProductCategoryDto>> GetCategoriesAsync(CancellationToken ct = default);
    Task<ServiceResult<int>> UpsertCategoryAsync(ProductCategoryUpsertDto dto, CancellationToken ct = default);
    Task<IReadOnlyList<ProductDto>> GetLowStockAsync(CancellationToken ct = default);
}

public interface ISupplierService
{
    Task<IReadOnlyList<SupplierDto>> GetAllAsync(bool includeInactive = false, CancellationToken ct = default);
    Task<ServiceResult<int>> UpsertAsync(SupplierUpsertDto dto, CancellationToken ct = default);
    Task<ServiceResult> SetActiveAsync(int id, bool isActive, CancellationToken ct = default);
}

public interface IPurchaseService
{
    Task<ServiceResult<int>> CreatePurchaseAsync(CreatePurchaseDto dto, CancellationToken ct = default);
    Task<IReadOnlyList<PurchaseSummaryDto>> GetAllAsync(DateRange? range = null, CancellationToken ct = default);
    Task<string> GenerateNextInvoiceNumberAsync(CancellationToken ct = default);
}

public interface ISalesService
{
    Task<ServiceResult<int>> CreateSaleAsync(CreateSaleDto dto, CancellationToken ct = default);
    Task<SaleDetailDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<SaleSummaryDto>> SearchAsync(DateRange? range, int? customerId, int? siteId, CancellationToken ct = default);
    Task<string> GenerateNextInvoiceNumberAsync(CancellationToken ct = default);
}

public interface IPaymentService
{
    Task<ServiceResult<int>> RecordPaymentAsync(RecordPaymentDto dto, CancellationToken ct = default);
    Task<IReadOnlyList<PaymentSummaryDto>> SearchAsync(DateRange? range, int? customerId, CancellationToken ct = default);
    Task<string> GenerateNextReceiptNumberAsync(CancellationToken ct = default);
}

public interface IInventoryService
{
    Task<IReadOnlyList<StockMovementDto>> GetMovementsAsync(int? productId, DateRange? range, CancellationToken ct = default);
    Task<ServiceResult> AdjustStockAsync(StockAdjustmentDto dto, int? userId, CancellationToken ct = default);
    Task<decimal> GetStockValuationAsync(CancellationToken ct = default);
}

public interface IExpenseService
{
    Task<IReadOnlyList<ExpenseDto>> SearchAsync(DateRange? range, int? categoryId, CancellationToken ct = default);
    Task<ServiceResult<int>> UpsertAsync(ExpenseUpsertDto dto, CancellationToken ct = default);
    Task<IReadOnlyList<ExpenseCategoryDto>> GetCategoriesAsync(CancellationToken ct = default);
    Task<ServiceResult<int>> UpsertCategoryAsync(string name, CancellationToken ct = default);
}

public interface IBackupService
{
    Task<ServiceResult<string>> CreateBackupAsync(string? destinationFolder = null, bool isManual = true, CancellationToken ct = default);
    Task<ServiceResult> RestoreBackupAsync(string backupFilePath, CancellationToken ct = default);
    Task<IReadOnlyList<BackupHistoryDto>> GetHistoryAsync(CancellationToken ct = default);
}

public interface IReportService
{
    Task<DashboardSummaryDto> GetDashboardAsync(CancellationToken ct = default);
    Task<IReadOnlyList<SaleSummaryDto>> SalesReportAsync(DateRange range, int? customerId, int? productId, CancellationToken ct = default);
    Task<IReadOnlyList<PaymentSummaryDto>> PaymentsReportAsync(DateRange range, PaymentMethod? method, CancellationToken ct = default);
    Task<IReadOnlyList<CustomerSearchResultDto>> ReceivablesReportAsync(CancellationToken ct = default);
    Task<IReadOnlyList<SupplierDto>> SupplierBalancesReportAsync(CancellationToken ct = default);
}

public interface IUserService
{
    Task<IReadOnlyList<UserDto>> GetAllAsync(CancellationToken ct = default);
    Task<ServiceResult<int>> UpsertAsync(UserUpsertDto dto, CancellationToken ct = default);
    Task<ServiceResult> SetActiveAsync(int id, bool isActive, CancellationToken ct = default);
    Task<IReadOnlyList<(int Id, string Name)>> GetRolesAsync(CancellationToken ct = default);
}

public interface IAuditService
{
    Task LogAsync(int? userId, string action, string entityName, string? entityId, string? details, CancellationToken ct = default);
}

public interface ISettingsService
{
    Task<string?> GetAsync(string key, CancellationToken ct = default);
    Task SetAsync(string key, string value, CancellationToken ct = default);
    Task<Dictionary<string, string>> GetAllAsync(CancellationToken ct = default);
}

public interface ICurrentUserContext
{
    int? UserId { get; }
    string? FullName { get; }
    SystemRole? Role { get; }
    void SignIn(int userId, string fullName, SystemRole role);
    void SignOut();
}
