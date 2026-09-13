using BuildLedger.Application.Common;
using BuildLedger.Application.DTOs;
using BuildLedger.Application.Interfaces;
using BuildLedger.Core.Entities;
using BuildLedger.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BuildLedger.Infrastructure.Services;

public class SupplierService : ISupplierService
{
    private readonly BuildLedgerDbContext _db;
    private readonly IAuditService _audit;
    private readonly ICurrentUserContext _currentUser;

    public SupplierService(BuildLedgerDbContext db, IAuditService audit, ICurrentUserContext currentUser)
    {
        _db = db;
        _audit = audit;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<SupplierDto>> GetAllAsync(bool includeInactive = false, CancellationToken ct = default)
    {
        var q = _db.Suppliers.AsNoTracking().Include(s => s.Purchases).AsQueryable();
        if (!includeInactive) q = q.Where(s => s.IsActive);
        var suppliers = await q.OrderBy(s => s.Name).ToListAsync(ct);
        return suppliers.Select(s => new SupplierDto(
            s.Id, s.Name, s.Phone, s.Address,
            s.OpeningBalance + s.Purchases.Sum(p => p.Items.Sum(i => i.Quantity * i.UnitRate) - p.DiscountAmount + p.TaxAmount),
            s.IsActive)).ToList();
    }

    public async Task<ServiceResult<int>> UpsertAsync(SupplierUpsertDto dto, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Name)) return ServiceResult<int>.Fail("Supplier name is required.");

        Supplier entity;
        if (dto.Id is int id)
        {
            entity = await _db.Suppliers.FindAsync([id], ct) ?? throw new InvalidOperationException("Supplier not found.");
            entity.UpdatedAtUtc = DateTime.UtcNow;
        }
        else
        {
            entity = new Supplier();
            _db.Suppliers.Add(entity);
        }

        entity.Name = dto.Name.Trim();
        entity.Phone = dto.Phone;
        entity.Address = dto.Address;
        entity.Notes = dto.Notes;
        if (dto.Id is null) entity.OpeningBalance = dto.OpeningBalance;

        await _db.SaveChangesAsync(ct);
        await _audit.LogAsync(_currentUser.UserId, dto.Id is null ? "Create" : "Update", "Supplier", entity.Id.ToString(), null, ct);
        return ServiceResult<int>.Ok(entity.Id);
    }

    public async Task<ServiceResult> SetActiveAsync(int id, bool isActive, CancellationToken ct = default)
    {
        var s = await _db.Suppliers.FindAsync([id], ct);
        if (s is null) return ServiceResult.Fail("Supplier not found.");
        s.IsActive = isActive;
        s.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return ServiceResult.Ok();
    }
}
