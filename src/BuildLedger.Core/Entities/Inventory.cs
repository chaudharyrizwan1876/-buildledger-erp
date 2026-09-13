using BuildLedger.Core.Enums;

namespace BuildLedger.Core.Entities;

/// <summary>Immutable audit trail of every stock change. CurrentStock on Product is a projection of this log.</summary>
public class StockMovement : AuditableEntity
{
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public StockMovementType MovementType { get; set; }
    /// <summary>Positive = stock in, negative = stock out.</summary>
    public decimal QuantityChange { get; set; }
    public decimal ResultingStock { get; set; }
    public DateTime MovementDate { get; set; } = DateTime.Now;
    public string? ReferenceType { get; set; }
    public int? ReferenceId { get; set; }
    public string? Reason { get; set; }
}

public class ExpenseCategory : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public List<Expense> Expenses { get; set; } = new();
}

public class Expense : AuditableEntity
{
    public int CategoryId { get; set; }
    public ExpenseCategory? Category { get; set; }
    public DateTime ExpenseDate { get; set; } = DateTime.Now;
    public decimal Amount { get; set; }
    public PaymentMethod Method { get; set; }
    public string? Payee { get; set; }
    public string? Notes { get; set; }
    public ExpenseStatus Status { get; set; } = ExpenseStatus.Recorded;
}

public class BackupHistory : AuditableEntity
{
    public DateTime BackupDateUtc { get; set; } = DateTime.UtcNow;
    public string FilePath { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public BackupStatus Status { get; set; }
    public string? ErrorMessage { get; set; }
    public bool IsManual { get; set; }
}
