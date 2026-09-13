using BuildLedger.Application.DTOs;
using BuildLedger.Infrastructure.Services;

namespace BuildLedger.Tests;

public class PurchaseServiceTests
{
    [Fact]
    public async Task CreatePurchase_IncreasesStock_AndRecordsMovement()
    {
        using var db = new TestDb();
        var (_, _, productId) = await SeedHelper.SeedProductAsync(db.Context);
        var supplierId = await SeedHelper.SeedSupplierAsync(db.Context);
        var audit = new AuditService(db.Context);
        var service = new PurchaseService(db.Context, audit, new FakeCurrentUserContext());

        var result = await service.CreatePurchaseAsync(new CreatePurchaseDto
        {
            SupplierId = supplierId,
            InvoiceNumber = "PUR-001",
            Lines = { new PurchaseLineDto { ProductId = productId, Quantity = 1000, UnitRate = 100 } }
        });

        Assert.True(result.Success);
        var product = await db.Context.Products.FindAsync(productId);
        Assert.Equal(1000m, product!.CurrentStock);

        var movement = db.Context.StockMovements.Single();
        Assert.Equal(1000m, movement.QuantityChange);
        Assert.Equal(1000m, movement.ResultingStock);
    }

    [Fact]
    public async Task CreatePurchase_InvalidProduct_RollsBackTransaction()
    {
        using var db = new TestDb();
        var supplierId = await SeedHelper.SeedSupplierAsync(db.Context);
        var audit = new AuditService(db.Context);
        var service = new PurchaseService(db.Context, audit, new FakeCurrentUserContext());

        var result = await service.CreatePurchaseAsync(new CreatePurchaseDto
        {
            SupplierId = supplierId,
            InvoiceNumber = "PUR-002",
            Lines = { new PurchaseLineDto { ProductId = 9999, Quantity = 10, UnitRate = 100 } }
        });

        Assert.False(result.Success);
        Assert.Empty(db.Context.Purchases);
        Assert.Empty(db.Context.StockMovements);
    }

    [Fact]
    public async Task CreatePurchase_CalculatesInvoiceTotal_WithDiscountAndTax()
    {
        using var db = new TestDb();
        var (_, _, productId) = await SeedHelper.SeedProductAsync(db.Context);
        var supplierId = await SeedHelper.SeedSupplierAsync(db.Context);
        var audit = new AuditService(db.Context);
        var service = new PurchaseService(db.Context, audit, new FakeCurrentUserContext());

        await service.CreatePurchaseAsync(new CreatePurchaseDto
        {
            SupplierId = supplierId,
            InvoiceNumber = "PUR-003",
            DiscountAmount = 500,
            TaxAmount = 200,
            Lines = { new PurchaseLineDto { ProductId = productId, Quantity = 100, UnitRate = 100 } }
        });

        var purchase = db.Context.Purchases.Single();
        db.Context.Entry(purchase).Collection(p => p.Items).Load();
        Assert.Equal(10000m, purchase.SubTotal);
        Assert.Equal(9700m, purchase.Total);
    }
}
