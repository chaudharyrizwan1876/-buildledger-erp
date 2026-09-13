using BuildLedger.Application.Common;
using BuildLedger.Application.DTOs;
using BuildLedger.Application.Interfaces;
using BuildLedger.Core.Entities;
using BuildLedger.Core.Enums;
using BuildLedger.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace BuildLedger.Infrastructure.Backup;

/// <summary>
/// Uses SQLite's `VACUUM INTO` to create a transaction-consistent backup copy without
/// locking out concurrent readers/writers or risking a torn copy of the live db file.
/// </summary>
public class BackupService : IBackupService
{
    private readonly BuildLedgerDbContext _db;
    private readonly string _defaultBackupFolder;

    public BackupService(BuildLedgerDbContext db, string defaultBackupFolder)
    {
        _db = db;
        _defaultBackupFolder = defaultBackupFolder;
    }

    public async Task<ServiceResult<string>> CreateBackupAsync(string? destinationFolder = null, bool isManual = true, CancellationToken ct = default)
    {
        var folder = string.IsNullOrWhiteSpace(destinationFolder) ? _defaultBackupFolder : destinationFolder;
        Directory.CreateDirectory(folder);
        var fileName = $"buildledger-backup-{DateTime.Now:yyyyMMdd-HHmmss}.db";
        var fullPath = Path.Combine(folder, fileName);

        try
        {
            var connection = (SqliteConnection)_db.Database.GetDbConnection();
            var wasClosed = connection.State != System.Data.ConnectionState.Open;
            if (wasClosed) await connection.OpenAsync(ct);

            await using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "VACUUM INTO $path";
                cmd.Parameters.AddWithValue("$path", fullPath);
                await cmd.ExecuteNonQueryAsync(ct);
            }

            if (wasClosed) await connection.CloseAsync();

            var fileInfo = new FileInfo(fullPath);
            _db.BackupHistories.Add(new BackupHistory
            {
                BackupDateUtc = DateTime.UtcNow,
                FilePath = fullPath,
                FileSizeBytes = fileInfo.Length,
                Status = BackupStatus.Success,
                IsManual = isManual
            });
            await _db.SaveChangesAsync(ct);

            return ServiceResult<string>.Ok(fullPath);
        }
        catch (Exception ex)
        {
            _db.BackupHistories.Add(new BackupHistory
            {
                BackupDateUtc = DateTime.UtcNow,
                FilePath = fullPath,
                Status = BackupStatus.Failed,
                ErrorMessage = ex.Message,
                IsManual = isManual
            });
            await _db.SaveChangesAsync(ct);
            return ServiceResult<string>.Fail($"Backup failed: {ex.Message}");
        }
    }

    public async Task<ServiceResult> RestoreBackupAsync(string backupFilePath, CancellationToken ct = default)
    {
        if (!File.Exists(backupFilePath)) return ServiceResult.Fail("Backup file not found.");

        try
        {
            var connection = (SqliteConnection)_db.Database.GetDbConnection();
            var dataSource = new SqliteConnectionStringBuilder(connection.ConnectionString).DataSource;

            // Safety backup of current db before overwriting it.
            var safetyPath = Path.Combine(_defaultBackupFolder, $"pre-restore-safety-{DateTime.Now:yyyyMMdd-HHmmss}.db");
            Directory.CreateDirectory(_defaultBackupFolder);
            await CreateBackupAsync(_defaultBackupFolder, isManual: false, ct);

            await _db.Database.CloseConnectionAsync();
            File.Copy(backupFilePath, dataSource, overwrite: true);

            return ServiceResult.Ok();
        }
        catch (Exception ex)
        {
            return ServiceResult.Fail($"Restore failed: {ex.Message}");
        }
    }

    public async Task<IReadOnlyList<BackupHistoryDto>> GetHistoryAsync(CancellationToken ct = default)
    {
        var history = await _db.BackupHistories.AsNoTracking().OrderByDescending(b => b.BackupDateUtc).ToListAsync(ct);
        return history.Select(b => new BackupHistoryDto(b.Id, b.BackupDateUtc, b.FilePath, b.FileSizeBytes, b.Status, b.IsManual)).ToList();
    }
}
