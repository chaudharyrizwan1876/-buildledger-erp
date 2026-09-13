using BuildLedger.Application.DTOs;
using BuildLedger.Core.Enums;
using BuildLedger.Infrastructure.Services;

namespace BuildLedger.Tests;

public class PaymentServiceTests
{
    [Fact]
    public async Task AdvancePayment_ProducesNegativeBalance_MeaningCredit()
    {
        using var db = new TestDb();
        var customerId = await SeedHelper.SeedCustomerAsync(db.Context);
        var audit = new AuditService(db.Context);
        var customerService = new CustomerService(db.Context, audit, new FakeCurrentUserContext());
        var paymentService = new PaymentService(db.Context, audit, new FakeCurrentUserContext());

        var result = await paymentService.RecordPaymentAsync(new RecordPaymentDto
        {
            CustomerId = customerId,
            ReceiptNumber = "RCT-001",
            Amount = 3000,
            Method = PaymentMethod.Cash,
            Purpose = PaymentPurpose.Advance
        });

        Assert.True(result.Success);
        var balance = await customerService.GetBalanceAsync(customerId);
        Assert.Equal(-3000m, balance);
    }

    [Fact]
    public async Task PaymentAgainstInvoice_ReducesSaleAmountDue()
    {
        using var db = new TestDb();
        var (_, _, productId) = await SeedHelper.SeedProductAsync(db.Context);
        db.Context.Products.Find(productId)!.CurrentStock = 1000;
        await db.Context.SaveChangesAsync();
        var customerId = await SeedHelper.SeedCustomerAsync(db.Context);

        var audit = new AuditService(db.Context);
        var salesService = new SalesService(db.Context, audit, new FakeCurrentUserContext());
        var paymentService = new PaymentService(db.Context, audit, new FakeCurrentUserContext());

        var sale = await salesService.CreateSaleAsync(new CreateSaleDto
        {
            CustomerId = customerId,
            InvoiceNumber = "INV-100",
            Lines = { new SaleLineDto { ProductId = productId, Quantity = 50, UnitRate = 120 } }
        });

        var payment = await paymentService.RecordPaymentAsync(new RecordPaymentDto
        {
            CustomerId = customerId,
            ReceiptNumber = "RCT-100",
            Amount = 4000,
            Method = PaymentMethod.Cash,
            Purpose = PaymentPurpose.AgainstInvoice,
            Allocations = { (sale.Data, 4000m) }
        });

        Assert.True(payment.Success);
        var detail = await salesService.GetByIdAsync(sale.Data);
        Assert.Equal(2000m, detail!.AmountDue);
    }

    [Fact]
    public async Task PaymentAgainstBalance_ReducesCustomerBalance()
    {
        using var db = new TestDb();
        var customerId = await SeedHelper.SeedCustomerAsync(db.Context);
        var audit = new AuditService(db.Context);
        var customerService = new CustomerService(db.Context, audit, new FakeCurrentUserContext());
        var paymentService = new PaymentService(db.Context, audit, new FakeCurrentUserContext());

        // Set an opening balance directly to simulate an existing debt, then pay part of it off.
        var customer = await db.Context.Customers.FindAsync(customerId);
        customer!.OpeningBalance = 10000;
        db.Context.LedgerEntries.Add(new BuildLedger.Core.Entities.LedgerEntry
        {
            CustomerId = customerId, Amount = 10000, RunningBalance = 10000,
            EntryType = LedgerEntryType.OpeningBalance, ReferenceNumber = "OPENING"
        });
        await db.Context.SaveChangesAsync();

        await paymentService.RecordPaymentAsync(new RecordPaymentDto
        {
            CustomerId = customerId,
            ReceiptNumber = "RCT-200",
            Amount = 4000,
            Method = PaymentMethod.BankTransfer,
            Purpose = PaymentPurpose.AgainstBalance
        });

        var balance = await customerService.GetBalanceAsync(customerId);
        Assert.Equal(6000m, balance);
    }
}
