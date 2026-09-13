using BuildLedger.Application.Common;
using BuildLedger.Application.DTOs;
using BuildLedger.Application.Interfaces;
using BuildLedger.Infrastructure.Persistence;
using BuildLedger.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;

namespace BuildLedger.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly BuildLedgerDbContext _db;
    private readonly ICurrentUserContext _currentUser;
    private readonly IAuditService _audit;

    public AuthService(BuildLedgerDbContext db, ICurrentUserContext currentUser, IAuditService audit)
    {
        _db = db;
        _currentUser = currentUser;
        _audit = audit;
    }

    public async Task<LoginResultDto> LoginAsync(string username, string password, CancellationToken ct = default)
    {
        var user = await _db.Users.Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Username == username && u.IsActive, ct);

        if (user is null || user.Role is null || !PasswordHasher.Verify(password, user.PasswordHash))
        {
            await _audit.LogAsync(null, "LoginFailed", "User", username, "Invalid credentials", ct);
            return new LoginResultDto(false, "Invalid username or password.", 0, string.Empty, default);
        }

        user.LastLoginAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        _currentUser.SignIn(user.Id, user.FullName, user.Role.SystemRole);
        await _audit.LogAsync(user.Id, "Login", "User", user.Id.ToString(), null, ct);

        return new LoginResultDto(true, null, user.Id, user.FullName, user.Role.SystemRole);
    }

    public async Task<ServiceResult> ChangePasswordAsync(int userId, string currentPassword, string newPassword, CancellationToken ct = default)
    {
        var user = await _db.Users.FindAsync([userId], ct);
        if (user is null) return ServiceResult.Fail("User not found.");
        if (!PasswordHasher.Verify(currentPassword, user.PasswordHash))
            return ServiceResult.Fail("Current password is incorrect.");
        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
            return ServiceResult.Fail("New password must be at least 6 characters.");

        user.PasswordHash = PasswordHasher.Hash(newPassword);
        await _db.SaveChangesAsync(ct);
        await _audit.LogAsync(userId, "ChangePassword", "User", userId.ToString(), null, ct);
        return ServiceResult.Ok();
    }
}
