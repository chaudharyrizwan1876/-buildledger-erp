using BuildLedger.Application.Common;
using BuildLedger.Application.DTOs;
using BuildLedger.Application.Interfaces;
using BuildLedger.Core.Entities;
using BuildLedger.Core.Enums;
using BuildLedger.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BuildLedger.Infrastructure.Services;

public class InventoryService : IInventoryService
{
    private readonly BuildLedgerDbContext _db;
    private readonly IAuditService _audit;

    public InventoryService(BuildLedgerDbContext db, IAuditService audit)
    {
        _db = db;
        _audit = audit;
    }

    public async Task<IReadOnlyList<StockMovementDto>> GetMovementsAsync(int? productId, DateRange? range, CancellationToken ct = default)
    {
        var q = _db.StockMovements.AsNoTracking().Include(m => m.Product).AsQueryable();
        if (productId is int pid) q = q.Where(m => m.ProductId == pid);
        if (range is not null) q = q.Where(m => m.MovementDate >= range.From && m.MovementDate <= range.To);

        var movements = await q.OrderByDescending(m => m.MovementDate).ToListAsync(ct);
        return movements.Select(m => new StockMovementDto(m.Id, m.MovementDate, m.Product?.Name ?? string.Empty,
            m.MovementType, m.QuantityChange, m.ResultingStock, m.Reason)).ToList();
    }

    public async Task<ServiceResult> AdjustStockAsync(StockAdjustmentDto dto, int? userId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Reason)) return ServiceResult.Fail("A reason is required for stock adjustments.");

        var product = await _db.Products.FindAsync([dto.ProductId], ct);
        if (product is null) return ServiceResult.Fail("Product not found.");

        var change = dto.NewQuantity - product.CurrentStock;
        if (change == 0) return ServiceResult.Fail("New quantity is the same as current stock; nothing to adjust.");

        product.CurrentStock = dto.NewQuantity;
        product.UpdatedAtUtc = DateTime.UtcNow;

        _db.StockMovements.Add(new StockMovement
        {
            ProductId = product.Id,
            MovementType = change > 0 ? StockMovementType.AdjustmentIncrease : StockMovementType.AdjustmentDecrease,
            QuantityChange = change,
            ResultingStock = product.CurrentStock,
            MovementDate = DateTime.Now,
            ReferenceType = "ManualAdjustment",
            Reason = dto.Reason
        });

        await _db.SaveChangesAsync(ct);
        await _audit.LogAsync(userId, "StockAdjustment", "Product", product.Id.ToString(), dto.Reason, ct);
        return ServiceResult.Ok();
    }

    public async Task<decimal> GetStockValuationAsync(CancellationToken ct = default)
    {
        return await _db.Products.AsNoTracking().Where(p => p.IsActive)
            .SumAsync(p => (decimal?)(p.CurrentStock * p.PurchaseRate), ct) ?? 0m;
    }
}
