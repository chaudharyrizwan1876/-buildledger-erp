using BuildLedger.Core.Enums;

namespace BuildLedger.Core.Entities;

public class Purchase : AuditableEntity
{
    public int SupplierId { get; set; }
    public Supplier? Supplier { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime PurchaseDate { get; set; } = DateTime.Now;
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public string? Notes { get; set; }
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Posted;

    public List<PurchaseItem> Items { get; set; } = new();

    public decimal SubTotal => Items.Sum(i => i.Quantity * i.UnitRate);
    public decimal Total => SubTotal - DiscountAmount + TaxAmount;
}

public class PurchaseItem : AuditableEntity
{
    public int PurchaseId { get; set; }
    public Purchase? Purchase { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitRate { get; set; }
    public decimal LineTotal => Quantity * UnitRate;
}
