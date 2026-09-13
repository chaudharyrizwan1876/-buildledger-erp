# BuildLedger ERP — Progress Log

## 2026-09-13
- Inspected target folder (empty), confirmed .NET 10.0.300 SDK and git available.
- Initialized git repo, created solution (`BuildLedger.slnx`), scaffolded 5 projects: Core,
  Application, Infrastructure, App (WPF), Tests (xUnit); wired references and NuGet packages.
- Built the full domain model (20 entities), EF Core configurations, and the initial migration —
  verified against a real SQLite database.
- Built the Application-layer service interfaces + DTOs, then implemented every service in
  Infrastructure: Auth, Customer (with duplicate-prevention search + ledger), Product/Unit/Category,
  Supplier, Purchase (transactional stock-in), Sales (transactional stock-out with insufficient-stock
  guard + optional point-of-sale payment), Payment (advance/partial/full/against-invoice/against-
  balance with ledger posting), Inventory (audited stock adjustments), Expense, Report (dashboard +
  4 reports), Backup (SQLite `VACUUM INTO`), User management, Settings, Audit logging.
- Built the full design system (light + dark themes, typography scale, button/input/grid/tab
  styles) and the complete WPF UI: Login, shell/navigation, Dashboard, Customers (search + ledger +
  sites), Products/Units/Categories, Suppliers, Purchases, Sales & Delivery, Payments & Receipts,
  Inventory, Expenses, Reports Center, Backup & Restore, User Management, Settings, About — all
  wired to live data through DI (Generic Host + Microsoft.Extensions.DependencyInjection).
- Fixed a real transaction-safety bug found by tests: audit logging was happening after
  `CommitAsync`, so if it failed the code tried to roll back an already-committed transaction,
  masking a successful save as a failure. Moved audit logging inside the atomic transaction in
  SalesService, PurchaseService, and PaymentService.
- Wrote 26 xUnit tests (customers, products, purchases, sales, payments, inventory, backup, auth,
  reports) — all passing against a real in-memory SQLite database.
- Verified: solution builds clean (0 warnings, 0 errors), app launches and initializes/seeds its
  database with no exceptions, self-contained Release publish (`win-x64`) builds and launches
  cleanly.
- Wrote README, DATABASE_SCHEMA, USER_GUIDE, BACKUP_AND_RESTORE, RELEASE_NOTES, publish script,
  and an Inno Setup installer script.

### Status: CORE SYSTEM COMPLETE AND TESTED — see docs/RELEASE_NOTES.md for known limitations
(no invoice/report PDF/print yet, role-based UI gating not yet enforced, auto-backup timer and
change-password UI not yet wired — everything else in the original spec is implemented and working)
