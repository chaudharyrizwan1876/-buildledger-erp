using BuildLedger.Core.Entities;
using BuildLedger.Infrastructure.Persistence;

namespace BuildLedger.Tests;

public static class SeedHelper
{
    public static async Task<(int UnitId, int CategoryId, int ProductId)> SeedProductAsync(
        BuildLedgerDbContext db, string name = "Cement", decimal purchaseRate = 100, decimal saleRate = 120, decimal minStock = 10)
    {
        var unit = new Unit { Name = "Bag", Abbreviation = "bag" };
        var category = new ProductCategory { Name = "Cement" };
        db.Units.Add(unit);
        db.ProductCategories.Add(category);
        await db.SaveChangesAsync();

        var product = new Product
        {
            Name = name, UnitId = unit.Id, CategoryId = category.Id,
            PurchaseRate = purchaseRate, SaleRate = saleRate, MinimumStockLevel = minStock
        };
        db.Products.Add(product);
        await db.SaveChangesAsync();

        return (unit.Id, category.Id, product.Id);
    }

    public static async Task<int> SeedSupplierAsync(BuildLedgerDbContext db, string name = "ABC Supplies")
    {
        var supplier = new Supplier { Name = name };
        db.Suppliers.Add(supplier);
        await db.SaveChangesAsync();
        return supplier.Id;
    }

    public static async Task<int> SeedCustomerAsync(BuildLedgerDbContext db, string name = "Test Customer", string phone = "0300")
    {
        var customer = new Customer { FullName = name, Phone = phone, CustomerType = Core.Enums.CustomerType.Individual };
        db.Customers.Add(customer);
        await db.SaveChangesAsync();
        return customer.Id;
    }
}
