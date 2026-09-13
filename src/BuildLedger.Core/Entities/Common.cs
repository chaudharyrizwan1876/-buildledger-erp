namespace BuildLedger.Core.Entities;

public abstract class AuditableEntity
{
    public int Id { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
    public bool IsActive { get; set; } = true;
}
