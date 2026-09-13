using BuildLedger.Application.Common;
using BuildLedger.Application.DTOs;
using BuildLedger.Application.Interfaces;
using BuildLedger.Core.Entities;
using BuildLedger.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BuildLedger.Infrastructure.Services;

public class ProductService : IProductService
{
    private readonly BuildLedgerDbContext _db;
    private readonly IAuditService _audit;
    private readonly ICurrentUserContext _currentUser;

    public ProductService(BuildLedgerDbContext db, IAuditService audit, ICurrentUserContext currentUser)
    {
        _db = db;
        _audit = audit;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<ProductDto>> GetAllAsync(bool includeInactive = false, CancellationToken ct = default)
    {
        var q = _db.Products.AsNoTracking().Include(p => p.Category).Include(p => p.Unit).AsQueryable();
        if (!includeInactive) q = q.Where(p => p.IsActive);
        var products = await q.OrderBy(p => p.Name).ToListAsync(ct);
        return products.Select(ToDto).ToList();
    }

    private static ProductDto ToDto(Product p) => new(
        p.Id, p.Name, p.CategoryId, p.Category?.Name ?? string.Empty, p.UnitId, p.Unit?.Abbreviation ?? string.Empty,
        p.PurchaseRate, p.SaleRate, p.MinimumStockLevel, p.CurrentStock, p.Description, p.IsActive,
        p.CurrentStock <= p.MinimumStockLevel);

    public async Task<ServiceResult<int>> UpsertProductAsync(ProductUpsertDto dto, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Name)) return ServiceResult<int>.Fail("Product name is required.");
        if (dto.SaleRate < 0 || dto.PurchaseRate < 0) return ServiceResult<int>.Fail("Rates cannot be negative.");

        Product entity;
        if (dto.Id is int id)
        {
            entity = await _db.Products.FindAsync([id], ct) ?? throw new InvalidOperationException("Product not found.");
            entity.UpdatedAtUtc = DateTime.UtcNow;
        }
        else
        {
            entity = new Product();
            _db.Products.Add(entity);
        }

        entity.Name = dto.Name.Trim();
        entity.CategoryId = dto.CategoryId;
        entity.UnitId = dto.UnitId;
        entity.PurchaseRate = dto.PurchaseRate;
        entity.SaleRate = dto.SaleRate;
        entity.MinimumStockLevel = dto.MinimumStockLevel;
        entity.Description = dto.Description;

        await _db.SaveChangesAsync(ct);
        await _audit.LogAsync(_currentUser.UserId, dto.Id is null ? "Create" : "Update", "Product", entity.Id.ToString(), null, ct);
        return ServiceResult<int>.Ok(entity.Id);
    }

    public async Task<ServiceResult> SetProductActiveAsync(int id, bool isActive, CancellationToken ct = default)
    {
        var p = await _db.Products.FindAsync([id], ct);
        if (p is null) return ServiceResult.Fail("Product not found.");
        p.IsActive = isActive;
        p.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return ServiceResult.Ok();
    }

    public async Task<IReadOnlyList<UnitDto>> GetUnitsAsync(CancellationToken ct = default)
    {
        var units = await _db.Units.AsNoTracking().Where(u => u.IsActive).OrderBy(u => u.Name).ToListAsync(ct);
        return units.Select(u => new UnitDto(u.Id, u.Name, u.Abbreviation, u.IsActive)).ToList();
    }

    public async Task<ServiceResult<int>> UpsertUnitAsync(UnitUpsertDto dto, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Name)) return ServiceResult<int>.Fail("Unit name is required.");
        Core.Entities.Unit entity;
        if (dto.Id is int id)
        {
            entity = await _db.Units.FindAsync([id], ct) ?? throw new InvalidOperationException("Unit not found.");
        }
        else
        {
            entity = new Core.Entities.Unit();
            _db.Units.Add(entity);
        }
        entity.Name = dto.Name.Trim();
        entity.Abbreviation = dto.Abbreviation.Trim();
        await _db.SaveChangesAsync(ct);
        return ServiceResult<int>.Ok(entity.Id);
    }

    public async Task<IReadOnlyList<ProductCategoryDto>> GetCategoriesAsync(CancellationToken ct = default)
    {
        var cats = await _db.ProductCategories.AsNoTracking().Where(c => c.IsActive).OrderBy(c => c.Name).ToListAsync(ct);
        return cats.Select(c => new ProductCategoryDto(c.Id, c.Name, c.IsActive)).ToList();
    }

    public async Task<ServiceResult<int>> UpsertCategoryAsync(ProductCategoryUpsertDto dto, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Name)) return ServiceResult<int>.Fail("Category name is required.");
        ProductCategory entity;
        if (dto.Id is int id)
        {
            entity = await _db.ProductCategories.FindAsync([id], ct) ?? throw new InvalidOperationException("Category not found.");
        }
        else
        {
            entity = new ProductCategory();
            _db.ProductCategories.Add(entity);
        }
        entity.Name = dto.Name.Trim();
        await _db.SaveChangesAsync(ct);
        return ServiceResult<int>.Ok(entity.Id);
    }

    public async Task<IReadOnlyList<ProductDto>> GetLowStockAsync(CancellationToken ct = default)
    {
        var products = await _db.Products.AsNoTracking().Include(p => p.Category).Include(p => p.Unit)
            .Where(p => p.IsActive && p.CurrentStock <= p.MinimumStockLevel)
            .OrderBy(p => p.Name).ToListAsync(ct);
        return products.Select(ToDto).ToList();
    }
}
