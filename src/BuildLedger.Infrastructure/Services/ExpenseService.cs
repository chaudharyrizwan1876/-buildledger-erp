using BuildLedger.Application.Common;
using BuildLedger.Application.DTOs;
using BuildLedger.Application.Interfaces;
using BuildLedger.Core.Entities;
using BuildLedger.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BuildLedger.Infrastructure.Services;

public class ExpenseService : IExpenseService
{
    private readonly BuildLedgerDbContext _db;
    private readonly IAuditService _audit;
    private readonly ICurrentUserContext _currentUser;

    public ExpenseService(BuildLedgerDbContext db, IAuditService audit, ICurrentUserContext currentUser)
    {
        _db = db;
        _audit = audit;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<ExpenseDto>> SearchAsync(DateRange? range, int? categoryId, CancellationToken ct = default)
    {
        var q = _db.Expenses.AsNoTracking().Include(e => e.Category).AsQueryable();
        if (range is not null) q = q.Where(e => e.ExpenseDate >= range.From && e.ExpenseDate <= range.To);
        if (categoryId is int cid) q = q.Where(e => e.CategoryId == cid);

        var expenses = await q.OrderByDescending(e => e.ExpenseDate).ToListAsync(ct);
        return expenses.Select(e => new ExpenseDto(e.Id, e.ExpenseDate, e.Category?.Name ?? string.Empty,
            e.Amount, e.Method, e.Payee, e.Notes)).ToList();
    }

    public async Task<ServiceResult<int>> UpsertAsync(ExpenseUpsertDto dto, CancellationToken ct = default)
    {
        if (dto.Amount <= 0) return ServiceResult<int>.Fail("Amount must be greater than zero.");

        Expense entity;
        if (dto.Id is int id)
        {
            entity = await _db.Expenses.FindAsync([id], ct) ?? throw new InvalidOperationException("Expense not found.");
        }
        else
        {
            entity = new Expense();
            _db.Expenses.Add(entity);
        }

        entity.CategoryId = dto.CategoryId;
        entity.ExpenseDate = dto.ExpenseDate;
        entity.Amount = dto.Amount;
        entity.Method = dto.Method;
        entity.Payee = dto.Payee;
        entity.Notes = dto.Notes;

        await _db.SaveChangesAsync(ct);
        await _audit.LogAsync(_currentUser.UserId, dto.Id is null ? "Create" : "Update", "Expense", entity.Id.ToString(), null, ct);
        return ServiceResult<int>.Ok(entity.Id);
    }

    public async Task<IReadOnlyList<ExpenseCategoryDto>> GetCategoriesAsync(CancellationToken ct = default)
    {
        var cats = await _db.ExpenseCategories.AsNoTracking().Where(c => c.IsActive).OrderBy(c => c.Name).ToListAsync(ct);
        return cats.Select(c => new ExpenseCategoryDto(c.Id, c.Name, c.IsActive)).ToList();
    }

    public async Task<ServiceResult<int>> UpsertCategoryAsync(string name, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(name)) return ServiceResult<int>.Fail("Category name is required.");
        var entity = new ExpenseCategory { Name = name.Trim() };
        _db.ExpenseCategories.Add(entity);
        await _db.SaveChangesAsync(ct);
        return ServiceResult<int>.Ok(entity.Id);
    }
}
