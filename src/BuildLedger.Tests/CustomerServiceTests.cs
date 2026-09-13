using BuildLedger.Application.DTOs;
using BuildLedger.Core.Enums;
using BuildLedger.Infrastructure.Services;

namespace BuildLedger.Tests;

public class CustomerServiceTests
{
    [Fact]
    public async Task CreateCustomer_WithOpeningBalance_CreatesLedgerEntry()
    {
        using var db = new TestDb();
        var audit = new AuditService(db.Context);
        var service = new CustomerService(db.Context, audit, new FakeCurrentUserContext());

        var result = await service.UpsertAsync(new CustomerUpsertDto
        {
            FullName = "Ahmed Traders",
            Phone = "03001234567",
            CustomerType = CustomerType.Contractor,
            OpeningBalance = 5000m
        });

        Assert.True(result.Success);
        var balance = await service.GetBalanceAsync(result.Data);
        Assert.Equal(5000m, balance);
    }

    [Fact]
    public async Task CreateCustomer_DuplicatePhone_IsRejected()
    {
        using var db = new TestDb();
        var audit = new AuditService(db.Context);
        var service = new CustomerService(db.Context, audit, new FakeCurrentUserContext());

        await service.UpsertAsync(new CustomerUpsertDto { FullName = "Ali", Phone = "0300", CustomerType = CustomerType.Individual });
        var second = await service.UpsertAsync(new CustomerUpsertDto { FullName = "Bilal", Phone = "0300", CustomerType = CustomerType.Individual });

        Assert.False(second.Success);
    }

    [Fact]
    public async Task Customer_CanHaveMultipleSites_WithIndependentBalances()
    {
        using var db = new TestDb();
        var audit = new AuditService(db.Context);
        var service = new CustomerService(db.Context, audit, new FakeCurrentUserContext());

        var customer = await service.UpsertAsync(new CustomerUpsertDto { FullName = "Khan Constructions", Phone = "0311", CustomerType = CustomerType.ConstructionCompany });
        var site1 = await service.UpsertSiteAsync(new SiteUpsertDto { CustomerId = customer.Data, SiteName = "Site A" });
        var site2 = await service.UpsertSiteAsync(new SiteUpsertDto { CustomerId = customer.Data, SiteName = "Site B" });

        var sites = await service.GetSitesForCustomerAsync(customer.Data);
        Assert.Equal(2, sites.Count);
        Assert.Contains(sites, s => s.Id == site1.Data);
        Assert.Contains(sites, s => s.Id == site2.Data);
    }

    [Fact]
    public async Task Search_PreventsAccidentalDuplicate_ByFindingExistingMatch()
    {
        using var db = new TestDb();
        var audit = new AuditService(db.Context);
        var service = new CustomerService(db.Context, audit, new FakeCurrentUserContext());

        await service.UpsertAsync(new CustomerUpsertDto { FullName = "Zafar Steel", Phone = "0333222111", CustomerType = CustomerType.Individual });

        var byPhone = await service.SearchAsync("0333222111");
        var byName = await service.SearchAsync("Zafar");

        Assert.Single(byPhone);
        Assert.Single(byName);
    }
}
