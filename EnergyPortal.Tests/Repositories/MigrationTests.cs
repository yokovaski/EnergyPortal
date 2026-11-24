using DatabaseInterface;
using Microsoft.EntityFrameworkCore;

namespace EnergyPortal.Tests.Repositories;

public class MigrationTests
{
    [Fact]
    public void Model_matches_latest_migration_snapshot()
    {
        // Same provider setup as production; no connection is opened for this comparison.
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql("Host=unused", o => o.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName))
            .UseSnakeCaseNamingConvention()
            .Options;
        using var db = new ApplicationDbContext(options);

        Assert.False(db.Database.HasPendingModelChanges(), "Model changed without a migration; run dotnet ef migrations add.");
    }
}
