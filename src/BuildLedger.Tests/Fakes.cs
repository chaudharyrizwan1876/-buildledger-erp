using BuildLedger.Application.Interfaces;
using BuildLedger.Core.Enums;

namespace BuildLedger.Tests;

public class FakeCurrentUserContext : ICurrentUserContext
{
    public int? UserId { get; private set; }
    public string? FullName { get; private set; } = "Test User";
    public SystemRole? Role { get; private set; } = SystemRole.Owner;

    public void SignIn(int userId, string fullName, SystemRole role)
    {
        UserId = userId; FullName = fullName; Role = role;
    }

    public void SignOut()
    {
        UserId = null; FullName = null; Role = null;
    }
}
