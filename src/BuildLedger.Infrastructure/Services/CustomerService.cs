using BuildLedger.Application.Common;
using BuildLedger.Application.DTOs;
using BuildLedger.Application.Interfaces;
using BuildLedger.Core.Entities;
using BuildLedger.Core.Enums;
using BuildLedger.Core.Exceptions;
using BuildLedger.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BuildLedger.Infrastructure.Services;

public class CustomerService : ICustomerService
{
    private readonly BuildLedgerDbContext _db;
    private readonly IAuditService _audit;
    private readonly ICurrentUserContext _currentUser;

    public CustomerService(BuildLedgerDbContext db, IAuditService audit, ICurrentUserContext currentUser)
    {
        _db = db;
        _audit = audit;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<CustomerSearchResultDto>> SearchAsync(string query, CancellationToken ct = default)
    {
        var q = _db.Customers.AsNoTracking().Where(c => c.IsActive);
        if (!string.IsNullOrWhiteSpace(query))
        {
            var term = query.Trim();
            q = q.Where(c => c.FullName.Contains(term) || c.Phone.Contains(term)
                || (c.AlternatePhone != null && c.AlternatePhone.Contains(term))
                || c.Id.ToString() == term
                || c.Sites.Any(s => s.SiteName.Contains(term)));
        }

        var customers = await q.OrderBy(c => c.FullName).Take(50).ToListAsync(ct);
        var result = new List<CustomerSearchResultDto>();
        foreach (var c in customers)
        {
            var balance = await GetBalanceAsync(c.Id, null, ct);
            result.Add(new CustomerSearchResultDto(c.Id, c.FullName, c.Phone, c.AlternatePhone, c.Address, c.CustomerType, balance, c.IsActive));
        }
        return result;
    }

    public async Task<CustomerDetailDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var c = await _db.Customers.AsNoTracking().Include(x => x.Sites)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (c is null) return null;

        var balance = await GetBalanceAsync(id, null, ct);
        var sites = new List<SiteDto>();
        foreach (var s in c.Sites)
        {
            var siteBalance = await GetBalanceAsync(id, s.Id, ct);
            sites.Add(new SiteDto(s.Id, s.CustomerId, s.SiteName, s.SiteAddress, s.ProjectType, s.ContactPerson, siteBalance, s.IsActive));
        }

        return new CustomerDetailDto(c.Id, c.FullName, c.Phone, c.AlternatePhone, c.Address, c.CustomerType,
            c.Notes, c.OpeningBalance, balance, c.IsActive, c.CreatedAtUtc, sites);
    }

    public async Task<ServiceResult<int>> UpsertAsync(CustomerUpsertDto dto, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.FullName)) return ServiceResult<int>.Fail("Full name is required.");
        if (string.IsNullOrWhiteSpace(dto.Phone)) return ServiceResult<int>.Fail("Phone number is required.");

        var duplicate = await _db.Customers.AnyAsync(c =>
            c.Phone == dto.Phone && c.Id != (dto.Id ?? 0), ct);
        if (duplicate) return ServiceResult<int>.Fail(new DuplicateCustomerException(dto.Phone).Message);

        Customer entity;
        if (dto.Id is int id)
        {
            entity = await _db.Customers.FindAsync([id], ct) ?? throw new DomainException("Customer not found.");
            entity.UpdatedAtUtc = DateTime.UtcNow;
        }
        else
        {
            entity = new Customer();
            _db.Customers.Add(entity);
        }

        entity.FullName = dto.FullName.Trim();
        entity.Phone = dto.Phone.Trim();
        entity.AlternatePhone = dto.AlternatePhone;
        entity.Address = dto.Address;
        entity.CustomerType = dto.CustomerType;
        entity.Notes = dto.Notes;

        var isNew = dto.Id is null;
        if (isNew) entity.OpeningBalance = dto.OpeningBalance;

        await _db.SaveChangesAsync(ct);

        if (isNew && dto.OpeningBalance != 0)
        {
            _db.LedgerEntries.Add(new LedgerEntry
            {
                CustomerId = entity.Id,
                EntryDate = entity.CreatedAtUtc,
                EntryType = LedgerEntryType.OpeningBalance,
                ReferenceNumber = "OPENING",
                Amount = dto.OpeningBalance,
                RunningBalance = dto.OpeningBalance,
                Notes = "Opening balance on customer creation"
            });
            await _db.SaveChangesAsync(ct);
        }

        await _audit.LogAsync(_currentUser.UserId, isNew ? "Create" : "Update", "Customer", entity.Id.ToString(), null, ct);
        return ServiceResult<int>.Ok(entity.Id);
    }

    public async Task<ServiceResult> SetActiveAsync(int id, bool isActive, CancellationToken ct = default)
    {
        var c = await _db.Customers.FindAsync([id], ct);
        if (c is null) return ServiceResult.Fail("Customer not found.");
        c.IsActive = isActive;
        c.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        await _audit.LogAsync(_currentUser.UserId, isActive ? "Activate" : "Deactivate", "Customer", id.ToString(), null, ct);
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult<int>> UpsertSiteAsync(SiteUpsertDto dto, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.SiteName)) return ServiceResult<int>.Fail("Site name is required.");

        Site entity;
        if (dto.Id is int id)
        {
            entity = await _db.Sites.FindAsync([id], ct) ?? throw new DomainException("Site not found.");
            entity.UpdatedAtUtc = DateTime.UtcNow;
        }
        else
        {
            entity = new Site { CustomerId = dto.CustomerId };
            _db.Sites.Add(entity);
        }

        entity.SiteName = dto.SiteName.Trim();
        entity.SiteAddress = dto.SiteAddress;
        entity.ProjectType = dto.ProjectType;
        entity.ContactPerson = dto.ContactPerson;
        entity.Notes = dto.Notes;

        await _db.SaveChangesAsync(ct);
        await _audit.LogAsync(_currentUser.UserId, dto.Id is null ? "Create" : "Update", "Site", entity.Id.ToString(), null, ct);
        return ServiceResult<int>.Ok(entity.Id);
    }

    public async Task<IReadOnlyList<SiteDto>> GetSitesForCustomerAsync(int customerId, CancellationToken ct = default)
    {
        var sites = await _db.Sites.AsNoTracking().Where(s => s.CustomerId == customerId && s.IsActive).ToListAsync(ct);
        var result = new List<SiteDto>();
        foreach (var s in sites)
        {
            var bal = await GetBalanceAsync(customerId, s.Id, ct);
            result.Add(new SiteDto(s.Id, s.CustomerId, s.SiteName, s.SiteAddress, s.ProjectType, s.ContactPerson, bal, s.IsActive));
        }
        return result;
    }

    public async Task<IReadOnlyList<LedgerRowDto>> GetLedgerAsync(int customerId, int? siteId, DateRange? range, CancellationToken ct = default)
    {
        var q = _db.LedgerEntries.AsNoTracking().Where(l => l.CustomerId == customerId);
        if (siteId is int sid) q = q.Where(l => l.SiteId == sid);
        if (range is not null) q = q.Where(l => l.EntryDate >= range.From && l.EntryDate <= range.To);

        var rows = await q.OrderBy(l => l.EntryDate).ThenBy(l => l.Id).ToListAsync(ct);
        return rows.Select(l => new LedgerRowDto(l.Id, l.EntryDate, l.EntryType, l.ReferenceNumber, l.Amount, l.RunningBalance, l.Notes)).ToList();
    }

    public async Task<decimal> GetBalanceAsync(int customerId, int? siteId = null, CancellationToken ct = default)
    {
        var q = _db.LedgerEntries.AsNoTracking().Where(l => l.CustomerId == customerId);
        if (siteId is int sid) q = q.Where(l => l.SiteId == sid);
        return await q.SumAsync(l => (decimal?)l.Amount, ct) ?? 0m;
    }
}
