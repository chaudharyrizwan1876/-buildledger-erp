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

### Invoice/report printing & automatic backup
- Sale invoices can be printed from the Sales screen ("Print" per row), and each Reports Center tab
  can be printed via "Print This Report" — both use a FlowDocument + the standard Windows print
  dialog (`InvoicePrintService`). PDF export specifically (as opposed to "print to a PDF printer",
  which the OS print dialog already supports) is not separately implemented.
- Automatic daily backup now actually runs: a background check (once a minute) fires
  `IBackupService.CreateBackupAsync` when the current time reaches the configured `AutoBackupTime`
  and no automatic backup has run yet that day, as long as "Enable automatic daily backup" is on in
  Settings.

### Known limitations
- **Permissions are role-gated conceptually (Owner/Admin/Cashier) but the UI does not yet hide or
  disable actions per role** — every signed-in user currently sees every screen. Enforcing this in
  the ViewModels/Views is a follow-up.
- **Password change UI** exists in the service layer (`IAuthService.ChangePasswordAsync`) but is
  not yet wired to a Settings screen control.
- **Profit & Loss report** is intentionally not implemented — it requires complete, reliable cost
  and expense data first, per the "never show a fake profit figure" requirement. Sales, Payments,
  Receivables, and Supplier Balances reports are implemented and working.
- Purchase invoices are not yet printable (only Sales invoices and the four reports) — the same
  `InvoicePrintService` pattern can be extended to `PurchasesView` in the same way.

### Recommended next steps
1. Enforce role-based UI restrictions (hide Users/Settings/Backup for Cashier role, etc.).
2. Wire the change-password dialog into Settings.
3. Extend printing to purchase invoices.
4. Add returns workflows (sale return / purchase return) with their own stock-movement types
   (already modeled in the schema, not yet exposed in the UI).
