using BuildLedger.Core.Enums;

namespace BuildLedger.Core.Entities;

public class Customer : AuditableEntity
{
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? AlternatePhone { get; set; }
    public string? Address { get; set; }
    public CustomerType CustomerType { get; set; }
    public string? Notes { get; set; }

    public List<Site> Sites { get; set; } = new();
    public List<Sale> Sales { get; set; } = new();
    public List<Payment> Payments { get; set; } = new();
    public List<LedgerEntry> LedgerEntries { get; set; } = new();

    /// <summary>Opening balance when the customer was onboarded (debit-positive).</summary>
    public decimal OpeningBalance { get; set; }
}

public class Site : AuditableEntity
{
    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public string SiteName { get; set; } = string.Empty;
    public string? SiteAddress { get; set; }
    public string? ProjectType { get; set; }
    public string? ContactPerson { get; set; }
    public string? Notes { get; set; }

    public List<Sale> Sales { get; set; } = new();
    public List<LedgerEntry> LedgerEntries { get; set; } = new();
}
