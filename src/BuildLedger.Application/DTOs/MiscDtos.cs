using BuildLedger.Core.Enums;

namespace BuildLedger.Application.DTOs;

public record StockMovementDto(
    int Id, DateTime MovementDate, string ProductName, StockMovementType MovementType,
    decimal QuantityChange, decimal ResultingStock, string? Reason);

public class StockAdjustmentDto
{
    public int ProductId { get; set; }
    public decimal NewQuantity { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public record ExpenseCategoryDto(int Id, string Name, bool IsActive);
public record ExpenseDto(int Id, DateTime ExpenseDate, string CategoryName, decimal Amount, PaymentMethod Method, string? Payee, string? Notes);
public class ExpenseUpsertDto
{
    public int? Id { get; set; }
    public int CategoryId { get; set; }
    public DateTime ExpenseDate { get; set; } = DateTime.Now;
    public decimal Amount { get; set; }
    public PaymentMethod Method { get; set; }
    public string? Payee { get; set; }
    public string? Notes { get; set; }
}

public record DashboardSummaryDto(
    decimal TodaySales, decimal TodayPaymentsReceived, decimal TotalReceivables, decimal TotalAdvances,
    decimal CurrentStockValue, int LowStockProductCount, int ActiveCustomerCount,
    IReadOnlyList<(DateTime Date, decimal Amount)> SalesTrend7Days,
    IReadOnlyList<(string ProductName, decimal QuantitySold)> TopSellingProducts,
    IReadOnlyList<SaleSummaryDto> RecentSales);

public record LoginResultDto(bool Success, string? Error, int UserId, string FullName, SystemRole Role);

public record BackupHistoryDto(int Id, DateTime BackupDateUtc, string FilePath, long FileSizeBytes, BackupStatus Status, bool IsManual);

public record UserDto(int Id, string FullName, string Username, string RoleName, bool IsActive, DateTime? LastLoginAtUtc);
public class UserUpsertDto
{
    public int? Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string? Password { get; set; }
    public int RoleId { get; set; }
}
