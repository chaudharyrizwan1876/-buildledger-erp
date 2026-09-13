using BuildLedger.Application.Interfaces;
using BuildLedger.Core.Enums;

namespace BuildLedger.Infrastructure.Security;

/// <summary>Singleton holding the signed-in user for the lifetime of the app session.</summary>
public class CurrentUserContext : ICurrentUserContext
{
    public int? UserId { get; private set; }
    public string? FullName { get; private set; }
    public SystemRole? Role { get; private set; }

    public void SignIn(int userId, string fullName, SystemRole role)
    {
        UserId = userId;
        FullName = fullName;
        Role = role;
    }

    public void SignOut()
    {
        UserId = null;
        FullName = null;
        Role = null;
    }
}
