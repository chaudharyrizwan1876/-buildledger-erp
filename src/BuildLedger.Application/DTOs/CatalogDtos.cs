namespace BuildLedger.Application.DTOs;

public record UnitDto(int Id, string Name, string Abbreviation, bool IsActive);
public class UnitUpsertDto { public int? Id { get; set; } public string Name { get; set; } = string.Empty; public string Abbreviation { get; set; } = string.Empty; }

public record ProductCategoryDto(int Id, string Name, bool IsActive);
public class ProductCategoryUpsertDto { public int? Id { get; set; } public string Name { get; set; } = string.Empty; }

public record ProductDto(
    int Id, string Name, int CategoryId, string CategoryName, int UnitId, string UnitAbbreviation,
    decimal PurchaseRate, decimal SaleRate, decimal MinimumStockLevel, decimal CurrentStock,
    string? Description, bool IsActive, bool IsLowStock);

public class ProductUpsertDto
{
    public int? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public int UnitId { get; set; }
    public decimal PurchaseRate { get; set; }
    public decimal SaleRate { get; set; }
    public decimal MinimumStockLevel { get; set; }
    public string? Description { get; set; }
}

public record SupplierDto(int Id, string Name, string? Phone, string? Address, decimal CurrentBalance, bool IsActive);
public class SupplierUpsertDto
{
    public int? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? Notes { get; set; }
    public decimal OpeningBalance { get; set; }
}
