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

### Status: CORE SYSTEM COMPLETE AND TESTED — see docs/RELEASE_NOTES.md for remaining known
limitations (role-based UI gating and change-password UI not yet wired — see below for the rest)

## 2026-09-13 (later same day) — UX polish + printing + auto-backup
User feedback after first visual review of the running app:
- Sidebar nav text was unreadable — a global implicit `TextBlock` style in Typography.xaml was
  force-setting dark `TextPrimary` foreground on every TextBlock, including the ones inside the
  dark-background sidebar buttons, overriding the button's own (correct) light foreground. Fixed by
  binding each nav item's TextBlock.Foreground to its parent Button's Foreground.
- Sidebar scrollbar felt jerky — the previous ScrollBar style only forced `Width="8"` onto the
  default OS template (never designed for that width), causing broken/glitchy rendering. Replaced
  with a proper minimal custom ScrollBar template (clean draggable thumb, no arrow buttons) for both
  orientations.
- MainWindow opened at its normal (non-maximized) size by default, which read as "the app looks
  tiny" — set `WindowState="Maximized"` as the default; the OS-standard restore/maximize toggle in
  the title bar was otherwise already working correctly.
- LoginWindow had `ResizeMode="NoResize"`, which hides the minimize/maximize title-bar buttons —
  changed to `ResizeMode="CanResize"` so both are present, and added `MinHeight`/`MinWidth`.
- Removed the "Default admin: admin / admin123" hint text below the sign-in button (credentials
  belong in docs, not the login screen).
- Implemented **invoice & report printing** (item 1 of the deferred list): a reusable
  `InvoicePrintService` (FlowDocument + PrintDialog) prints a formatted sale invoice from the Sales
  screen ("Print" per row) and any of the four Reports Center tabs ("Print This Report").
- Implemented the **automatic daily backup timer** (item 3 of the deferred list): a
  `DispatcherTimer` in `App.xaml.cs` checks once a minute (via a fresh DI scope) whether
  `AutoBackupEnabled` is on and the current time matches `AutoBackupTime`, and — if no automatic
  backup has run yet today — calls `IBackupService.CreateBackupAsync(isManual: false)`.
- Added UI motion per request: a fade + slight slide-in animation plays on every screen navigation
  (`MainWindow` content transition), a reusable rotating `LoadingSpinner` control, a global busy
  overlay bound to the active screen's `IsBusy`, and a spinner + "Signing in..." state on the login
  button while authenticating.
- Re-ran the full test suite after all changes: still 26/26 passing. Full solution still builds
  clean (0 warnings, 0 errors).

Deferred, not yet done (unchanged from before): role-based UI restrictions per Owner/Admin/Cashier,
and a change-password screen (the service method already exists).

## 2026-09-13 (third pass) — dialog clipping fixes, sidebar active-state clarity
Second round of screenshot feedback found several dialog windows were clipping their own content:
- **Root cause**: several dialogs placed their form fields directly in a `Grid.Row="*"` StackPanel
  with no `ScrollViewer`, and picked a fixed `Height` slightly smaller than the content actually
  needs — so anything below the last field (often the Save button, or the bottom border of the last
  TextBox) was cut off with no way to reach it. Confirmed on: Products & Units' "New Category"/"New
  Unit" dialog (`SimpleInputDialog`), the New Supplier dialog, and the Inventory "Adjust Stock"
  dialog.
- **Fix**: wrapped every affected dialog's field area in a `ScrollViewer` (`SiteDialog`,
  `SupplierDialog`, `StockAdjustmentDialog`, `UserDialog`, `ExpenseDialog`), switched
  `SimpleInputDialog` to `SizeToContent="Height"` so it can never clip regardless of which fields
  are shown, and made every dialog `ResizeMode="CanResize"` with sane `MinHeight`/`MinWidth` as a
  safety net for smaller screens or larger system font scaling — previously several were hard-locked
  to `NoResize` at a fixed size.
- **New Sale / New Purchase / Record Payment / Product / Customer dialogs**: reduced default
  `Height`/`Width` (Sale: 700x880 → 620x800, Purchase: 640x820 → 580x760, Payment: 680x520 → 620x540)
  so they comfortably fit on smaller displays instead of needing to be clamped by Windows, which was
  crowding the dialog's own title-bar buttons close to the main window's maximized title-bar buttons
  and making them hard to tell apart.
- **Sidebar active-item clarity**: added a solid left accent bar (amber, matches the design system's
  existing accent color) plus bold text on whichever nav item is currently selected, on top of the
  existing background highlight — so the active screen is unambiguous even if the highlight color
  alone was hard to distinguish at a glance.
- **Login button loading feedback**: the sign-in spinner could complete and disappear in well under
  100ms against a local SQLite database, so it was easy to miss entirely. Added a 400ms minimum
  visible duration for the busy state so the spinner is always perceptible.
- **Visual polish**: added a soft drop shadow to all Card-style panels (dashboard KPI tiles, list
  panels, forms) for a bit more depth/premium feel.
- Rebuilt (0 warnings/errors) and re-ran the full test suite (26/26 passing) after all changes.

Note on verification: attempted to drive the running app end-to-end via simulated mouse/keyboard
in this sandboxed environment to capture before/after screenshots, but the automation targeted the
wrong window and typed into this chat instead (visible as a stray "admin" message) — that attempt
was abandoned immediately. These fixes are verified by code review and a clean rebuild/test run, but
not by an in-app screenshot in this session; please confirm visually on your own machine.
