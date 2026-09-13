using BuildLedger.Infrastructure.Backup;

namespace BuildLedger.Tests;

public class BackupServiceTests
{
    [Fact]
    public async Task CreateBackup_ProducesFile_AndHistoryRecord()
    {
        using var db = new TestDb();
        await SeedHelper.SeedCustomerAsync(db.Context);

        var folder = Path.Combine(Path.GetTempPath(), "BuildLedgerTests_" + Guid.NewGuid());
        var service = new BackupService(db.Context, folder);

        var result = await service.CreateBackupAsync(isManual: true);

        Assert.True(result.Success);
        Assert.True(File.Exists(result.Data));

        var history = await service.GetHistoryAsync();
        Assert.Single(history);
        Assert.Equal(BuildLedger.Core.Enums.BackupStatus.Success, history[0].Status);

        Directory.Delete(folder, recursive: true);
    }
}
