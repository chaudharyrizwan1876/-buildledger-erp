using BuildLedger.Application.DTOs;
using BuildLedger.Infrastructure.Services;

namespace BuildLedger.Tests;

public class InventoryServiceTests
{
    [Fact]
    public async Task AdjustStock_WithReason_UpdatesStockAndLogsMovement()
    {
        using var db = new TestDb();
        var (_, _, productId) = await SeedHelper.SeedProductAsync(db.Context);
        db.Context.Products.Find(productId)!.CurrentStock = 100;
        await db.Context.SaveChangesAsync();

        var service = new InventoryService(db.Context, new AuditService(db.Context));
        var result = await service.AdjustStockAsync(new StockAdjustmentDto
        {
            ProductId = productId, NewQuantity = 80, Reason = "Damaged bags removed"
        }, userId: null);

        Assert.True(result.Success);
        var product = await db.Context.Products.FindAsync(productId);
        Assert.Equal(80m, product!.CurrentStock);

        var movement = db.Context.StockMovements.Single();
        Assert.Equal(-20m, movement.QuantityChange);
        Assert.Equal("Damaged bags removed", movement.Reason);
    }

    [Fact]
    public async Task AdjustStock_WithoutReason_IsRejected()
    {
        using var db = new TestDb();
        var (_, _, productId) = await SeedHelper.SeedProductAsync(db.Context);
        var service = new InventoryService(db.Context, new AuditService(db.Context));

        var result = await service.AdjustStockAsync(new StockAdjustmentDto { ProductId = productId, NewQuantity = 50, Reason = "" }, userId: null);

        Assert.False(result.Success);
    }
}
