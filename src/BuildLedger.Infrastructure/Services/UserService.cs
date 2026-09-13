using BuildLedger.Application.Common;
using BuildLedger.Application.DTOs;
using BuildLedger.Application.Interfaces;
using BuildLedger.Core.Entities;
using BuildLedger.Infrastructure.Persistence;
using BuildLedger.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;

namespace BuildLedger.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly BuildLedgerDbContext _db;
    private readonly IAuditService _audit;
    private readonly ICurrentUserContext _currentUser;

    public UserService(BuildLedgerDbContext db, IAuditService audit, ICurrentUserContext currentUser)
    {
        _db = db;
        _audit = audit;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<UserDto>> GetAllAsync(CancellationToken ct = default)
    {
        var users = await _db.Users.AsNoTracking().Include(u => u.Role).OrderBy(u => u.FullName).ToListAsync(ct);
        return users.Select(u => new UserDto(u.Id, u.FullName, u.Username, u.Role?.Name ?? string.Empty, u.IsActive, u.LastLoginAtUtc)).ToList();
    }

    public async Task<ServiceResult<int>> UpsertAsync(UserUpsertDto dto, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.FullName)) return ServiceResult<int>.Fail("Full name is required.");
        if (string.IsNullOrWhiteSpace(dto.Username)) return ServiceResult<int>.Fail("Username is required.");

        var duplicate = await _db.Users.AnyAsync(u => u.Username == dto.Username && u.Id != (dto.Id ?? 0), ct);
        if (duplicate) return ServiceResult<int>.Fail("This username is already taken.");

        User entity;
        if (dto.Id is int id)
        {
            entity = await _db.Users.FindAsync([id], ct) ?? throw new InvalidOperationException("User not found.");
        }
        else
        {
            if (string.IsNullOrWhiteSpace(dto.Password) || dto.Password.Length < 6)
                return ServiceResult<int>.Fail("Password must be at least 6 characters.");
            entity = new User { PasswordHash = PasswordHasher.Hash(dto.Password) };
            _db.Users.Add(entity);
        }

        entity.FullName = dto.FullName.Trim();
        entity.Username = dto.Username.Trim();
        entity.RoleId = dto.RoleId;
        if (!string.IsNullOrWhiteSpace(dto.Password) && dto.Id is not null)
            entity.PasswordHash = PasswordHasher.Hash(dto.Password);

        await _db.SaveChangesAsync(ct);
        await _audit.LogAsync(_currentUser.UserId, dto.Id is null ? "Create" : "Update", "User", entity.Id.ToString(), null, ct);
        return ServiceResult<int>.Ok(entity.Id);
    }

    public async Task<ServiceResult> SetActiveAsync(int id, bool isActive, CancellationToken ct = default)
    {
        var u = await _db.Users.FindAsync([id], ct);
        if (u is null) return ServiceResult.Fail("User not found.");
        u.IsActive = isActive;
        await _db.SaveChangesAsync(ct);
        return ServiceResult.Ok();
    }

    public async Task<IReadOnlyList<(int Id, string Name)>> GetRolesAsync(CancellationToken ct = default)
    {
        var roles = await _db.Roles.AsNoTracking().OrderBy(r => r.Name).ToListAsync(ct);
        return roles.Select(r => (r.Id, r.Name)).ToList();
    }
}
