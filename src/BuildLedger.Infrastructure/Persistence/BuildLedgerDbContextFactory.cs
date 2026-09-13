using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BuildLedger.Infrastructure.Persistence;

/// <summary>Used only by `dotnet ef` tooling at design time to generate migrations.</summary>
public class BuildLedgerDbContextFactory : IDesignTimeDbContextFactory<BuildLedgerDbContext>
{
    public BuildLedgerDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<BuildLedgerDbContext>();
        optionsBuilder.UseSqlite("Data Source=buildledger.design.db");
        return new BuildLedgerDbContext(optionsBuilder.Options);
    }
}
