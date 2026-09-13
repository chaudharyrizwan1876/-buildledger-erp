using BuildLedger.Core.Enums;

namespace BuildLedger.Core.Entities;

public class Sale : AuditableEntity
{
    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public int? SiteId { get; set; }
    public Site? Site { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime SaleDate { get; set; } = DateTime.Now;
    public decimal DiscountAmount { get; set; }
    public string? Notes { get; set; }
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Posted;

    public List<SaleItem> Items { get; set; } = new();
    public List<PaymentAllocation> PaymentAllocations { get; set; } = new();

    public decimal SubTotal => Items.Sum(i => i.Quantity * i.UnitRate);
    public decimal Total => SubTotal - DiscountAmount;
    public decimal AmountPaid => PaymentAllocations.Sum(a => a.AllocatedAmount);
    public decimal AmountDue => Total - AmountPaid;
}

public class SaleItem : AuditableEntity
{
    public int SaleId { get; set; }
    public Sale? Sale { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitRate { get; set; }
    public decimal LineTotal => Quantity * UnitRate;
}
