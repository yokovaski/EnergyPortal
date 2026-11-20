using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using DatabaseInterface.Entities;
using EnergyPortal.Tests.Infrastructure;

namespace EnergyPortal.Tests.Api;

/// <summary>Endpoints used by the Vue frontend (cookie-authenticated in production).</summary>
public class WebApiTests : IAsyncLifetime
{
    private const string User = "user-1";
    private const string UserWithoutPi = "user-2";
    private readonly PortalFactory factory = new();
    private long piId;

    public async Task InitializeAsync()
    {
        await factory.SeedAsync(db =>
        {
            db.Users.Add(new ApplicationUser { Id = User, UserName = "user1@test", Email = "user1@test" });
            db.Users.Add(new ApplicationUser { Id = UserWithoutPi, UserName = "user2@test", Email = "user2@test" });
            db.RaspberryPis.Add(new RaspberryPi { RpiKey = "pi", UserId = User });
            db.Settings.Add(new Settings
            {
                UserId = User, TimeZoneId = "Europe/Amsterdam", ShowDayName = true,
                HighUsagePricePerKwh = 0.3m, LowUsagePricePerKwh = 0.2m, GasPrice = 1.2m
            });
            return Task.CompletedTask;
        });
        piId = await factory.QueryAsync(db => Task.FromResult(db.RaspberryPis.Single().Id));
    }

    public async Task DisposeAsync() => await factory.DisposeAsync();

    private HttpClient Client(string user = User) => factory.ClientFor(user);

    private static async Task<JsonElement> Json(HttpResponseMessage response)
    {
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }

    private Task SeedMetrics<T>(DateTime from, TimeSpan step, int count) where T : class, IMetric, new() =>
        factory.SeedAsync(db =>
        {
            for (var i = 0; i < count; i++)
            {
                db.Add(TestDb.Metric<T>(piId, from + step * i, m =>
                {
                    m.UsageNow = 400 + i;
                    m.UsageTotalHigh = 1_000_000 + i * 100;
                    m.UsageTotalLow = 2_000_000 + i * 50;
                    m.UsageGasTotal = 5_000_000 + i * 10;
                    m.SolarTotal = i * 20;
                }));
            }
            return Task.CompletedTask;
        });

    private static string Iso(DateTime t) => Uri.EscapeDataString(t.ToString("o"));

    [Theory]
    [InlineData("/webapi/v3/settings")]
    [InlineData("/webapi/v3/metrics/hours")]
    [InlineData("/webapi/v3/energy/totals")]
    [InlineData("/")]
    [InlineData("/history")]
    public async Task Requires_authentication(string url)
    {
        var response = await factory.ClientFor(null).GetAsync(url);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData("/")]
    [InlineData("/history")]
    [InlineData("/settings")]
    public async Task Pages_render_vue_mount_point(string url)
    {
        var response = await Client().GetAsync(url);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("id=\"app\"", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Old_settings_url_redirects()
    {
        var response = await Client().GetAsync("/home/settings");

        Assert.Equal(HttpStatusCode.MovedPermanently, response.StatusCode);
        Assert.Equal("/settings", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Unknown_page_shows_styled_404()
    {
        var response = await Client().GetAsync("/bestaat-niet");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Contains("Pagina niet gevonden", await response.Content.ReadAsStringAsync());
    }

    [Theory]
    [InlineData("/Identity/Account/Login", "Inloggen")]
    [InlineData("/Identity/Account/ForgotPassword", "Wachtwoord vergeten")]
    [InlineData("/Identity/Account/Lockout", "Te veel pogingen")]
    public async Task Identity_pages_render_in_dutch(string url, string text)
    {
        var response = await factory.ClientFor(null).GetAsync(url);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(text, await response.Content.ReadAsStringAsync());
    }

    [Theory]
    [InlineData("/Identity/Account/Manage", "Tweestapsverificatie")]
    [InlineData("/Identity/Account/Manage/EnableAuthenticator", "Authenticator-app instellen")]
    [InlineData("/Identity/Account/Manage/DeletePersonalData", "Account verwijderen")]
    public async Task Account_pages_render(string url, string text)
    {
        var response = await Client().GetAsync(url);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(text, await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Error_page_renders_anonymously()
    {
        var response = await factory.ClientFor(null).GetAsync("/Home/Error");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // ---------- settings ----------

    [Fact]
    public async Task Settings_get_returns_user_settings()
    {
        var json = await Json(await Client().GetAsync("/webapi/v3/settings"));

        Assert.Equal("Europe/Amsterdam", json.GetProperty("timeZoneId").GetString());
        Assert.Equal(0.3m, json.GetProperty("highUsagePricePerKwh").GetDecimal());
    }

    [Fact]
    public async Task Settings_get_is_empty_for_user_without_settings()
    {
        var response = await Client(UserWithoutPi).GetAsync("/webapi/v3/settings");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Settings_put_updates_current_user_only()
    {
        var response = await Client().PutAsJsonAsync("/webapi/v3/settings",
            new { solarSystem = true, timeZoneId = "Europe/London", gasPrice = 1.23456m, userId = "someone-else" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await Json(await Client().GetAsync("/webapi/v3/settings"));
        Assert.True(json.GetProperty("solarSystem").GetBoolean());
        Assert.Equal(1.23456m, json.GetProperty("gasPrice").GetDecimal());
        Assert.Equal(User, json.GetProperty("userId").GetString());
    }

    [Theory]
    [InlineData("gasPrice")]
    [InlineData("highUsagePricePerKwh")]
    [InlineData("electricityDeliveryPricePerMonth")]
    public async Task Settings_put_rejects_negative_prices(string field)
    {
        var response = await Client().PutAsJsonAsync("/webapi/v3/settings", new Dictionary<string, object> { [field] = -0.1m });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Settings_put_accepts_negative_feed_in_tariffs()
    {
        var response = await Client().PutAsJsonAsync("/webapi/v3/settings",
            new { highRedeliveryPricePerKwh = -0.05m, lowRedeliveryPricePerKwh = -0.02m });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await Json(await Client().GetAsync("/webapi/v3/settings"));
        Assert.Equal(-0.05m, json.GetProperty("highRedeliveryPricePerKwh").GetDecimal());
    }

    [Theory]
    [InlineData("12345")]
    [InlineData("0123AB")]
    [InlineData("1234 A")]
    public async Task Settings_put_rejects_malformed_postcode(string postcode)
    {
        var response = await Client().PutAsJsonAsync("/webapi/v3/settings", new { postcode });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Settings_put_ignores_client_coordinates()
    {
        var response = await Client().PutAsJsonAsync("/webapi/v3/settings", new { latitude = 1.0, longitude = 2.0 });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await Json(await Client().GetAsync("/webapi/v3/settings"));
        Assert.Equal(JsonValueKind.Null, json.GetProperty("latitude").ValueKind);
    }

    // ---------- outdoor temperature ----------

    [Fact]
    public async Task Metrics_have_no_temperature_without_location()
    {
        var start = DateTime.UtcNow.Date.AddDays(-3);
        await SeedMetrics<HourMetric>(start, TimeSpan.FromHours(6), 12);

        var json = await Json(await Client().GetAsync($"/webapi/v3/metrics/days?from={Iso(start)}"));

        Assert.Empty(json.GetProperty("temperature").EnumerateArray());
    }

    [Fact]
    public async Task Metrics_use_nearest_station_for_temperature_and_degree_days()
    {
        var start = DateTime.UtcNow.Date.AddDays(-3);
        await SeedMetrics<HourMetric>(start, TimeSpan.FromHours(6), 12);
        await factory.SeedAsync(async db =>
        {
            var settings = db.Settings.Single(s => s.UserId == User);
            // Utrecht; De Bilt is ~5 km away, Maastricht ~150 km
            settings.Latitude = 52.09;
            settings.Longitude = 5.12;
            db.WeatherStations.Add(new WeatherStation { Id = "06260", Name = "De Bilt", Latitude = 52.10, Longitude = 5.18 });
            db.WeatherStations.Add(new WeatherStation { Id = "06380", Name = "Maastricht", Latitude = 50.91, Longitude = 5.77 });
            for (var t = start.AddDays(-1); t < DateTime.UtcNow; t = t.AddHours(1))
            {
                db.Temperatures.Add(new Temperature { StationId = "06260", Created = t, Value = 8 });
                db.Temperatures.Add(new Temperature { StationId = "06380", Created = t, Value = 30 });
            }
            await Task.CompletedTask;
        });

        var json = await Json(await Client().GetAsync($"/webapi/v3/metrics/days?from={Iso(start)}"));

        var day = json.GetProperty("temperature")[1];
        Assert.Equal(json.GetProperty("intake")[1][0].GetInt64(), day[0].GetInt64());
        Assert.Equal(8, day[1].GetDouble());
        // 18 - 8 = 10 degree days for a full day (23 or 25 hours on a DST switch)
        Assert.InRange(json.GetProperty("degreeDays")[1][1].GetDouble(), 9.5, 10.5);

        var settingsJson = await Json(await Client().GetAsync("/webapi/v3/settings"));
        Assert.Equal("De Bilt", settingsJson.GetProperty("weatherStation").GetString());
    }

    // ---------- account ----------

    [Fact]
    public async Task Account_returns_user_and_pi_key()
    {
        var json = await Json(await Client().GetAsync("/webapi/v3/account"));

        Assert.Equal("user1@test", json.GetProperty("email").GetString());
        Assert.Equal("pi", json.GetProperty("rpiKey").GetString());
    }

    [Fact]
    public async Task Account_has_no_key_for_user_without_pi()
    {
        var json = await Json(await Client(UserWithoutPi).GetAsync("/webapi/v3/account"));
        Assert.Equal(JsonValueKind.Null, json.GetProperty("rpiKey").ValueKind);
    }

    [Theory]
    [InlineData("GET")]
    [InlineData("PUT")]
    public async Task Settings_return_404_for_unknown_user(string method)
    {
        var request = new HttpRequestMessage(new HttpMethod(method), "/webapi/v3/settings")
        {
            Content = method == "PUT" ? JsonContent.Create(new { }) : null
        };

        var response = await Client("ghost").SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Time_zones_lists_iana_names()
    {
        var json = await Json(await Client().GetAsync("/webapi/v3/settings/time-zones"));

        Assert.Contains(json.GetProperty("timeZones").EnumerateArray(),
            t => t.GetProperty("id").GetString() == "Europe/Amsterdam");
    }

    // ---------- metrics ----------

    [Fact]
    public async Task Ten_second_metrics_return_series_in_kw()
    {
        var start = DateTime.UtcNow.AddMinutes(-5);
        await SeedMetrics<TenSecondMetric>(start, TimeSpan.FromSeconds(10), 6);

        var json = await Json(await Client().GetAsync($"/webapi/v3/metrics/tenseconds?from={Iso(start.AddSeconds(-1))}"));

        Assert.Equal("HH:mm:ss", json.GetProperty("format").GetString());
        Assert.Equal("Europe/Amsterdam", json.GetProperty("userTimeZone").GetString());
        var intake = json.GetProperty("intake").EnumerateArray().Select(p => p[1].GetDecimal()).ToList();
        Assert.Contains(0.400m, intake);
        Assert.Equal(json.GetProperty("timestamps").GetArrayLength(), json.GetProperty("usage").GetArrayLength());
    }

    [Fact]
    public async Task Minute_metrics_use_minute_table()
    {
        var start = DateTime.UtcNow.AddMinutes(-30);
        await SeedMetrics<MinuteMetric>(start, TimeSpan.FromMinutes(1), 10);

        var json = await Json(await Client().GetAsync($"/webapi/v3/metrics/minutes?from={Iso(start.AddMinutes(-1))}"));

        Assert.Contains(json.GetProperty("intake").EnumerateArray(), p => p[1].GetDecimal() == 0.405m);
    }

    [Fact]
    public async Task Five_minute_metrics_average_minute_rows_per_bucket()
    {
        var start = new DateTime(2026, 11, 20, 9, 0, 0, DateTimeKind.Utc);
        await SeedMetrics<MinuteMetric>(start, TimeSpan.FromMinutes(1), 10); // usage 400..409

        var json = await Json(await Client().GetAsync(
            $"/webapi/v3/metrics/fiveminutes?from={Iso(start)}&to={Iso(start.AddMinutes(10))}"));

        Assert.Equal("HH:mm", json.GetProperty("format").GetString());
        var epochs = json.GetProperty("epochs").EnumerateArray().Select(e => e.GetInt64()).ToList();
        Assert.Equal([0L, 300_000L], epochs.Take(2).Select(e => e - epochs[0]));
        var intake = json.GetProperty("intake").EnumerateArray().Select(p => p[1].GetDecimal()).ToList();
        Assert.Equal([0.402m, 0.407m], intake.Take(2)); // mean of 400..404 and 405..409
    }

    [Fact]
    public async Task Hour_metrics_use_hour_format()
    {
        var start = DateTime.UtcNow.AddHours(-10);
        await SeedMetrics<HourMetric>(start, TimeSpan.FromHours(1), 5);

        var json = await Json(await Client().GetAsync($"/webapi/v3/metrics/hours?from={Iso(start.AddHours(-1))}"));

        Assert.Equal("MM-dd HH:mm", json.GetProperty("format").GetString());
    }

    [Theory]
    [InlineData("days", "yyyy-MM-dd (dddd)", 10)]
    [InlineData("months", "yyyy-MM", 90)]
    [InlineData("years", "yyyy", 800)]
    public async Task Aggregated_groups_compute_consumption_and_costs(string group, string format, int days)
    {
        var start = DateTime.UtcNow.Date.AddDays(-days);
        await SeedMetrics<HourMetric>(start, TimeSpan.FromHours(6), days * 4);

        var json = await Json(await Client().GetAsync(
            $"/webapi/v3/metrics/{group}?from={Iso(start)}&skipTimeSpanCheck=true"));

        Assert.Equal(format, json.GetProperty("format").GetString());
        // Each 6h step adds 100 Wh high + 50 Wh low; some bucket must show positive intake and cost.
        Assert.Contains(json.GetProperty("intake").EnumerateArray(), p => p[1].GetDecimal() > 0);
        Assert.Contains(json.GetProperty("intakeCosts").EnumerateArray(), p => p[1].GetDecimal() > 0);
        Assert.Contains(json.GetProperty("gas").EnumerateArray(), p => p[1].GetDecimal() > 0);
    }

    [Fact]
    public async Task Weeks_are_seven_day_buckets_from_the_start_day()
    {
        var start = DateTime.UtcNow.Date.AddDays(-21);
        await SeedMetrics<HourMetric>(start, TimeSpan.FromHours(6), 21 * 4);

        var json = await Json(await Client().GetAsync($"/webapi/v3/metrics/weeks?from={Iso(start)}"));

        Assert.Equal("yyyy-MM-dd (dddd)", json.GetProperty("format").GetString());
        var slots = json.GetProperty("timestamps").EnumerateArray().Select(t => DateTime.Parse(t.GetString())).ToList();
        Assert.Equal(start.Date, slots[0]);
        Assert.All(slots.Zip(slots.Skip(1)), p => Assert.Equal(TimeSpan.FromDays(7), p.Second - p.First));
        // 28 readings per week, each adding 100 Wh high + 50 Wh low.
        Assert.Equal(4.2m, json.GetProperty("intake")[0][1].GetDecimal());
        Assert.Equal(4.2m, json.GetProperty("intake")[1][1].GetDecimal());
    }

    [Fact]
    public async Task Weeks_stay_on_the_seven_day_grid_when_a_week_starts_with_a_gap()
    {
        var start = DateTime.UtcNow.Date.AddDays(-14);
        // First reading three days into the first week.
        await SeedMetrics<HourMetric>(start.AddDays(3), TimeSpan.FromHours(6), 11 * 4);

        var json = await Json(await Client().GetAsync($"/webapi/v3/metrics/weeks?from={Iso(start)}"));

        var slots = json.GetProperty("timestamps").EnumerateArray().Select(t => DateTime.Parse(t.GetString())).ToList();
        Assert.Equal([start.Date, start.Date.AddDays(7)], slots.Take(2));
        Assert.All(slots.Zip(slots.Skip(1)), p => Assert.Equal(TimeSpan.FromDays(7), p.Second - p.First));
    }

    [Fact]
    public async Task Days_work_for_user_without_settings()
    {
        await factory.SeedAsync(db =>
        {
            db.RaspberryPis.Add(new RaspberryPi { RpiKey = "pi2", UserId = UserWithoutPi });
            return Task.CompletedTask;
        });

        var response = await Client(UserWithoutPi).GetAsync($"/webapi/v3/metrics/days?from={Iso(DateTime.UtcNow.AddDays(-3))}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData("tenseconds", 4)]   // > 3 h
    [InlineData("minutes", 13)]     // > 12 h
    [InlineData("fiveminutes", 73)] // > 3 d
    [InlineData("hours", 24 * 31)]  // > 30 d
    [InlineData("days", 24 * 701)]  // > 700 d
    public async Task Rejects_ranges_too_large_for_resolution(string group, int hours)
    {
        var response = await Client().GetAsync($"/webapi/v3/metrics/{group}?from={Iso(DateTime.UtcNow.AddHours(-hours))}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Rejects_negative_range()
    {
        var from = DateTime.UtcNow.AddHours(-1);
        var response = await Client().GetAsync($"/webapi/v3/metrics/hours?from={Iso(from)}&to={Iso(from.AddHours(-1))}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Metrics_return_404_for_user_without_pi()
    {
        var response = await Client(UserWithoutPi).GetAsync($"/webapi/v3/metrics/hours?from={Iso(DateTime.UtcNow.AddHours(-2))}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Last_reading_returns_latest_ten_second_values()
    {
        await SeedMetrics<TenSecondMetric>(DateTime.UtcNow.AddSeconds(-30), TimeSpan.FromSeconds(10), 3);

        var json = await Json(await Client().GetAsync("/webapi/v3/metrics/last-reading"));

        Assert.Contains(json.GetProperty("intake").EnumerateArray(), p => p[1].GetDecimal() == 0.402m);
    }

    // ---------- energy ----------

    [Fact]
    public async Task Total_today_is_zero_without_readings()
    {
        var json = await Json(await Client().GetAsync("/webapi/v3/energy/total-today"));

        Assert.Equal(0, json.GetProperty("usageTotalHigh").GetDouble());
        Assert.Equal(0, json.GetProperty("totalGas").GetDouble());
    }

    [Fact]
    public async Task Total_today_is_difference_between_first_and_last_reading()
    {
        var now = DateTime.UtcNow;
        await factory.SeedAsync(db =>
        {
            db.Add(TestDb.Metric<TenSecondMetric>(piId, now.AddSeconds(-20), m =>
                { m.UsageTotalHigh = 10_000; m.UsageTotalLow = 0; m.UsageGasTotal = 1_000; m.SolarNow = 5; m.SolarTotal = 100; }));
            db.Add(TestDb.Metric<TenSecondMetric>(piId, now.AddSeconds(-10), m =>
                { m.UsageTotalHigh = 12_500; m.UsageTotalLow = 0; m.UsageGasTotal = 1_750; m.SolarNow = 5; m.SolarTotal = 1_100; }));
            return Task.CompletedTask;
        });

        var json = await Json(await Client().GetAsync("/webapi/v3/energy/total-today"));

        Assert.Equal(2.5, json.GetProperty("usageTotalHigh").GetDouble());
        Assert.Equal(0.75, json.GetProperty("totalGas").GetDouble());
        Assert.Equal(1.0, json.GetProperty("totalSolar").GetDouble());
    }

    [Fact]
    public async Task Total_yesterday_until_now_only_counts_yesterday()
    {
        var dayAgo = DateTime.UtcNow.AddDays(-1);
        await factory.SeedAsync(db =>
        {
            db.Add(TestDb.Metric<TenSecondMetric>(piId, dayAgo.AddSeconds(-20), m => { m.UsageTotalHigh = 10_000; }));
            db.Add(TestDb.Metric<TenSecondMetric>(piId, dayAgo.AddSeconds(-10), m => { m.UsageTotalHigh = 11_000; }));
            // later yesterday (after "now" yesterday) and today: not counted
            db.Add(TestDb.Metric<TenSecondMetric>(piId, dayAgo.AddMinutes(5), m => { m.UsageTotalHigh = 50_000; }));
            db.Add(TestDb.Metric<TenSecondMetric>(piId, DateTime.UtcNow.AddSeconds(-5), m => { m.UsageTotalHigh = 90_000; }));
            return Task.CompletedTask;
        });

        var json = await Json(await Client().GetAsync("/webapi/v3/energy/total-yesterday-until-now"));

        Assert.Equal(1.0, json.GetProperty("usageTotalHigh").GetDouble());
    }

    [Fact]
    public async Task Total_today_rejects_unknown_user()
    {
        var response = await Client("ghost").GetAsync("/webapi/v3/energy/total-today");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Totals_return_latest_meter_readings()
    {
        await SeedMetrics<TenSecondMetric>(DateTime.UtcNow.AddMinutes(-1), TimeSpan.FromSeconds(10), 3);

        var json = await Json(await Client().GetAsync("/webapi/v3/energy/totals"));

        Assert.Equal(1_000_200, json.GetProperty("usageTotalHigh").GetInt64());
    }

    [Fact]
    public async Task Total_today_returns_404_for_user_without_pi()
    {
        var response = await Client(UserWithoutPi).GetAsync("/webapi/v3/energy/total-today");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Totals_return_404_without_readings()
    {
        var response = await Client().GetAsync("/webapi/v3/energy/totals");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Totals_return_404_for_unknown_user()
    {
        var response = await Client("ghost").GetAsync("/webapi/v3/energy/totals");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
