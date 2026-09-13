using BuildLedger.Application.Common;
using BuildLedger.Application.DTOs;
using BuildLedger.Application.Interfaces;
using BuildLedger.Core.Entities;
using BuildLedger.Core.Enums;
using BuildLedger.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BuildLedger.Infrastructure.Services;

public class PurchaseService : IPurchaseService
{
    private readonly BuildLedgerDbContext _db;
    private readonly IAuditService _audit;
    private readonly ICurrentUserContext _currentUser;

    public PurchaseService(BuildLedgerDbContext db, IAuditService audit, ICurrentUserContext currentUser)
    {
        _db = db;
        _audit = audit;
        _currentUser = currentUser;
    }

    public async Task<ServiceResult<int>> CreatePurchaseAsync(CreatePurchaseDto dto, CancellationToken ct = default)
    {
        if (dto.Lines.Count == 0) return ServiceResult<int>.Fail("A purchase must have at least one line item.");
        if (string.IsNullOrWhiteSpace(dto.InvoiceNumber)) return ServiceResult<int>.Fail("Invoice number is required.");

        await using var tx = await _db.Database.BeginTransactionAsync(ct);
        try
        {
            var purchase = new Purchase
            {
                SupplierId = dto.SupplierId,
                InvoiceNumber = dto.InvoiceNumber.Trim(),
                PurchaseDate = dto.PurchaseDate,
                DiscountAmount = dto.DiscountAmount,
                TaxAmount = dto.TaxAmount,
                Notes = dto.Notes,
                Status = InvoiceStatus.Posted
            };
            _db.Purchases.Add(purchase);
            await _db.SaveChangesAsync(ct);

            foreach (var line in dto.Lines)
            {
                if (line.Quantity <= 0) return Rollback(tx, "Quantity must be greater than zero.");

                _db.PurchaseItems.Add(new PurchaseItem
                {
                    PurchaseId = purchase.Id,
                    ProductId = line.ProductId,
                    Quantity = line.Quantity,
                    UnitRate = line.UnitRate
                });

                var product = await _db.Products.FindAsync([line.ProductId], ct);
                if (product is null) return Rollback(tx, $"Product {line.ProductId} not found.");

                product.CurrentStock += line.Quantity;
                product.UpdatedAtUtc = DateTime.UtcNow;

                _db.StockMovements.Add(new StockMovement
                {
                    ProductId = product.Id,
                    MovementType = StockMovementType.Purchase,
                    QuantityChange = line.Quantity,
                    ResultingStock = product.CurrentStock,
                    MovementDate = dto.PurchaseDate,
                    ReferenceType = nameof(Purchase),
                    ReferenceId = purchase.Id
                });
            }

            await _db.SaveChangesAsync(ct);
            await _audit.LogAsync(_currentUser.UserId, "Create", "Purchase", purchase.Id.ToString(), null, ct);
            await tx.CommitAsync(ct);

            return ServiceResult<int>.Ok(purchase.Id);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync(ct);
            return ServiceResult<int>.Fail($"Failed to save purchase: {ex.Message}");
        }
    }

    private static ServiceResult<int> Rollback(Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction tx, string message)
    {
        tx.Rollback();
        return ServiceResult<int>.Fail(message);
    }

    public async Task<IReadOnlyList<PurchaseSummaryDto>> GetAllAsync(DateRange? range = null, CancellationToken ct = default)
    {
        var q = _db.Purchases.AsNoTracking().Include(p => p.Supplier).Include(p => p.Items).AsQueryable();
        if (range is not null) q = q.Where(p => p.PurchaseDate >= range.From && p.PurchaseDate <= range.To);
        var purchases = await q.OrderByDescending(p => p.PurchaseDate).ToListAsync(ct);
        return purchases.Select(p => new PurchaseSummaryDto(p.Id, p.InvoiceNumber, p.PurchaseDate, p.Supplier?.Name ?? string.Empty, p.Total)).ToList();
    }

    public async Task<string> GenerateNextInvoiceNumberAsync(CancellationToken ct = default)
    {
        var count = await _db.Purchases.CountAsync(ct);
        return $"PUR-{DateTime.Now:yyyyMM}-{count + 1:0000}";
    }
}
