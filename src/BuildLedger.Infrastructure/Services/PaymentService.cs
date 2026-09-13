using BuildLedger.Application.Common;
using BuildLedger.Application.DTOs;
using BuildLedger.Application.Interfaces;
using BuildLedger.Core.Entities;
using BuildLedger.Core.Enums;
using BuildLedger.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BuildLedger.Infrastructure.Services;

public class PaymentService : IPaymentService
{
    private readonly BuildLedgerDbContext _db;
    private readonly IAuditService _audit;
    private readonly ICurrentUserContext _currentUser;

    public PaymentService(BuildLedgerDbContext db, IAuditService audit, ICurrentUserContext currentUser)
    {
        _db = db;
        _audit = audit;
        _currentUser = currentUser;
    }

    public async Task<ServiceResult<int>> RecordPaymentAsync(RecordPaymentDto dto, CancellationToken ct = default)
    {
        if (dto.Amount <= 0) return ServiceResult<int>.Fail("Payment amount must be greater than zero.");
        if (string.IsNullOrWhiteSpace(dto.ReceiptNumber)) return ServiceResult<int>.Fail("Receipt number is required.");

        if (dto.Purpose == PaymentPurpose.AgainstInvoice)
        {
            var allocatedTotal = dto.Allocations.Sum(a => a.Amount);
            if (allocatedTotal > dto.Amount)
                return ServiceResult<int>.Fail("Allocated amount cannot exceed the payment amount.");
        }

        await using var tx = await _db.Database.BeginTransactionAsync(ct);
        try
        {
            var payment = new Payment
            {
                CustomerId = dto.CustomerId,
                SiteId = dto.SiteId,
                ReceiptNumber = dto.ReceiptNumber.Trim(),
                PaymentDate = dto.PaymentDate,
                Amount = dto.Amount,
                Method = dto.Method,
                Purpose = dto.Purpose,
                Direction = PaymentDirection.Received,
                ReferenceNumber = dto.ReferenceNumber,
                Notes = dto.Notes
            };
            _db.Payments.Add(payment);
            await _db.SaveChangesAsync(ct);

            if (dto.Purpose == PaymentPurpose.AgainstInvoice)
            {
                foreach (var (saleId, amount) in dto.Allocations)
                {
                    if (amount <= 0) continue;
                    _db.PaymentAllocations.Add(new PaymentAllocation
                    {
                        PaymentId = payment.Id,
                        SaleId = saleId,
                        AllocatedAmount = amount
                    });
                }
                await _db.SaveChangesAsync(ct);
            }

            var previousBalance = await _db.LedgerEntries.Where(l => l.CustomerId == dto.CustomerId)
                .SumAsync(l => (decimal?)l.Amount, ct) ?? 0m;
            var newBalance = previousBalance - dto.Amount;

            _db.LedgerEntries.Add(new LedgerEntry
            {
                CustomerId = dto.CustomerId,
                SiteId = dto.SiteId,
                EntryDate = dto.PaymentDate,
                EntryType = LedgerEntryType.Payment,
                ReferenceNumber = payment.ReceiptNumber,
                Amount = -dto.Amount,
                RunningBalance = newBalance,
                Notes = dto.Purpose switch
                {
                    PaymentPurpose.Advance => "Advance payment",
                    PaymentPurpose.AgainstInvoice => "Payment against invoice(s)",
                    _ => "Payment against balance"
                }
            });

            await _db.SaveChangesAsync(ct);
            await _audit.LogAsync(_currentUser.UserId, "Create", "Payment", payment.Id.ToString(), null, ct);
            await tx.CommitAsync(ct);

            return ServiceResult<int>.Ok(payment.Id);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync(ct);
            return ServiceResult<int>.Fail($"Failed to record payment: {ex.Message}");
        }
    }

    public async Task<IReadOnlyList<PaymentSummaryDto>> SearchAsync(DateRange? range, int? customerId, CancellationToken ct = default)
    {
        var q = _db.Payments.AsNoTracking().Include(p => p.Customer).AsQueryable();
        if (range is not null) q = q.Where(p => p.PaymentDate >= range.From && p.PaymentDate <= range.To);
        if (customerId is int cid) q = q.Where(p => p.CustomerId == cid);

        var payments = await q.OrderByDescending(p => p.PaymentDate).ToListAsync(ct);
        return payments.Select(p => new PaymentSummaryDto(p.Id, p.ReceiptNumber, p.PaymentDate,
            p.Customer?.FullName ?? string.Empty, p.Amount, p.Method, p.Purpose)).ToList();
    }

    public async Task<string> GenerateNextReceiptNumberAsync(CancellationToken ct = default)
    {
        var count = await _db.Payments.CountAsync(ct);
        return $"RCT-{DateTime.Now:yyyyMM}-{count + 1:0000}";
    }
}
