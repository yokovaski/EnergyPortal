using DatabaseInterface;
using DatabaseInterface.Entities;
using DatabaseInterface.Repositories;
using EnergyPortal.Tests.Infrastructure;

namespace EnergyPortal.Tests.Repositories;

public class MetricRepositoryTests
{
    public record Case(string Name, Func<ApplicationDbContext, IMetricRepository> Repo, Func<long, DateTime, IMetric> Metric, IMetric WrongType)
    {
        public override string ToString() => Name;
    }

    public static TheoryData<Case> Repositories =>
    [
        new("ten-second", db => new DbTenSecondMetricRepository(db), (id, t) => TestDb.Metric<TenSecondMetric>(id, t), new HourMetric()),
        new("minute", db => new DbMinuteMetricRepository(db), (id, t) => TestDb.Metric<MinuteMetric>(id, t), new HourMetric()),
        new("hour", db => new DbHourMetricRepository(db), (id, t) => TestDb.Metric<HourMetric>(id, t), new MinuteMetric()),
    ];

    private static readonly DateTime Noon = new(2026, 7, 15, 12, 0, 0, DateTimeKind.Utc);

    [Theory, MemberData(nameof(Repositories))]
    public async Task StoreMetric_defaults_created_to_now_and_sets_updated(Case c)
    {
        await using var db = TestDb.Create();
        var metric = c.Metric(1, default);

        await c.Repo(db).StoreMetric(metric);

        Assert.InRange(metric.Created, DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow);
        Assert.InRange(metric.Updated, DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow);
        Assert.Single(await c.Repo(db).GetMetrics(1));
    }

    [Theory, MemberData(nameof(Repositories))]
    public async Task StoreMetric_converts_local_time_to_utc(Case c)
    {
        await using var db = TestDb.Create();
        var local = new DateTime(2026, 7, 15, 12, 0, 0, DateTimeKind.Local);
        var metric = c.Metric(1, local);

        await c.Repo(db).StoreMetric(metric);

        Assert.Equal(local.ToUniversalTime(), metric.Created);
    }

    [Theory, MemberData(nameof(Repositories))]
    public async Task Store_rejects_metric_of_wrong_type(Case c)
    {
        await using var db = TestDb.Create();

        await Assert.ThrowsAsync<Exception>(() => c.Repo(db).StoreMetric(c.WrongType));
    }

    [Theory, MemberData(nameof(Repositories))]
    public async Task GetMetrics_filters_by_device_and_half_open_range_ordered(Case c)
    {
        await using var db = TestDb.Create();
        await c.Repo(db).StoreMetrics(
        [
            c.Metric(1, Noon.AddHours(2)),
            c.Metric(1, Noon),
            c.Metric(1, Noon.AddHours(1)),
            c.Metric(1, Noon.AddHours(-1)),
            c.Metric(2, Noon),
        ]);

        var result = (await c.Repo(db).GetMetrics(1, Noon, Noon.AddHours(2))).ToList();

        Assert.Equal([Noon, Noon.AddHours(1)], result.Select(m => m.Created));
        Assert.Equal(4, (await c.Repo(db).GetMetrics(1)).Count());
    }

    [Theory, MemberData(nameof(Repositories))]
    public async Task GetMetrics_converts_to_user_time_zone(Case c)
    {
        await using var db = TestDb.Create();
        await c.Repo(db).StoreMetric(c.Metric(1, Noon));

        var result = Assert.Single(await c.Repo(db).GetMetrics(1, timeZoneId: "Europe/Amsterdam"));

        Assert.Equal(14, result.Created.Hour);
    }
}

public class TenSecondMetricRepositoryTests
{
    [Fact]
    public async Task GetMinAndMaxSolar_only_counts_producing_readings_in_window()
    {
        await using var db = TestDb.Create();
        var now = DateTime.UtcNow;
        db.TenSecondMetrics.AddRange(
            TestDb.Metric<TenSecondMetric>(1, now, m => { m.SolarNow = 10; m.SolarTotal = 300; }),
            TestDb.Metric<TenSecondMetric>(1, now, m => { m.SolarNow = 10; m.SolarTotal = 100; }),
            TestDb.Metric<TenSecondMetric>(1, now, m => { m.SolarNow = 10; m.SolarTotal = 200; }),
            TestDb.Metric<TenSecondMetric>(1, now, m => { m.SolarNow = 0; m.SolarTotal = 50; }),
            TestDb.Metric<TenSecondMetric>(1, now, m => { m.SolarNow = 150; m.SolarTotal = 0; }),
            TestDb.Metric<TenSecondMetric>(1, now.Date.AddDays(-1), m => { m.SolarNow = 10; m.SolarTotal = 5; }),
            TestDb.Metric<TenSecondMetric>(2, now, m => { m.SolarNow = 10; m.SolarTotal = 9999; }));
        await db.SaveChangesAsync();

        var (min, max) = await new DbTenSecondMetricRepository(db).GetMinAndMaxSolar(1, now.Date, now);

        Assert.Equal((100, 300), (min, max));
    }

    [Fact]
    public async Task GetMinAndMaxSolar_is_zero_without_data()
    {
        await using var db = TestDb.Create();

        Assert.Equal((0L, 0L), await new DbTenSecondMetricRepository(db).GetMinAndMaxSolar(1, DateTime.UtcNow.Date, DateTime.UtcNow));
    }

    [Fact]
    public async Task FirstOrDefaultAsync_applies_query_operation()
    {
        await using var db = TestDb.Create();
        var repo = new DbTenSecondMetricRepository(db);
        await repo.StoreMetrics([TestDb.Metric<TenSecondMetric>(1, DateTime.UtcNow), TestDb.Metric<TenSecondMetric>(2, DateTime.UtcNow)]);

        var found = await repo.FirstOrDefaultAsync(q => q.Where(m => m.RaspberryPiId == 2));
        var missing = await repo.FirstOrDefaultAsync(q => q.Where(m => m.RaspberryPiId == 3));

        Assert.Equal(2, found.RaspberryPiId);
        Assert.Null(missing);
    }
}
