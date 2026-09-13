using BuildLedger.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace BuildLedger.Tests;

/// <summary>
/// Creates an isolated in-memory SQLite database (via a single kept-open connection) per test,
/// with the full EF Core schema applied through EnsureCreated so tests exercise real SQL, not a fake provider.
/// </summary>
public sealed class TestDb : IDisposable
{
    private readonly SqliteConnection _connection;
    public BuildLedgerDbContext Context { get; }

    public TestDb()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<BuildLedgerDbContext>()
            .UseSqlite(_connection)
            .Options;

        Context = new BuildLedgerDbContext(options);
        Context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        Context.Dispose();
        _connection.Dispose();
    }
}
