namespace BuildLedger.Core.Entities;

public class Unit : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string Abbreviation { get; set; } = string.Empty;
    public List<Product> Products { get; set; } = new();
}

public class ProductCategory : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public List<Product> Products { get; set; } = new();
}

public class Product : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public ProductCategory? Category { get; set; }
    public int UnitId { get; set; }
    public Unit? Unit { get; set; }
    public decimal PurchaseRate { get; set; }
    public decimal SaleRate { get; set; }
    public decimal MinimumStockLevel { get; set; }
    public string? Description { get; set; }

    /// <summary>Denormalized running on-hand quantity, maintained transactionally via StockMovements.</summary>
    public decimal CurrentStock { get; set; }

    public List<StockMovement> StockMovements { get; set; } = new();
}

public class Supplier : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? Notes { get; set; }
    /// <summary>Opening balance the business owes this supplier (credit-positive).</summary>
    public decimal OpeningBalance { get; set; }

    public List<Purchase> Purchases { get; set; } = new();
}
