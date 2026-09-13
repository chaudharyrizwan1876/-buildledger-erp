using BuildLedger.Core.Entities;
using BuildLedger.Core.Enums;
using BuildLedger.Infrastructure.Persistence;
using BuildLedger.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;

namespace BuildLedger.Infrastructure.Seed;

/// <summary>Applies pending migrations and seeds first-run reference data (roles, default admin, base units/categories).</summary>
public static class DbInitializer
{
    public static async Task InitializeAsync(BuildLedgerDbContext db, CancellationToken ct = default)
    {
        await db.Database.MigrateAsync(ct);

        if (!await db.Roles.AnyAsync(ct))
        {
            var owner = new Role { Name = "Owner", SystemRole = SystemRole.Owner };
            var admin = new Role { Name = "Admin", SystemRole = SystemRole.Admin };
            var cashier = new Role { Name = "Cashier", SystemRole = SystemRole.Cashier };
            db.Roles.AddRange(owner, admin, cashier);
            await db.SaveChangesAsync(ct);

            db.Users.Add(new User
            {
                FullName = "Administrator",
                Username = "admin",
                PasswordHash = PasswordHasher.Hash("admin123"),
                RoleId = owner.Id
            });
            await db.SaveChangesAsync(ct);
        }

        if (!await db.Units.AnyAsync(ct))
        {
            db.Units.AddRange(
                new Core.Entities.Unit { Name = "Bag", Abbreviation = "bag" },
                new Core.Entities.Unit { Name = "Kilogram", Abbreviation = "kg" },
                new Core.Entities.Unit { Name = "Ton", Abbreviation = "ton" },
                new Core.Entities.Unit { Name = "Piece", Abbreviation = "pcs" },
                new Core.Entities.Unit { Name = "Truckload", Abbreviation = "truck" },
                new Core.Entities.Unit { Name = "Cubic Feet", Abbreviation = "cft" });
            await db.SaveChangesAsync(ct);
        }

        if (!await db.ProductCategories.AnyAsync(ct))
        {
            db.ProductCategories.AddRange(
                new ProductCategory { Name = "Steel" },
                new ProductCategory { Name = "Cement" },
                new ProductCategory { Name = "Aggregates" },
                new ProductCategory { Name = "Masonry" },
                new ProductCategory { Name = "Other" });
            await db.SaveChangesAsync(ct);
        }

        if (!await db.ExpenseCategories.AnyAsync(ct))
        {
            db.ExpenseCategories.AddRange(
                new ExpenseCategory { Name = "Rent" },
                new ExpenseCategory { Name = "Utilities" },
                new ExpenseCategory { Name = "Salaries" },
                new ExpenseCategory { Name = "Transport" },
                new ExpenseCategory { Name = "Miscellaneous" });
            await db.SaveChangesAsync(ct);
        }

        if (!await db.AppSettings.AnyAsync(ct))
        {
            db.AppSettings.AddRange(
                new AppSetting { Key = "BusinessName", Value = "BuildLedger Construction Supplies" },
                new AppSetting { Key = "AllowNegativeStock", Value = "false" },
                new AppSetting { Key = "AutoBackupEnabled", Value = "true" },
                new AppSetting { Key = "AutoBackupTime", Value = "20:00" },
                new AppSetting { Key = "Theme", Value = "Light" });
            await db.SaveChangesAsync(ct);
        }
    }
}
