using BuildLedger.Application.DTOs;
using BuildLedger.Infrastructure.Services;

namespace BuildLedger.Tests;

public class SalesServiceTests
{
    [Fact]
    public async Task CreateSale_DecreasesStock_AndRecordsMovement()
    {
        using var db = new TestDb();
        var (_, _, productId) = await SeedHelper.SeedProductAsync(db.Context);
        db.Context.Products.Find(productId)!.CurrentStock = 1000;
        await db.Context.SaveChangesAsync();
        var customerId = await SeedHelper.SeedCustomerAsync(db.Context);

        var audit = new AuditService(db.Context);
        var service = new SalesService(db.Context, audit, new FakeCurrentUserContext());

        var result = await service.CreateSaleAsync(new CreateSaleDto
        {
            CustomerId = customerId,
            InvoiceNumber = "INV-001",
            Lines = { new SaleLineDto { ProductId = productId, Quantity = 500, UnitRate = 120 } }
        });

        Assert.True(result.Success);
        var product = await db.Context.Products.FindAsync(productId);
        Assert.Equal(500m, product!.CurrentStock);
    }

    [Fact]
    public async Task CreateSale_InsufficientStock_IsRejected()
    {
        using var db = new TestDb();
        var (_, _, productId) = await SeedHelper.SeedProductAsync(db.Context);
        db.Context.Products.Find(productId)!.CurrentStock = 10;
        await db.Context.SaveChangesAsync();
        var customerId = await SeedHelper.SeedCustomerAsync(db.Context);

        var audit = new AuditService(db.Context);
        var service = new SalesService(db.Context, audit, new FakeCurrentUserContext());

        var result = await service.CreateSaleAsync(new CreateSaleDto
        {
            CustomerId = customerId,
            InvoiceNumber = "INV-002",
            Lines = { new SaleLineDto { ProductId = productId, Quantity = 500, UnitRate = 120 } }
        });

        Assert.False(result.Success);
        Assert.Contains("Insufficient stock", result.Error);
        var product = await db.Context.Products.FindAsync(productId);
        Assert.Equal(10m, product!.CurrentStock);
    }

    [Fact]
    public async Task CreateSale_AllowNegativeStock_OverridesCheck()
    {
        using var db = new TestDb();
        var (_, _, productId) = await SeedHelper.SeedProductAsync(db.Context);
        db.Context.Products.Find(productId)!.CurrentStock = 10;
        await db.Context.SaveChangesAsync();
        var customerId = await SeedHelper.SeedCustomerAsync(db.Context);

        var audit = new AuditService(db.Context);
        var service = new SalesService(db.Context, audit, new FakeCurrentUserContext());

        var result = await service.CreateSaleAsync(new CreateSaleDto
        {
            CustomerId = customerId,
            InvoiceNumber = "INV-003",
            AllowNegativeStock = true,
            Lines = { new SaleLineDto { ProductId = productId, Quantity = 50, UnitRate = 120 } }
        });

        Assert.True(result.Success);
        var product = await db.Context.Products.FindAsync(productId);
        Assert.Equal(-40m, product!.CurrentStock);
    }

    [Fact]
    public async Task CreateSale_WithFullPaymentNow_LeavesZeroDue()
    {
        using var db = new TestDb();
        var (_, _, productId) = await SeedHelper.SeedProductAsync(db.Context);
        db.Context.Products.Find(productId)!.CurrentStock = 1000;
        await db.Context.SaveChangesAsync();
        var customerId = await SeedHelper.SeedCustomerAsync(db.Context);

        var audit = new AuditService(db.Context);
        var service = new SalesService(db.Context, audit, new FakeCurrentUserContext());

        var result = await service.CreateSaleAsync(new CreateSaleDto
        {
            CustomerId = customerId,
            InvoiceNumber = "INV-004",
            AmountPaidNow = 6000,
            Lines = { new SaleLineDto { ProductId = productId, Quantity = 50, UnitRate = 120 } }
        });

        var detail = await service.GetByIdAsync(result.Data);
        Assert.Equal(6000m, detail!.Total);
        Assert.Equal(6000m, detail.AmountPaid);
        Assert.Equal(0m, detail.AmountDue);
    }

    [Fact]
    public async Task CreateSale_WithPartialPayment_LeavesRemainingDue()
    {
        using var db = new TestDb();
        var (_, _, productId) = await SeedHelper.SeedProductAsync(db.Context);
        db.Context.Products.Find(productId)!.CurrentStock = 1000;
        await db.Context.SaveChangesAsync();
        var customerId = await SeedHelper.SeedCustomerAsync(db.Context);

        var audit = new AuditService(db.Context);
        var service = new SalesService(db.Context, audit, new FakeCurrentUserContext());

        var result = await service.CreateSaleAsync(new CreateSaleDto
        {
            CustomerId = customerId,
            InvoiceNumber = "INV-005",
            AmountPaidNow = 2000,
            Lines = { new SaleLineDto { ProductId = productId, Quantity = 50, UnitRate = 120 } }
        });

        var detail = await service.GetByIdAsync(result.Data);
        Assert.Equal(6000m, detail!.Total);
        Assert.Equal(2000m, detail.AmountPaid);
        Assert.Equal(4000m, detail.AmountDue);
    }
}
