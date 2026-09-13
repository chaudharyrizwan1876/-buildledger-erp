namespace BuildLedger.Core.Enums;

public enum CustomerType
{
    Individual,
    Contractor,
    ConstructionCompany,
    SiteCustomer
}

public enum PaymentMethod
{
    Cash,
    BankTransfer,
    Cheque,
    Other
}

public enum PaymentPurpose
{
    Advance,
    AgainstInvoice,
    AgainstBalance
}

public enum PaymentDirection
{
    Received,
    Paid
}

public enum StockMovementType
{
    Purchase,
    Sale,
    PurchaseReturn,
    SaleReturn,
    AdjustmentIncrease,
    AdjustmentDecrease,
    OpeningStock
}

public enum LedgerEntryType
{
    OpeningBalance,
    Sale,
    Payment,
    SaleReturn,
    Adjustment
}

public enum InvoiceStatus
{
    Draft,
    Posted,
    PartiallyPaid,
    Paid,
    Voided
}

public enum ExpenseStatus
{
    Recorded,
    Voided
}

public enum BackupStatus
{
    Success,
    Failed
}

public enum SystemRole
{
    Owner,
    Admin,
    Cashier
}
