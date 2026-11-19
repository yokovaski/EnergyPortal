using DatabaseInterface.Entities;
using DatabaseInterface.Repositories;
using EnergyPortal.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace EnergyPortal.Tests.Repositories;

public class SettingsRepositoryTests
{
    [Fact]
    public async Task Get_returns_null_for_unknown_user()
    {
        await using var db = TestDb.Create();
        Assert.Null(await new DbSettingsRepository(db).GetSettingsAsync("nobody"));
    }

    [Fact]
    public async Task Set_inserts_then_updates_all_fields()
    {
        var name = Guid.NewGuid().ToString();
        await using (var db = TestDb.Create(name))
            await new DbSettingsRepository(db).SetSettingsAsync(new Settings { UserId = "u1", GasPrice = 1m });

        await using (var db = TestDb.Create(name))
        {
            await new DbSettingsRepository(db).SetSettingsAsync(new Settings
            {
                UserId = "u1",
                SolarSystem = true,
                ShowDayName = true,
                TimeZoneId = "Europe/London",
                HighUsagePricePerKwh = 0.3m,
                LowUsagePricePerKwh = 0.2m,
                HighRedeliveryPricePerKwh = 0.1m,
                LowRedeliveryPricePerKwh = 0.05m,
                GasPrice = 1.2m,
                ElectricityDeliveryPricePerMonth = 7m,
                GasDeliveryPricePerMonth = 8m
            });
        }

        await using (var db = TestDb.Create(name))
        {
            var stored = Assert.Single(await db.Settings.ToListAsync());
            Assert.True(stored.SolarSystem);
            Assert.True(stored.ShowDayName);
            Assert.Equal("Europe/London", stored.TimeZoneId);
            Assert.Equal(
                [0.3m, 0.2m, 0.1m, 0.05m, 1.2m, 7m, 8m],
                [stored.HighUsagePricePerKwh, stored.LowUsagePricePerKwh, stored.HighRedeliveryPricePerKwh,
                 stored.LowRedeliveryPricePerKwh, stored.GasPrice, stored.ElectricityDeliveryPricePerMonth,
                 stored.GasDeliveryPricePerMonth]);
        }
    }

    [Fact]
    public void New_settings_default_to_amsterdam() =>
        Assert.Equal("Europe/Amsterdam", new Settings().TimeZoneId);
}

public class DeviceRepositoryTests
{
    [Fact]
    public async Task GetDevice_finds_by_key()
    {
        await using var db = TestDb.Create();
        db.RaspberryPis.Add(new RaspberryPi { RpiKey = "abc", UserId = "u1" });
        await db.SaveChangesAsync();

        var repo = new DbDeviceRepository(db);

        Assert.Equal("u1", (await repo.GetDevice("abc")).UserId);
        Assert.Null(await repo.GetDevice("nope"));
    }

    [Fact]
    public async Task StoreDevices_maps_old_ids_to_new_or_existing_ids()
    {
        await using var db = TestDb.Create();
        db.RaspberryPis.Add(new RaspberryPi { RpiKey = "existing", UserId = "u1" });
        await db.SaveChangesAsync();
        var existingId = (await db.RaspberryPis.SingleAsync()).Id;

        var mapping = await new DbDeviceRepository(db).StoreDevices(
        [
            new RaspberryPi { Id = 100, RpiKey = "dup", UserId = "u1" },
            new RaspberryPi { Id = 200, RpiKey = "new", UserId = "u2" }
        ]);

        Assert.Equal(existingId, mapping[100]);
        var created = await db.RaspberryPis.SingleAsync(r => r.UserId == "u2");
        Assert.Equal(created.Id, mapping[200]);
        Assert.Equal(2, await db.RaspberryPis.CountAsync());
    }
}

public class UserRepositoryTests
{
    [Fact]
    public void Constructor_rejects_null_context() =>
        Assert.Throws<ArgumentNullException>(() => new DbUserRepository(null));

    [Fact]
    public async Task StoreUsers_skips_existing_emails()
    {
        await using var db = TestDb.Create();
        db.Users.Add(new ApplicationUser { Id = "1", Email = "a@x.nl", UserName = "a@x.nl" });
        await db.SaveChangesAsync();

        await new DbUserRepository(db).StoreUsers(
        [
            new ApplicationUser { Id = "2", Email = "a@x.nl", UserName = "a@x.nl" },
            new ApplicationUser { Id = "3", Email = "b@x.nl", UserName = "b@x.nl" }
        ]);

        Assert.Equal(["1", "3"], await db.Users.OrderBy(u => u.Id).Select(u => u.Id).ToListAsync());
    }
}
