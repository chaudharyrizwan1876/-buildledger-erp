using BuildLedger.Core.Entities;
using BuildLedger.Core.Enums;
using BuildLedger.Infrastructure.Security;
using BuildLedger.Infrastructure.Services;

namespace BuildLedger.Tests;

public class AuthServiceTests
{
    private static async Task<int> SeedUserAsync(TestDb db, string username, string password, SystemRole role)
    {
        var roleEntity = new Role { Name = role.ToString(), SystemRole = role };
        db.Context.Roles.Add(roleEntity);
        await db.Context.SaveChangesAsync();

        var user = new User { FullName = "Test", Username = username, PasswordHash = PasswordHasher.Hash(password), RoleId = roleEntity.Id };
        db.Context.Users.Add(user);
        await db.Context.SaveChangesAsync();
        return user.Id;
    }

    [Fact]
    public async Task Login_WithCorrectCredentials_Succeeds()
    {
        using var db = new TestDb();
        await SeedUserAsync(db, "admin", "admin123", SystemRole.Owner);
        var currentUser = new FakeCurrentUserContext();
        var service = new AuthService(db.Context, currentUser, new AuditService(db.Context));

        var result = await service.LoginAsync("admin", "admin123");

        Assert.True(result.Success);
        Assert.Equal(SystemRole.Owner, result.Role);
    }

    [Fact]
    public async Task Login_WithWrongPassword_Fails()
    {
        using var db = new TestDb();
        await SeedUserAsync(db, "admin", "admin123", SystemRole.Owner);
        var service = new AuthService(db.Context, new FakeCurrentUserContext(), new AuditService(db.Context));

        var result = await service.LoginAsync("admin", "wrongpassword");

        Assert.False(result.Success);
    }

    [Fact]
    public async Task Login_WithUnknownUsername_Fails()
    {
        using var db = new TestDb();
        var service = new AuthService(db.Context, new FakeCurrentUserContext(), new AuditService(db.Context));

        var result = await service.LoginAsync("ghost", "whatever");

        Assert.False(result.Success);
    }
}
