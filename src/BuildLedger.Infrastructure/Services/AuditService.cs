using BuildLedger.Application.Interfaces;
using BuildLedger.Core.Entities;
using BuildLedger.Infrastructure.Persistence;

namespace BuildLedger.Infrastructure.Services;

public class AuditService : IAuditService
{
    private readonly BuildLedgerDbContext _db;
    public AuditService(BuildLedgerDbContext db) => _db = db;

    public async Task LogAsync(int? userId, string action, string entityName, string? entityId, string? details, CancellationToken ct = default)
    {
        _db.AuditLogs.Add(new AuditLog
        {
            UserId = userId,
            Action = action,
            EntityName = entityName,
            EntityId = entityId,
            Details = details,
            OccurredAtUtc = DateTime.UtcNow
        });
        await _db.SaveChangesAsync(ct);
    }
}
