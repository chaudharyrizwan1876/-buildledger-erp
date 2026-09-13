using BuildLedger.Application.Common;
using BuildLedger.Application.DTOs;
using BuildLedger.Application.Interfaces;
using BuildLedger.Core.Enums;
using BuildLedger.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BuildLedger.Infrastructure.Services;

public class ReportService : IReportService
{
    private readonly BuildLedgerDbContext _db;
    public ReportService(BuildLedgerDbContext db) => _db = db;

    public async Task<DashboardSummaryDto> GetDashboardAsync(CancellationToken ct = default)
    {
        var today = DateRange.Today();

        var todaySales = await _db.Sales.AsNoTracking()
            .Where(s => s.SaleDate >= today.From && s.SaleDate <= today.To)
            .SelectMany(s => s.Items).SumAsync(i => (decimal?)(i.Quantity * i.UnitRate), ct) ?? 0m;

        var todayPayments = await _db.Payments.AsNoTracking()
            .Where(p => p.PaymentDate >= today.From && p.PaymentDate <= today.To)
            .SumAsync(p => (decimal?)p.Amount, ct) ?? 0m;

        var balances = await _db.LedgerEntries.AsNoTracking()
            .GroupBy(l => l.CustomerId)
            .Select(g => g.Sum(l => l.Amount))
            .ToListAsync(ct);
        var totalReceivables = balances.Where(b => b > 0).Sum();
        var totalAdvances = -balances.Where(b => b < 0).Sum();

        var stockValue = await _db.Products.AsNoTracking().Where(p => p.IsActive)
            .SumAsync(p => (decimal?)(p.CurrentStock * p.PurchaseRate), ct) ?? 0m;

        var lowStockCount = await _db.Products.AsNoTracking()
            .CountAsync(p => p.IsActive && p.CurrentStock <= p.MinimumStockLevel, ct);

        var activeCustomers = await _db.Customers.AsNoTracking().CountAsync(c => c.IsActive, ct);

        var sevenDaysAgo = DateTime.Today.AddDays(-6);
        var salesTrendRaw = await _db.Sales.AsNoTracking()
            .Where(s => s.SaleDate >= sevenDaysAgo)
            .Include(s => s.Items)
            .ToListAsync(ct);
        var salesTrend = Enumerable.Range(0, 7)
            .Select(offset => sevenDaysAgo.AddDays(offset))
            .Select(day => (day, salesTrendRaw.Where(s => s.SaleDate.Date == day.Date).SelectMany(s => s.Items).Sum(i => i.Quantity * i.UnitRate)))
            .ToList();

        var topProducts = await _db.SaleItems.AsNoTracking()
            .Include(i => i.Product)
            .GroupBy(i => i.Product!.Name)
            .Select(g => new { Name = g.Key, Qty = g.Sum(i => i.Quantity) })
            .OrderByDescending(g => g.Qty)
            .Take(5)
            .ToListAsync(ct);

        var recentSales = await SalesReportAsync(new DateRange(DateTime.Now.AddDays(-30), DateTime.Now), null, null, ct);

        return new DashboardSummaryDto(
            todaySales, todayPayments, totalReceivables, totalAdvances, stockValue, lowStockCount, activeCustomers,
            salesTrend, topProducts.Select(p => (p.Name, p.Qty)).ToList(), recentSales.Take(10).ToList());
    }

    public async Task<IReadOnlyList<SaleSummaryDto>> SalesReportAsync(DateRange range, int? customerId, int? productId, CancellationToken ct = default)
    {
        var q = _db.Sales.AsNoTracking().Include(s => s.Customer).Include(s => s.Site)
            .Include(s => s.Items).Include(s => s.PaymentAllocations)
            .Where(s => s.SaleDate >= range.From && s.SaleDate <= range.To);
        if (customerId is int cid) q = q.Where(s => s.CustomerId == cid);
        if (productId is int pid) q = q.Where(s => s.Items.Any(i => i.ProductId == pid));

        var sales = await q.OrderByDescending(s => s.SaleDate).ToListAsync(ct);
        return sales.Select(s => new SaleSummaryDto(s.Id, s.InvoiceNumber, s.SaleDate,
            s.Customer?.FullName ?? string.Empty, s.Site?.SiteName, s.Total, s.AmountPaid, s.AmountDue,
            s.AmountDue <= 0 ? InvoiceStatus.Paid : (s.AmountPaid > 0 ? InvoiceStatus.PartiallyPaid : InvoiceStatus.Posted))).ToList();
    }

    public async Task<IReadOnlyList<PaymentSummaryDto>> PaymentsReportAsync(DateRange range, PaymentMethod? method, CancellationToken ct = default)
    {
        var q = _db.Payments.AsNoTracking().Include(p => p.Customer)
            .Where(p => p.PaymentDate >= range.From && p.PaymentDate <= range.To);
        if (method is PaymentMethod m) q = q.Where(p => p.Method == m);

        var payments = await q.OrderByDescending(p => p.PaymentDate).ToListAsync(ct);
        return payments.Select(p => new PaymentSummaryDto(p.Id, p.ReceiptNumber, p.PaymentDate,
            p.Customer?.FullName ?? string.Empty, p.Amount, p.Method, p.Purpose)).ToList();
    }

    public async Task<IReadOnlyList<CustomerSearchResultDto>> ReceivablesReportAsync(CancellationToken ct = default)
    {
        var customers = await _db.Customers.AsNoTracking().Where(c => c.IsActive).ToListAsync(ct);
        var result = new List<CustomerSearchResultDto>();
        foreach (var c in customers)
        {
            var balance = await _db.LedgerEntries.Where(l => l.CustomerId == c.Id).SumAsync(l => (decimal?)l.Amount, ct) ?? 0m;
            if (balance > 0)
                result.Add(new CustomerSearchResultDto(c.Id, c.FullName, c.Phone, c.AlternatePhone, c.Address, c.CustomerType, balance, c.IsActive));
        }
        return result.OrderByDescending(r => r.CurrentBalance).ToList();
    }

    public async Task<IReadOnlyList<SupplierDto>> SupplierBalancesReportAsync(CancellationToken ct = default)
    {
        var suppliers = await _db.Suppliers.AsNoTracking().Include(s => s.Purchases).ThenInclude(p => p.Items)
            .Where(s => s.IsActive).ToListAsync(ct);
        return suppliers.Select(s => new SupplierDto(s.Id, s.Name, s.Phone, s.Address,
            s.OpeningBalance + s.Purchases.Sum(p => p.Items.Sum(i => i.Quantity * i.UnitRate) - p.DiscountAmount + p.TaxAmount),
            s.IsActive)).ToList();
    }
}
