using BuildLedger.Application.Common;
using BuildLedger.Application.DTOs;
using BuildLedger.Application.Interfaces;
using BuildLedger.Core.Entities;
using BuildLedger.Core.Enums;
using BuildLedger.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BuildLedger.Infrastructure.Services;

public class SalesService : ISalesService
{
    private readonly BuildLedgerDbContext _db;
    private readonly IAuditService _audit;
    private readonly ICurrentUserContext _currentUser;

    public SalesService(BuildLedgerDbContext db, IAuditService audit, ICurrentUserContext currentUser)
    {
        _db = db;
        _audit = audit;
        _currentUser = currentUser;
    }

    public async Task<ServiceResult<int>> CreateSaleAsync(CreateSaleDto dto, CancellationToken ct = default)
    {
        if (dto.Lines.Count == 0) return ServiceResult<int>.Fail("A sale must have at least one line item.");
        if (string.IsNullOrWhiteSpace(dto.InvoiceNumber)) return ServiceResult<int>.Fail("Invoice number is required.");

        // Validate stock availability up front (unless explicitly overridden).
        if (!dto.AllowNegativeStock)
        {
            foreach (var line in dto.Lines)
            {
                var product = await _db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == line.ProductId, ct);
                if (product is null) return ServiceResult<int>.Fail($"Product {line.ProductId} not found.");
                if (product.CurrentStock < line.Quantity)
                    return ServiceResult<int>.Fail($"Insufficient stock for '{product.Name}'. Available: {product.CurrentStock}, requested: {line.Quantity}.");
            }
        }

        await using var tx = await _db.Database.BeginTransactionAsync(ct);
        try
        {
            var sale = new Sale
            {
                CustomerId = dto.CustomerId,
                SiteId = dto.SiteId,
                InvoiceNumber = dto.InvoiceNumber.Trim(),
                SaleDate = dto.SaleDate,
                DiscountAmount = dto.DiscountAmount,
                Notes = dto.Notes,
                Status = InvoiceStatus.Posted
            };
            _db.Sales.Add(sale);
            await _db.SaveChangesAsync(ct);

            decimal subTotal = 0;
            foreach (var line in dto.Lines)
            {
                if (line.Quantity <= 0)
                {
                    await tx.RollbackAsync(ct);
                    return ServiceResult<int>.Fail("Quantity must be greater than zero.");
                }

                var product = await _db.Products.FindAsync([line.ProductId], ct);
                if (product is null)
                {
                    await tx.RollbackAsync(ct);
                    return ServiceResult<int>.Fail($"Product {line.ProductId} not found.");
                }

                if (!dto.AllowNegativeStock && product.CurrentStock < line.Quantity)
                {
                    await tx.RollbackAsync(ct);
                    return ServiceResult<int>.Fail($"Insufficient stock for '{product.Name}'.");
                }

                _db.SaleItems.Add(new SaleItem
                {
                    SaleId = sale.Id,
                    ProductId = line.ProductId,
                    Quantity = line.Quantity,
                    UnitRate = line.UnitRate
                });
                subTotal += line.Quantity * line.UnitRate;

                product.CurrentStock -= line.Quantity;
                product.UpdatedAtUtc = DateTime.UtcNow;

                _db.StockMovements.Add(new StockMovement
                {
                    ProductId = product.Id,
                    MovementType = StockMovementType.Sale,
                    QuantityChange = -line.Quantity,
                    ResultingStock = product.CurrentStock,
                    MovementDate = dto.SaleDate,
                    ReferenceType = nameof(Sale),
                    ReferenceId = sale.Id
                });
            }

            var total = subTotal - dto.DiscountAmount;

            var previousBalance = await GetRunningBalanceAsync(dto.CustomerId, ct);
            var newBalance = previousBalance + total;
            _db.LedgerEntries.Add(new LedgerEntry
            {
                CustomerId = dto.CustomerId,
                SiteId = dto.SiteId,
                EntryDate = dto.SaleDate,
                EntryType = LedgerEntryType.Sale,
                ReferenceNumber = sale.InvoiceNumber,
                Amount = total,
                RunningBalance = newBalance,
                Notes = $"Sale invoice {sale.InvoiceNumber}"
            });

            await _db.SaveChangesAsync(ct);

            if (dto.AmountPaidNow is decimal paidNow && paidNow > 0)
            {
                var payment = new Payment
                {
                    CustomerId = dto.CustomerId,
                    SiteId = dto.SiteId,
                    ReceiptNumber = $"RCT-{sale.InvoiceNumber}",
                    PaymentDate = dto.SaleDate,
                    Amount = paidNow,
                    Method = dto.PaymentMethod,
                    Purpose = PaymentPurpose.AgainstInvoice,
                    Direction = PaymentDirection.Received,
                    Notes = "Collected at point of sale"
                };
                _db.Payments.Add(payment);
                await _db.SaveChangesAsync(ct);

                _db.PaymentAllocations.Add(new PaymentAllocation
                {
                    PaymentId = payment.Id,
                    SaleId = sale.Id,
                    AllocatedAmount = Math.Min(paidNow, total)
                });

                var balanceAfterPayment = newBalance - paidNow;
                _db.LedgerEntries.Add(new LedgerEntry
                {
                    CustomerId = dto.CustomerId,
                    SiteId = dto.SiteId,
                    EntryDate = dto.SaleDate,
                    EntryType = LedgerEntryType.Payment,
                    ReferenceNumber = payment.ReceiptNumber,
                    Amount = -paidNow,
                    RunningBalance = balanceAfterPayment,
                    Notes = "Payment collected at point of sale"
                });

                await _db.SaveChangesAsync(ct);
            }

            await _audit.LogAsync(_currentUser.UserId, "Create", "Sale", sale.Id.ToString(), null, ct);
            await tx.CommitAsync(ct);
            return ServiceResult<int>.Ok(sale.Id);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync(ct);
            return ServiceResult<int>.Fail($"Failed to save sale: {ex.Message}");
        }
    }

    private async Task<decimal> GetRunningBalanceAsync(int customerId, CancellationToken ct)
        => await _db.LedgerEntries.Where(l => l.CustomerId == customerId).SumAsync(l => (decimal?)l.Amount, ct) ?? 0m;

    public async Task<SaleDetailDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var sale = await _db.Sales.AsNoTracking()
            .Include(s => s.Customer).Include(s => s.Site)
            .Include(s => s.Items).ThenInclude(i => i.Product).ThenInclude(p => p!.Unit)
            .Include(s => s.PaymentAllocations)
            .FirstOrDefaultAsync(s => s.Id == id, ct);
        if (sale is null) return null;

        var lines = sale.Items.Select(i => new SaleLineDto
        {
            ProductId = i.ProductId,
            ProductName = i.Product?.Name ?? string.Empty,
            Unit = i.Product?.Unit?.Abbreviation ?? string.Empty,
            Quantity = i.Quantity,
            UnitRate = i.UnitRate
        }).ToList();

        return new SaleDetailDto(sale.Id, sale.InvoiceNumber, sale.SaleDate, sale.CustomerId,
            sale.Customer?.FullName ?? string.Empty, sale.SiteId, sale.Site?.SiteName, sale.DiscountAmount,
            sale.Notes, lines, sale.SubTotal, sale.Total, sale.AmountPaid, sale.AmountDue);
    }

    public async Task<IReadOnlyList<SaleSummaryDto>> SearchAsync(DateRange? range, int? customerId, int? siteId, CancellationToken ct = default)
    {
        var q = _db.Sales.AsNoTracking().Include(s => s.Customer).Include(s => s.Site)
            .Include(s => s.PaymentAllocations).AsQueryable();
        if (range is not null) q = q.Where(s => s.SaleDate >= range.From && s.SaleDate <= range.To);
        if (customerId is int cid) q = q.Where(s => s.CustomerId == cid);
        if (siteId is int sid) q = q.Where(s => s.SiteId == sid);

        var sales = await q.OrderByDescending(s => s.SaleDate).ToListAsync(ct);
        return sales.Select(s => new SaleSummaryDto(s.Id, s.InvoiceNumber, s.SaleDate,
            s.Customer?.FullName ?? string.Empty, s.Site?.SiteName, s.Total, s.AmountPaid, s.AmountDue,
            s.AmountDue <= 0 ? InvoiceStatus.Paid : (s.AmountPaid > 0 ? InvoiceStatus.PartiallyPaid : InvoiceStatus.Posted))).ToList();
    }

    public async Task<string> GenerateNextInvoiceNumberAsync(CancellationToken ct = default)
    {
        var count = await _db.Sales.CountAsync(ct);
        return $"INV-{DateTime.Now:yyyyMM}-{count + 1:0000}";
    }
}
