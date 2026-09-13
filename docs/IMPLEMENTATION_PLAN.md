# BuildLedger ERP — Implementation Plan

## 0. Environment inspection results
- Folder was empty; fresh build.
- .NET SDK installed: 10.0.300 (net10.0 LTS) — used as specified.
- Git repo initialized at `D:\build-ledger-erp`.

## 1. Technology decisions
| Concern | Choice |
|---|---|
| UI | WPF, .NET 10, XAML, MVVM |
| MVVM toolkit | CommunityToolkit.Mvvm (source generators for ObservableObject/RelayCommand) |
| Validation | FluentValidation (Application layer) |
| Data | SQLite via EF Core (Microsoft.EntityFrameworkCore.Sqlite) |
| DI | Microsoft.Extensions.DependencyInjection + Generic Host |
| Logging | Microsoft.Extensions.Logging + rolling file provider (custom, no external dep needed) |
| Config | Microsoft.Extensions.Configuration (appsettings.json) |
| Password hashing | BCrypt.Net-Next |
| Testing | xUnit + EF Core InMemory/Sqlite for integration tests |
| Packaging | `dotnet publish` self-contained, win-x64, single-folder; optional Inno Setup script in `scripts/` |

## 2. Project layout
```
BuildLedger/
├── src/
│   ├── BuildLedger.Core/            # Entities, enums, domain rules, exceptions
│   ├── BuildLedger.Application/     # Services, DTOs, interfaces, validators
│   ├── BuildLedger.Infrastructure/  # EF Core DbContext, migrations, repositories, backup, security
│   ├── BuildLedger.App/             # WPF views, viewmodels, styles/design system, navigation
│   └── BuildLedger.Tests/           # xUnit tests
├── docs/
├── assets/
└── scripts/
```

## 3. Domain model (EF Core / SQLite)
Users, Roles, Permissions, Customers, Sites, Products, ProductCategories, Units, Suppliers,
Purchases, PurchaseItems, Sales, SaleItems, Payments, PaymentAllocations, StockMovements,
Expenses, ExpenseCategories, AuditLogs, AppSettings, BackupHistory.

Money fields use `decimal`. Ledger correctness (debit = sale increases receivable, credit =
payment/advance reduces it) is enforced in `LedgerService`, never inferred ad hoc in the UI.

## 4. Build phases
1. **Scaffolding** — solution, 5 projects, references, NuGet packages. (done)
2. **Design system** — colors, typography, spacing, control styles (light+dark) in
   `BuildLedger.App/Themes/`.
3. **Domain + persistence** — entities, `BuildLedgerDbContext`, EF configurations, initial
   migration, seed data (roles, default admin, base units/categories).
4. **Core services** — AuthService, CustomerService, ProductService, PurchaseService,
   SalesService, PaymentService/LedgerService, InventoryService, BackupService, ReportService.
5. **Shell + navigation** — MainWindow, sidebar nav, login flow, DI composition root in `App.xaml.cs`.
6. **Screens**, in priority order (each wired to real data, no placeholders left unfinished):
   Login → Dashboard → Customers/Sites → Products/Units → Suppliers → Purchases →
   Sales/Delivery → Payments/Ledger → Inventory → Reports → Expenses → Backup/Restore →
   Settings → User management → About.
7. **Tests** — unit tests for ledger math, stock math, validators; integration tests for
   transactional sale/purchase/payment flows against a real (temp-file) SQLite db.
8. **Packaging** — self-contained publish profile + Inno Setup script; smoke-test the built exe.
9. **Docs** — keep this plan, PROGRESS.md, DATABASE_SCHEMA.md, USER_GUIDE.md,
   BACKUP_AND_RESTORE.md, RELEASE_NOTES.md current as work lands.

## 5. Key design decisions locked in (no need to re-ask)
- Net accounting: Sales = Debit, Payments/Returns = Credit, running balance shown per
  customer and per site, plus a consolidated customer view.
- No profit figures anywhere until Expenses + COGS are wired — Dashboard/Reports show
  revenue/receivables only until the P&L report is implemented with real cost data.
- Negative stock blocked by default; an Admin-only setting can allow controlled overselling,
  logged in AuditLogs.
- Backups use SQLite's `VACUUM INTO` (transaction-safe copy) rather than raw file copy.
- Soft-delete (`IsActive`) for master data referenced by historical transactions; hard delete
  only for records never posted.
