using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using DatabaseInterface.Entities;
using DatabaseInterface.Repositories;
using EnergyPortal.Controllers.Api;
using EnergyPortal.Models.Legacy;
using EnergyPortal.Tests.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace EnergyPortal.Tests.Api;

public class IngestionTests : IAsyncLifetime
{
    private readonly PortalFactory factory = new();

    public Task InitializeAsync() => factory.SeedAsync(db =>
    {
        db.RaspberryPis.Add(new RaspberryPi { RpiKey = "pi-key", UserId = "u1" });
        return Task.CompletedTask;
    });

    public async Task DisposeAsync() => await factory.DisposeAsync();

    private static object Reading(DateTime created) => new
    {
        metrics = new[]
        {
            new
            {
                mode = 2, usageNow = 450, redeliveryNow = 0, solarNow = 0,
                usageTotalHigh = 4_277_964L, redeliveryTotalHigh = 5L, usageTotalLow = 3_405_234L,
                redeliveryTotalLow = 0L, solarTotal = 0L, usageGasNow = 0, usageGasTotal = 7_524_024L,
                created
            }
        }
    };

    [Fact]
    public async Task V3_rejects_missing_key()
    {
        var response = await factory.ClientFor(null).PostAsJsonAsync("/api/v3/energy", Reading(DateTime.UtcNow));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal(0, await factory.QueryAsync(db => db.TenSecondMetrics.CountAsync()));
    }

    [Fact]
    public async Task V3_rejects_unknown_key()
    {
        var client = factory.ClientFor(null);
        client.DefaultRequestHeaders.Add("rpiKey", "wrong");

        var response = await client.PostAsJsonAsync("/api/v3/energy", Reading(DateTime.UtcNow));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal(0, await factory.QueryAsync(db => db.TenSecondMetrics.CountAsync()));
    }

    [Fact]
    public async Task V3_stores_readings_for_known_key()
    {
        var client = factory.ClientFor(null);
        client.DefaultRequestHeaders.Add("rpiKey", "pi-key");
        var created = new DateTime(2026, 11, 19, 20, 0, 0, DateTimeKind.Utc);

        var response = await client.PostAsJsonAsync("/api/v3/energy", Reading(created));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var stored = await factory.QueryAsync(db => db.TenSecondMetrics.SingleAsync());
        Assert.Equal(450, stored.UsageNow);
        Assert.Equal(4_277_964, stored.UsageTotalHigh);
        Assert.Equal(7_524_024, stored.UsageGasTotal);
        Assert.Equal(created, stored.Created);
    }

    private static LegacyModel Legacy(string key, string usageNow = "450") => new()
    {
        RpiKey = key,
        Data =
        [
            new LegacyMetricModel
            {
                Mode = "1", UsageNow = usageNow, RedeliveryNow = "0", SolarNow = 12,
                UsageTotalHigh = "1000", RedeliveryTotalHigh = "2000", UsageTotalLow = "3000",
                RedeliveryTotalLow = "4000", SolarTotal = 5000, UsageGasNow = "0", UsageGasTotal = "6000",
                UnixTimeStamp = 1_700_000_000
            }
        ]
    };

    [Fact]
    public async Task V2_rejects_unknown_key()
    {
        var response = await factory.ClientFor(null).PostAsJsonAsync("/v2/energy", Legacy("wrong"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task V2_stores_legacy_payload()
    {
        var response = await factory.ClientFor(null).PostAsJsonAsync("/v2/energy", Legacy("pi-key"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var stored = await factory.QueryAsync(db => db.TenSecondMetrics.SingleAsync());
        Assert.Equal((450, 12, 1000L, 4000L, 5000L, 6000L, 1),
            (stored.UsageNow, stored.SolarNow, stored.UsageTotalHigh, stored.RedeliveryTotalLow, stored.SolarTotal, stored.UsageGasTotal, stored.Mode));
        Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(1_700_000_000).UtcDateTime, stored.Created);
    }

    [Fact]
    public async Task V2_parses_decimals_independent_of_server_culture()
    {
        var previous = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = new CultureInfo("nl-NL");
        try
        {
            await using var db = TestDb.Create();
            db.RaspberryPis.Add(new RaspberryPi { RpiKey = "k", UserId = "u" });
            await db.SaveChangesAsync();
            var controller = new LegacyApiController(NullLogger<LegacyApiController>.Instance,
                new DbTenSecondMetricRepository(db), new DbDeviceRepository(db));

            Assert.IsType<CreatedResult>(await controller.StoreEnergy(Legacy("k", usageNow: "450.5")));
            Assert.Equal(450, (await db.TenSecondMetrics.SingleAsync()).UsageNow);
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }
}
