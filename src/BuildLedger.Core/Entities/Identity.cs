using BuildLedger.Core.Enums;

namespace BuildLedger.Core.Entities;

public class Role : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public SystemRole SystemRole { get; set; }
    public List<Permission> Permissions { get; set; } = new();
    public List<User> Users { get; set; } = new();
}

public class Permission : AuditableEntity
{
    public string Key { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<Role> Roles { get; set; } = new();
}

public class User : AuditableEntity
{
    public string FullName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public Role? Role { get; set; }
    public DateTime? LastLoginAtUtc { get; set; }
}

public class AuditLog : AuditableEntity
{
    public int? UserId { get; set; }
    public User? User { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public string? Details { get; set; }
    public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;
}

public class AppSetting : AuditableEntity
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
