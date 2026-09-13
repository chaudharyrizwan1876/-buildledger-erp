using BuildLedger.Core.Enums;

namespace BuildLedger.Application.DTOs;

public record CustomerSearchResultDto(
    int Id, string FullName, string Phone, string? AlternatePhone, string? Address,
    CustomerType CustomerType, decimal CurrentBalance, bool IsActive);

public class CustomerUpsertDto
{
    public int? Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? AlternatePhone { get; set; }
    public string? Address { get; set; }
    public CustomerType CustomerType { get; set; }
    public string? Notes { get; set; }
    public decimal OpeningBalance { get; set; }
}

public record CustomerDetailDto(
    int Id, string FullName, string Phone, string? AlternatePhone, string? Address,
    CustomerType CustomerType, string? Notes, decimal OpeningBalance, decimal CurrentBalance,
    bool IsActive, DateTime CreatedAtUtc, IReadOnlyList<SiteDto> Sites);

public class SiteUpsertDto
{
    public int? Id { get; set; }
    public int CustomerId { get; set; }
    public string SiteName { get; set; } = string.Empty;
    public string? SiteAddress { get; set; }
    public string? ProjectType { get; set; }
    public string? ContactPerson { get; set; }
    public string? Notes { get; set; }
}

public record SiteDto(
    int Id, int CustomerId, string SiteName, string? SiteAddress, string? ProjectType,
    string? ContactPerson, decimal CurrentBalance, bool IsActive);

public record LedgerRowDto(
    int Id, DateTime EntryDate, LedgerEntryType EntryType, string ReferenceNumber,
    decimal Amount, decimal RunningBalance, string? Notes);
