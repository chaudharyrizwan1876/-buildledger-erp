# BuildLedger ERP — User Guide

## Signing in
Launch the app and sign in. The first-run administrator account is `admin` / `admin123` — change
this password immediately (ask your Owner-role user, or use Settings once password-change UI is
wired to your account). Create individual accounts for each staff member instead of sharing one
login, so the audit trail reflects who did what.

## Daily workflow

### 1. Customers & Sites
Use **Customers** to search by name, phone, ID, or site name before creating a new record — this
prevents duplicate customer entries. Selecting a customer shows their consolidated ledger and a
list of their construction sites; use "+ Site" to add another site under the same customer (e.g. a
contractor running three projects at once).

### 2. Products & Units
**Products & Units** is your material catalog. Add categories (Steel, Cement, Aggregates, …) and
units (bag, kg, ton, truckload, …) as needed — the system does not hard-code specific products.
Set a Minimum Stock Level per product so the dashboard and reports can flag low stock.

### 3. Purchases (stock-in)
Record a purchase from **Purchases** when material arrives from a supplier. Every line item
increases that product's stock immediately and is logged in the stock movement history.

### 4. Sales & Delivery (stock-out)
Create a sale from **Sales & Delivery**: search for the customer, optionally pick their site, add
line items, and — if the customer pays on the spot — enter the amount collected and its method.
The system blocks selling more than is in stock unless an Owner/Admin has enabled "Allow negative
stock" in Settings.

### 5. Payments & Receipts
Use **Payments & Receipts** to record money coming in at any time, independent of a sale:
- **Advance**: customer pays before any invoice exists — reduces their balance (shows as a credit).
- **Against Invoice**: pick the specific unpaid sale this payment settles.
- **Against Balance**: reduces the customer's overall balance without tying to one invoice.

### 6. Customer Ledger
Every customer's ledger (visible on the Customers screen) shows a running balance: sales increase
it (debit — the customer owes more), payments decrease it (credit). A negative balance means the
customer has paid in advance. Sites can be viewed individually or the customer's balance can be
viewed consolidated across all their sites.

### 7. Inventory
**Inventory** shows the full stock movement history (purchases, sales, adjustments) and lets an
authorized user correct stock counts via "Adjust Stock" — a reason is always required and is kept
in the audit trail.

### 8. Expenses
Record day-to-day operating costs (rent, utilities, salaries, transport) in **Expenses**, grouped
by category, for use in the Expenses Report and (once cost data is complete) profit reporting.

### 9. Reports Center
Filter by date range (with quick buttons for Today/This Week/This Month/Last Month) across Sales,
Payments, Receivables, and Supplier Balances. Note: the dashboard and reports never label
sales-minus-payments as "profit" — real profit requires cost and expense data, which the Reports
Center keeps separate until that data is complete for a given period.

### 10. Backup & Restore
See [BACKUP_AND_RESTORE.md](BACKUP_AND_RESTORE.md).

### 11. User Management
Owner/Admin can create staff accounts and assign them a role (Owner, Admin, Cashier). Deactivate
an account instead of deleting it to preserve historical audit records.

### 12. Settings
Business name, dark/light theme, negative-stock override, and automatic backup schedule.
