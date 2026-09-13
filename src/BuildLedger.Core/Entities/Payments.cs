using BuildLedger.Core.Enums;

namespace BuildLedger.Core.Entities;

public class Payment : AuditableEntity
{
    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public int? SiteId { get; set; }
    public Site? Site { get; set; }
    public string ReceiptNumber { get; set; } = string.Empty;
    public DateTime PaymentDate { get; set; } = DateTime.Now;
    public decimal Amount { get; set; }
    public PaymentMethod Method { get; set; }
    public PaymentPurpose Purpose { get; set; }
    public PaymentDirection Direction { get; set; } = PaymentDirection.Received;
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }

    public List<PaymentAllocation> Allocations { get; set; } = new();

    public decimal AllocatedAmount => Allocations.Sum(a => a.AllocatedAmount);
    public decimal UnallocatedAmount => Amount - AllocatedAmount;
}

/// <summary>Links a Payment to the specific Sale invoice(s) it settles. Unallocated amount = advance/credit.</summary>
public class PaymentAllocation : AuditableEntity
{
    public int PaymentId { get; set; }
    public Payment? Payment { get; set; }
    public int SaleId { get; set; }
    public Sale? Sale { get; set; }
    public decimal AllocatedAmount { get; set; }
}

/// <summary>Chronological, append-only ledger row backing customer/site balance reporting.</summary>
public class LedgerEntry : AuditableEntity
{
    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public int? SiteId { get; set; }
    public Site? Site { get; set; }
    public DateTime EntryDate { get; set; } = DateTime.Now;
    public LedgerEntryType EntryType { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    /// <summary>Positive = increases what customer owes (debit). Negative = payment/credit.</summary>
    public decimal Amount { get; set; }
    public decimal RunningBalance { get; set; }
    public string? Notes { get; set; }
}
