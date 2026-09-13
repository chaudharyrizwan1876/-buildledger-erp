using BuildLedger.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BuildLedger.Infrastructure.Persistence.Configurations;

public class RoleConfig : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> b)
    {
        b.Property(x => x.Name).IsRequired().HasMaxLength(50);
        b.HasIndex(x => x.Name).IsUnique();
        b.HasMany(x => x.Permissions).WithMany(x => x.Roles)
            .UsingEntity(j => j.ToTable("RolePermissions"));
    }
}

public class UserConfig : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> b)
    {
        b.Property(x => x.Username).IsRequired().HasMaxLength(50);
        b.HasIndex(x => x.Username).IsUnique();
        b.Property(x => x.FullName).IsRequired().HasMaxLength(100);
        b.HasOne(x => x.Role).WithMany(x => x.Users).HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class CustomerConfig : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> b)
    {
        b.Property(x => x.FullName).IsRequired().HasMaxLength(150);
        b.Property(x => x.Phone).IsRequired().HasMaxLength(30);
        b.Property(x => x.OpeningBalance).HasColumnType("decimal(18,2)");
        b.HasIndex(x => x.Phone);
        b.HasIndex(x => x.FullName);
    }
}

public class SiteConfig : IEntityTypeConfiguration<Site>
{
    public void Configure(EntityTypeBuilder<Site> b)
    {
        b.Property(x => x.SiteName).IsRequired().HasMaxLength(150);
        b.HasOne(x => x.Customer).WithMany(x => x.Sites).HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => x.SiteName);
    }
}

public class ProductConfig : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> b)
    {
        b.Property(x => x.Name).IsRequired().HasMaxLength(150);
        b.Property(x => x.PurchaseRate).HasColumnType("decimal(18,2)");
        b.Property(x => x.SaleRate).HasColumnType("decimal(18,2)");
        b.Property(x => x.MinimumStockLevel).HasColumnType("decimal(18,3)");
        b.Property(x => x.CurrentStock).HasColumnType("decimal(18,3)");
        b.HasOne(x => x.Category).WithMany(x => x.Products).HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Unit).WithMany(x => x.Products).HasForeignKey(x => x.UnitId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => x.Name);
    }
}

public class SupplierConfig : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> b)
    {
        b.Property(x => x.Name).IsRequired().HasMaxLength(150);
        b.Property(x => x.OpeningBalance).HasColumnType("decimal(18,2)");
        b.HasIndex(x => x.Name);
    }
}

public class PurchaseConfig : IEntityTypeConfiguration<Purchase>
{
    public void Configure(EntityTypeBuilder<Purchase> b)
    {
        b.Property(x => x.InvoiceNumber).IsRequired().HasMaxLength(50);
        b.Property(x => x.DiscountAmount).HasColumnType("decimal(18,2)");
        b.Property(x => x.TaxAmount).HasColumnType("decimal(18,2)");
        b.HasOne(x => x.Supplier).WithMany(x => x.Purchases).HasForeignKey(x => x.SupplierId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => x.InvoiceNumber);
        b.HasIndex(x => x.PurchaseDate);
        b.Ignore(x => x.SubTotal);
        b.Ignore(x => x.Total);
    }
}

public class PurchaseItemConfig : IEntityTypeConfiguration<PurchaseItem>
{
    public void Configure(EntityTypeBuilder<PurchaseItem> b)
    {
        b.Property(x => x.Quantity).HasColumnType("decimal(18,3)");
        b.Property(x => x.UnitRate).HasColumnType("decimal(18,2)");
        b.HasOne(x => x.Purchase).WithMany(x => x.Items).HasForeignKey(x => x.PurchaseId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
        b.Ignore(x => x.LineTotal);
    }
}

public class SaleConfig : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> b)
    {
        b.Property(x => x.InvoiceNumber).IsRequired().HasMaxLength(50);
        b.Property(x => x.DiscountAmount).HasColumnType("decimal(18,2)");
        b.HasOne(x => x.Customer).WithMany(x => x.Sales).HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Site).WithMany(x => x.Sales).HasForeignKey(x => x.SiteId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => x.InvoiceNumber);
        b.HasIndex(x => x.SaleDate);
        b.Ignore(x => x.SubTotal);
        b.Ignore(x => x.Total);
        b.Ignore(x => x.AmountPaid);
        b.Ignore(x => x.AmountDue);
    }
}

public class SaleItemConfig : IEntityTypeConfiguration<SaleItem>
{
    public void Configure(EntityTypeBuilder<SaleItem> b)
    {
        b.Property(x => x.Quantity).HasColumnType("decimal(18,3)");
        b.Property(x => x.UnitRate).HasColumnType("decimal(18,2)");
        b.HasOne(x => x.Sale).WithMany(x => x.Items).HasForeignKey(x => x.SaleId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
        b.Ignore(x => x.LineTotal);
    }
}

public class PaymentConfig : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> b)
    {
        b.Property(x => x.ReceiptNumber).IsRequired().HasMaxLength(50);
        b.Property(x => x.Amount).HasColumnType("decimal(18,2)");
        b.HasOne(x => x.Customer).WithMany(x => x.Payments).HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Site).WithMany().HasForeignKey(x => x.SiteId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => x.ReceiptNumber);
        b.HasIndex(x => x.PaymentDate);
        b.Ignore(x => x.AllocatedAmount);
        b.Ignore(x => x.UnallocatedAmount);
    }
}

public class PaymentAllocationConfig : IEntityTypeConfiguration<PaymentAllocation>
{
    public void Configure(EntityTypeBuilder<PaymentAllocation> b)
    {
        b.Property(x => x.AllocatedAmount).HasColumnType("decimal(18,2)");
        b.HasOne(x => x.Payment).WithMany(x => x.Allocations).HasForeignKey(x => x.PaymentId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Sale).WithMany(x => x.PaymentAllocations).HasForeignKey(x => x.SaleId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class LedgerEntryConfig : IEntityTypeConfiguration<LedgerEntry>
{
    public void Configure(EntityTypeBuilder<LedgerEntry> b)
    {
        b.Property(x => x.Amount).HasColumnType("decimal(18,2)");
        b.Property(x => x.RunningBalance).HasColumnType("decimal(18,2)");
        b.HasOne(x => x.Customer).WithMany(x => x.LedgerEntries).HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Site).WithMany(x => x.LedgerEntries).HasForeignKey(x => x.SiteId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => x.EntryDate);
        b.HasIndex(x => x.CustomerId);
    }
}

public class StockMovementConfig : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> b)
    {
        b.Property(x => x.QuantityChange).HasColumnType("decimal(18,3)");
        b.Property(x => x.ResultingStock).HasColumnType("decimal(18,3)");
        b.HasOne(x => x.Product).WithMany(x => x.StockMovements).HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => x.MovementDate);
        b.HasIndex(x => x.ProductId);
    }
}

public class ExpenseConfig : IEntityTypeConfiguration<Expense>
{
    public void Configure(EntityTypeBuilder<Expense> b)
    {
        b.Property(x => x.Amount).HasColumnType("decimal(18,2)");
        b.HasOne(x => x.Category).WithMany(x => x.Expenses).HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => x.ExpenseDate);
    }
}

public class AppSettingConfig : IEntityTypeConfiguration<AppSetting>
{
    public void Configure(EntityTypeBuilder<AppSetting> b)
    {
        b.Property(x => x.Key).IsRequired().HasMaxLength(100);
        b.HasIndex(x => x.Key).IsUnique();
    }
}
