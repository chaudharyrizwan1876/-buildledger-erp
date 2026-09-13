using BuildLedger.Application.DTOs;
using BuildLedger.Infrastructure.Services;

namespace BuildLedger.Tests;

public class ProductServiceTests
{
    [Fact]
    public async Task CreateProduct_WithValidData_Succeeds()
    {
        using var db = new TestDb();
        var (unitId, categoryId, _) = await SeedHelper.SeedProductAsync(db.Context, name: "Existing");
        var audit = new AuditService(db.Context);
        var service = new ProductService(db.Context, audit, new FakeCurrentUserContext());

        var result = await service.UpsertProductAsync(new ProductUpsertDto
        {
            Name = "Sariya 10mm", CategoryId = categoryId, UnitId = unitId,
            PurchaseRate = 250, SaleRate = 280, MinimumStockLevel = 500
        });

        Assert.True(result.Success);
        var products = await service.GetAllAsync();
        Assert.Contains(products, p => p.Name == "Sariya 10mm");
    }

    [Fact]
    public async Task CreateProduct_MissingName_Fails()
    {
        using var db = new TestDb();
        var (unitId, categoryId, _) = await SeedHelper.SeedProductAsync(db.Context);
        var service = new ProductService(db.Context, new AuditService(db.Context), new FakeCurrentUserContext());

        var result = await service.UpsertProductAsync(new ProductUpsertDto { Name = "", CategoryId = categoryId, UnitId = unitId });

        Assert.False(result.Success);
    }

    [Fact]
    public async Task LowStockProducts_AreDetected_WhenBelowMinimum()
    {
        using var db = new TestDb();
        var (_, _, productId) = await SeedHelper.SeedProductAsync(db.Context, minStock: 100);
        db.Context.Products.Find(productId)!.CurrentStock = 5;
        await db.Context.SaveChangesAsync();

        var service = new ProductService(db.Context, new AuditService(db.Context), new FakeCurrentUserContext());
        var lowStock = await service.GetLowStockAsync();

        Assert.Single(lowStock);
    }
}
