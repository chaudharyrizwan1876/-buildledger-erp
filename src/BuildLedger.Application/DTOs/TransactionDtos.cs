using BuildLedger.Core.Enums;

namespace BuildLedger.Application.DTOs;

public class SaleLineDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitRate { get; set; }
    public decimal LineTotal => Quantity * UnitRate;
}

public class CreateSaleDto
{
    public int CustomerId { get; set; }
    public int? SiteId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime SaleDate { get; set; } = DateTime.Now;
    public decimal DiscountAmount { get; set; }
    public string? Notes { get; set; }
    public List<SaleLineDto> Lines { get; set; } = new();

    /// <summary>Optional payment collected at point of sale.</summary>
    public decimal? AmountPaidNow { get; set; }
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
    public bool AllowNegativeStock { get; set; }
}

public record SaleSummaryDto(
    int Id, string InvoiceNumber, DateTime SaleDate, string CustomerName, string? SiteName,
    decimal Total, decimal AmountPaid, decimal AmountDue, InvoiceStatus Status);

public record SaleDetailDto(
    int Id, string InvoiceNumber, DateTime SaleDate, int CustomerId, string CustomerName,
    int? SiteId, string? SiteName, decimal DiscountAmount, string? Notes,
    IReadOnlyList<SaleLineDto> Lines, decimal SubTotal, decimal Total, decimal AmountPaid, decimal AmountDue);

public class PurchaseLineDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitRate { get; set; }
    public decimal LineTotal => Quantity * UnitRate;
}

public class CreatePurchaseDto
{
    public int SupplierId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime PurchaseDate { get; set; } = DateTime.Now;
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public string? Notes { get; set; }
    public List<PurchaseLineDto> Lines { get; set; } = new();
}

public record PurchaseSummaryDto(
    int Id, string InvoiceNumber, DateTime PurchaseDate, string SupplierName, decimal Total);

public class RecordPaymentDto
{
    public int CustomerId { get; set; }
    public int? SiteId { get; set; }
    public string ReceiptNumber { get; set; } = string.Empty;
    public DateTime PaymentDate { get; set; } = DateTime.Now;
    public decimal Amount { get; set; }
    public PaymentMethod Method { get; set; }
    public PaymentPurpose Purpose { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }
    /// <summary>When Purpose == AgainstInvoice, which sale(s) to allocate to and how much.</summary>
    public List<(int SaleId, decimal Amount)> Allocations { get; set; } = new();
}

public record PaymentSummaryDto(
    int Id, string ReceiptNumber, DateTime PaymentDate, string CustomerName, decimal Amount,
    PaymentMethod Method, PaymentPurpose Purpose);
