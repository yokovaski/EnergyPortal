using DatabaseInterface;
using DatabaseInterface.Entities;
using Microsoft.EntityFrameworkCore;

namespace EnergyPortal.Tests.Infrastructure;

public static class TestDb
{
    public static DbContextOptions<ApplicationDbContext> Options(string name = null) =>
        new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(name ?? Guid.NewGuid().ToString())
            .Options;

    public static ApplicationDbContext Create(string name = null) => new(Options(name));

    public static T Metric<T>(long raspberryPiId, DateTime created, Action<T> configure = null)
        where T : IMetric, new()
    {
        var metric = new T
        {
            RaspberryPiId = raspberryPiId,
            Created = created,
            Updated = created,
            Mode = 2,
            UsageNow = 500,
            RedeliveryNow = 0,
            SolarNow = 0,
            UsageTotalHigh = 1_000_000,
            UsageTotalLow = 2_000_000,
            RedeliveryTotalHigh = 300_000,
            RedeliveryTotalLow = 400_000,
            SolarTotal = 0,
            UsageGasNow = 0,
            UsageGasTotal = 5_000_000
        };
        configure?.Invoke(metric);
        return metric;
    }
}
