# BuildLedger ERP — Database Schema

SQLite database, managed by EF Core migrations (`src/BuildLedger.Infrastructure/Persistence/Migrations`).
All monetary columns are `decimal(18,2)`; quantities are `decimal(18,3)`.

## Identity & security
- **Roles** (Id, Name, SystemRole) — Owner / Admin / Cashier
- **Permissions** (Id, Key, Description) — many-to-many with Roles via `RolePermissions`
- **Users** (Id, FullName, Username [unique], PasswordHash [BCrypt], RoleId, LastLoginAtUtc, IsActive)
- **AuditLogs** (Id, UserId?, Action, EntityName, EntityId?, Details?, OccurredAtUtc)
- **AppSettings** (Id, Key [unique], Value)

## Customers & sites
- **Customers** (Id, FullName, Phone, AlternatePhone?, Address?, CustomerType, Notes?, OpeningBalance, IsActive)
  - Indexed on Phone and FullName for fast smart-search.
- **Sites** (Id, CustomerId → Customers, SiteName, SiteAddress?, ProjectType?, ContactPerson?, Notes?, IsActive)
  - One customer → many sites.

## Catalog
- **Units** (Id, Name, Abbreviation)
- **ProductCategories** (Id, Name)
- **Products** (Id, Name, CategoryId → ProductCategories, UnitId → Units, PurchaseRate, SaleRate,
  MinimumStockLevel, CurrentStock, Description?, IsActive)
  - `CurrentStock` is a maintained projection; the source of truth is `StockMovements`.
- **Suppliers** (Id, Name, Phone?, Address?, Notes?, OpeningBalance, IsActive)

## Purchases (stock-in)
- **Purchases** (Id, SupplierId → Suppliers, InvoiceNumber, PurchaseDate, DiscountAmount, TaxAmount, Notes?, Status)
- **PurchaseItems** (Id, PurchaseId → Purchases [cascade], ProductId → Products, Quantity, UnitRate)

## Sales (stock-out) & delivery
- **Sales** (Id, CustomerId → Customers, SiteId? → Sites, InvoiceNumber, SaleDate, DiscountAmount, Notes?, Status)
- **SaleItems** (Id, SaleId → Sales [cascade], ProductId → Products, Quantity, UnitRate)

## Payments & ledger
- **Payments** (Id, CustomerId → Customers, SiteId? → Sites, ReceiptNumber, PaymentDate, Amount, Method,
  Purpose, Direction, ReferenceNumber?, Notes?)
- **PaymentAllocations** (Id, PaymentId → Payments [cascade], SaleId → Sales, AllocatedAmount)
  - Links a payment to the specific invoice(s) it settles; unallocated amount = advance/credit.
- **LedgerEntries** (Id, CustomerId → Customers, SiteId? → Sites, EntryDate, EntryType, ReferenceNumber,
  Amount, RunningBalance, Notes?)
  - Append-only. `Amount` positive = debit (customer owes more, e.g. a sale); negative = credit
    (payment or advance). `RunningBalance` is the customer's cumulative balance after this entry.
    This table is the single source of truth for every balance shown in the UI and reports.

## Inventory & operations
- **StockMovements** (Id, ProductId → Products, MovementType, QuantityChange, ResultingStock,
  MovementDate, ReferenceType?, ReferenceId?, Reason?)
  - Immutable audit trail. Every purchase, sale, return, and manual adjustment writes one row here;
    `Product.CurrentStock` is kept in sync transactionally whenever a row is added.
- **ExpenseCategories** (Id, Name)
- **Expenses** (Id, CategoryId → ExpenseCategories, ExpenseDate, Amount, Method, Payee?, Notes?, Status)
- **BackupHistories** (Id, BackupDateUtc, FilePath, FileSizeBytes, Status, ErrorMessage?, IsManual)

## Indexes
Phone, FullName (Customers); SiteName (Sites); Name (Products, Suppliers); InvoiceNumber + PurchaseDate
(Purchases); InvoiceNumber + SaleDate (Sales); ReceiptNumber + PaymentDate (Payments); EntryDate +
CustomerId (LedgerEntries); MovementDate + ProductId (StockMovements); Username (Users, unique);
Key (AppSettings, unique).

## Referential integrity rules
- Deletes on transactional tables (Purchases, Sales, Payments, LedgerEntries, StockMovements) are
  `Restrict` from their parent master records (Customer, Supplier, Product) — a master record with
  history cannot be hard-deleted, only deactivated (`IsActive = false`).
- Line items (PurchaseItems, SaleItems) cascade-delete with their parent invoice only, never with
  the referenced Product.
