using DatabaseInterface;
using DatabaseInterface.Entities;
using EnergyPortal.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace EnergyPortal.Tests.Worker;

public class WorkerTests
{
    private readonly string dbName = Guid.NewGuid().ToString();
    private readonly ServiceProvider provider;
    private readonly long piId;

    // Whole minutes/hours in the past, so the worker treats them as complete.
    private readonly DateTime ThisMinute = Truncate(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    private readonly DateTime ThisHour = Truncate(DateTime.UtcNow, TimeSpan.FromHours(1));

    public WorkerTests()
    {
        provider = new ServiceCollection()
            .AddDbContext<ApplicationDbContext>(o => o.UseInMemoryDatabase(dbName))
            .BuildServiceProvider();
        using var db = Db();
        var pi = new RaspberryPi { RpiKey = "key", UserId = "u1" };
        db.RaspberryPis.Add(pi);
        db.SaveChanges();
        piId = pi.Id;
    }

    private static DateTime Truncate(DateTime t, TimeSpan unit) =>
        new(t.Ticks - t.Ticks % unit.Ticks, DateTimeKind.Utc);

    private ApplicationDbContext Db() => TestDb.Create(dbName);

    private EnergyWorker.Worker NewWorker() => new(NullLogger<EnergyWorker.Worker>.Instance, provider);

    private async Task Seed(params object[] entities)
    {
        await using var db = Db();
        db.AddRange(entities);
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task Aggregates_ten_second_readings_into_completed_minutes()
    {
        var minute = ThisMinute.AddMinutes(-5);
        await Seed(
            TestDb.Metric<TenSecondMetric>(piId, minute, m => { m.UsageNow = 100; m.UsageTotalHigh = 10; m.Mode = 1; }),
            TestDb.Metric<TenSecondMetric>(piId, minute.AddSeconds(10), m => { m.UsageNow = 201; m.UsageTotalHigh = 30; m.Mode = 2; }),
            TestDb.Metric<TenSecondMetric>(piId, minute.AddMinutes(1), m => { m.UsageNow = 50; }),
            // readings in the current (incomplete) minute or later are ignored
            TestDb.Metric<TenSecondMetric>(piId, ThisMinute.AddMinutes(2)));

        await NewWorker().RunOnceAsync(CancellationToken.None);

        await using var db = Db();
        var minutes = await db.MinuteMetrics.OrderBy(m => m.Created).ToListAsync();
        Assert.Equal([minute, minute.AddMinutes(1)], minutes.Select(m => m.Created));
        Assert.Equal(150, minutes[0].UsageNow); // round(150.5) -> banker's rounding
        Assert.Equal(30, minutes[0].UsageTotalHigh); // totals use max
        Assert.Equal(2, minutes[0].Mode);
        Assert.Equal(piId, minutes[0].RaspberryPiId);
    }

    [Fact]
    public async Task Does_not_aggregate_the_same_minute_twice()
    {
        var minute = ThisMinute.AddMinutes(-3);
        await Seed(TestDb.Metric<TenSecondMetric>(piId, minute));
        await NewWorker().RunOnceAsync(CancellationToken.None);

        await Seed(TestDb.Metric<TenSecondMetric>(piId, minute.AddMinutes(1)));
        await NewWorker().RunOnceAsync(CancellationToken.None);

        await using var db = Db();
        Assert.Equal([minute, minute.AddMinutes(1)], await db.MinuteMetrics.OrderBy(m => m.Created).Select(m => m.Created).ToListAsync());
    }

    [Fact]
    public async Task Aggregates_minutes_into_completed_hours()
    {
        var hour = ThisHour.AddHours(-2);
        await Seed(
            TestDb.Metric<MinuteMetric>(piId, hour, m => { m.SolarNow = 10; m.SolarTotal = 100; }),
            TestDb.Metric<MinuteMetric>(piId, hour.AddMinutes(30), m => { m.SolarNow = 20; m.SolarTotal = 200; }),
            TestDb.Metric<MinuteMetric>(piId, ThisHour.AddHours(2)));

        await NewWorker().RunOnceAsync(CancellationToken.None);

        await using var db = Db();
        var stored = Assert.Single(await db.HourMetrics.ToListAsync());
        Assert.Equal(hour, stored.Created);
        Assert.Equal(15, stored.SolarNow);
        Assert.Equal(200, stored.SolarTotal);
    }

    [Fact]
    public async Task Continues_hours_after_last_stored_hour()
    {
        var lastHour = ThisHour.AddHours(-3);
        await Seed(
            TestDb.Metric<HourMetric>(piId, lastHour),
            TestDb.Metric<MinuteMetric>(piId, lastHour.AddMinutes(10)), // already covered
            TestDb.Metric<MinuteMetric>(piId, lastHour.AddHours(1)));

        await NewWorker().RunOnceAsync(CancellationToken.None);

        await using var db = Db();
        Assert.Equal([lastHour, lastHour.AddHours(1)], await db.HourMetrics.OrderBy(m => m.Created).Select(m => m.Created).ToListAsync());
    }

    [Fact]
    public async Task Deletes_ten_second_readings_older_than_two_weeks_once_hours_are_current()
    {
        await Seed(
            TestDb.Metric<HourMetric>(piId, ThisHour.AddHours(-1)),
            TestDb.Metric<TenSecondMetric>(piId, DateTime.UtcNow.AddDays(-20)),
            TestDb.Metric<TenSecondMetric>(piId, DateTime.UtcNow.AddDays(-15)),
            TestDb.Metric<MinuteMetric>(piId, DateTime.UtcNow.AddMonths(-4)),
            TestDb.Metric<MinuteMetric>(piId, DateTime.UtcNow.AddMonths(-1)));

        await NewWorker().RunOnceAsync(CancellationToken.None);

        await using var db = Db();
        var cutoff = DateTime.UtcNow.AddDays(-14).Date;
        Assert.DoesNotContain(await db.TenSecondMetrics.ToListAsync(), m => m.Created < cutoff);
        Assert.DoesNotContain(await db.MinuteMetrics.ToListAsync(), m => m.Created < DateTime.UtcNow.AddMonths(-3).Date);
        Assert.Contains(await db.MinuteMetrics.ToListAsync(), m => m.Created.Date == DateTime.UtcNow.AddMonths(-1).Date);
    }

    [Fact]
    public async Task Cleanup_only_touches_the_pi_whose_hours_are_current()
    {
        long otherPi;
        await using (var db = Db())
        {
            var pi = new RaspberryPi { RpiKey = "other", UserId = "u2" };
            db.RaspberryPis.Add(pi);
            await db.SaveChangesAsync();
            otherPi = pi.Id;
        }
        await Seed(
            TestDb.Metric<HourMetric>(piId, ThisHour.AddHours(-1)),
            TestDb.Metric<HourMetric>(otherPi, DateTime.UtcNow.AddMonths(-6)), // other Pi is behind
            TestDb.Metric<TenSecondMetric>(otherPi, DateTime.UtcNow.AddDays(-20)),
            TestDb.Metric<MinuteMetric>(otherPi, DateTime.UtcNow.AddMonths(-4)));

        await NewWorker().RunOnceAsync(CancellationToken.None);

        await using var check = Db();
        Assert.Equal(1, await check.TenSecondMetrics.CountAsync(m => m.RaspberryPiId == otherPi));
        Assert.True(await check.MinuteMetrics.AnyAsync(m => m.RaspberryPiId == otherPi && m.Created < DateTime.UtcNow.AddMonths(-3)));
    }

    [Fact]
    public async Task Keeps_old_readings_while_hour_aggregation_is_behind()
    {
        await Seed(
            TestDb.Metric<HourMetric>(piId, DateTime.UtcNow.AddMonths(-6)),
            TestDb.Metric<TenSecondMetric>(piId, DateTime.UtcNow.AddMonths(-5)));

        await NewWorker().RunOnceAsync(CancellationToken.None);

        await using var db = Db();
        Assert.Equal(1, await db.TenSecondMetrics.CountAsync());
    }

    [Fact]
    public async Task Background_loop_runs_until_stopped()
    {
        await Seed(TestDb.Metric<TenSecondMetric>(piId, ThisMinute.AddMinutes(-2)));
        var worker = NewWorker();

        await worker.StartAsync(CancellationToken.None);
        var deadline = DateTime.UtcNow.AddSeconds(5);
        while (DateTime.UtcNow < deadline)
        {
            await using var db = Db();
            if (await db.MinuteMetrics.AnyAsync()) break;
            await Task.Delay(20);
        }
        await worker.StopAsync(CancellationToken.None);

        await using var check = Db();
        Assert.True(await check.MinuteMetrics.AnyAsync());
    }

    [Fact]
    public async Task Background_loop_survives_errors()
    {
        var broken = new EnergyWorker.Worker(NullLogger<EnergyWorker.Worker>.Instance, new ServiceCollection().BuildServiceProvider());

        await broken.StartAsync(CancellationToken.None);
        await Task.Delay(50);
        await broken.StopAsync(CancellationToken.None);

        Assert.True(broken.ExecuteTask!.IsCompletedSuccessfully);
    }
}
