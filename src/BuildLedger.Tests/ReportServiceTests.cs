using BuildLedger.Application.Common;
using BuildLedger.Application.DTOs;
using BuildLedger.Infrastructure.Services;

namespace BuildLedger.Tests;

public class ReportServiceTests
{
    [Fact]
    public async Task SalesReport_FiltersByDateRange()
    {
        using var db = new TestDb();
        var (_, _, productId) = await SeedHelper.SeedProductAsync(db.Context);
        db.Context.Products.Find(productId)!.CurrentStock = 1000;
        await db.Context.SaveChangesAsync();
        var customerId = await SeedHelper.SeedCustomerAsync(db.Context);

        var audit = new AuditService(db.Context);
        var salesService = new SalesService(db.Context, audit, new FakeCurrentUserContext());
        var reportService = new ReportService(db.Context);

        await salesService.CreateSaleAsync(new CreateSaleDto
        {
            CustomerId = customerId, InvoiceNumber = "OLD-1", SaleDate = DateTime.Now.AddDays(-30),
            Lines = { new SaleLineDto { ProductId = productId, Quantity = 10, UnitRate = 100 } }
        });
        await salesService.CreateSaleAsync(new CreateSaleDto
        {
            CustomerId = customerId, InvoiceNumber = "NEW-1", SaleDate = DateTime.Now,
            Lines = { new SaleLineDto { ProductId = productId, Quantity = 10, UnitRate = 100 } }
        });

        var range = new DateRange(DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1));
        var report = await reportService.SalesReportAsync(range, null, null);

        Assert.Single(report);
        Assert.Equal("NEW-1", report[0].InvoiceNumber);
    }

    [Fact]
    public async Task ReceivablesReport_OnlyIncludesCustomersWithPositiveBalance()
    {
        using var db = new TestDb();
        var customerId = await SeedHelper.SeedCustomerAsync(db.Context, "Owes Money", "0300-A");
        var customer = await db.Context.Customers.FindAsync(customerId);
        customer!.OpeningBalance = 5000;
        db.Context.LedgerEntries.Add(new BuildLedger.Core.Entities.LedgerEntry
        {
            CustomerId = customerId, Amount = 5000, RunningBalance = 5000,
            EntryType = BuildLedger.Core.Enums.LedgerEntryType.OpeningBalance, ReferenceNumber = "OPENING"
        });
        await db.Context.SaveChangesAsync();

        await SeedHelper.SeedCustomerAsync(db.Context, "No Balance", "0300-B");

        var reportService = new ReportService(db.Context);
        var receivables = await reportService.ReceivablesReportAsync();

        Assert.Single(receivables);
        Assert.Equal("Owes Money", receivables[0].FullName);
    }
}
