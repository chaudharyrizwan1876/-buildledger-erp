# Release Notes

## v1.0.0 — Initial release

### What's included
- Full MVVM WPF desktop app (.NET 10) with a custom light/dark design system.
- SQLite database via EF Core, auto-migrated and seeded on first run.
- Modules: Login, Dashboard, Customers & Sites (with smart duplicate-preventing search),
  Products/Units/Categories, Suppliers, Purchases, Sales & Delivery, Payments & Receipts
  (advance/partial/full/against-invoice/against-balance), customer ledger (per-site and
  consolidated), Inventory & stock movements with audited adjustments, Expenses, Reports Center
  (Sales, Payments, Receivables, Supplier Balances with date-range filters), Backup & Restore
  (`VACUUM INTO`-based, transaction-consistent), User Management (Owner/Admin/Cashier roles),
  Settings, About.
- 26 passing xUnit tests covering customer duplicate detection, multi-site customers, purchase
  stock increases, sale stock decreases and insufficient-stock rejection, negative-stock override,
  full/partial payment allocation, advance payments, payment-against-balance, stock adjustments
  with audit reason, report date filtering, backup creation, and authentication.

### Building the release
```powershell
./scripts/publish.ps1
```
Produces a self-contained `win-x64` build in `publish/` — verified to launch, initialize its
database, and seed reference data with zero errors.

### Building the installer
Requires [Inno Setup](https://jrsoftware.org/isinfo.php) installed separately (`ISCC.exe`):
```powershell
./scripts/publish.ps1
ISCC.exe scripts/installer.iss
```
Produces `dist/BuildLedgerERP-Setup-1.0.0.exe`.

### Known limitations
- **No invoice/report PDF export or print preview yet.** Sales/purchase invoices and reports are
  viewable and filterable on-screen but cannot yet be printed or exported to PDF — this is the
  single biggest gap against the full spec and is the recommended next increment.
- **Permissions are role-gated conceptually (Owner/Admin/Cashier) but the UI does not yet hide or
  disable actions per role** — every signed-in user currently sees every screen. Enforcing this in
  the ViewModels/Views is a follow-up.
- **Password change UI** exists in the service layer (`IAuthService.ChangePasswordAsync`) but is
  not yet wired to a Settings screen control.
- **Profit & Loss report** is intentionally not implemented — it requires complete, reliable cost
  and expense data first, per the "never show a fake profit figure" requirement. Sales, Payments,
  Receivables, and Supplier Balances reports are implemented and working.
- Automatic daily backup is configurable in Settings but the scheduled trigger (a background timer
  that fires at the configured time while the app is open) is not yet wired up — manual and
  "Backup To Folder" both work today.

### Recommended next steps
1. Wire up FlowDocument-based printing/PDF export for sale/purchase invoices and the four reports.
2. Enforce role-based UI restrictions (hide Users/Settings/Backup for Cashier role, etc.).
3. Wire the auto-backup timer and the change-password dialog.
4. Add returns workflows (sale return / purchase return) with their own stock-movement types
   (already modeled in the schema, not yet exposed in the UI).
