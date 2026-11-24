using DatabaseInterface.Extensions;

namespace EnergyPortal.Tests.Extensions;

public class DateTimeExtensionsTests
{
    private static DateTime Utc(int y, int m, int d, int h = 0) => new(y, m, d, h, 0, 0, DateTimeKind.Utc);

    [Theory]
    [InlineData(1, 13)] // winter: CET +1
    [InlineData(7, 14)] // summer: CEST +2
    public void ConvertToTimeZone_applies_amsterdam_offset_including_dst(int month, int expectedHour) =>
        Assert.Equal(expectedHour, Utc(2026, month, 15, 12).ConvertToTimeZone("Europe/Amsterdam").Hour);

    [Fact]
    public void ConvertToTimeZone_without_zone_is_identity() =>
        Assert.Equal(Utc(2026, 7, 15, 12), Utc(2026, 7, 15, 12).ConvertToTimeZone(null));

    [Theory]
    [InlineData("Europe/London", 7, 13)]
    [InlineData("Europe/London", 1, 12)]
    [InlineData("Europe/Lisbon", 7, 13)]
    [InlineData("Etc/UTC", 7, 12)]
    public void ConvertToTimeZone_applies_dst_for_zero_base_offset_zones(string zone, int month, int expectedHour) =>
        Assert.Equal(expectedHour, Utc(2026, month, 15, 12).ConvertToTimeZone(zone).Hour);

    [Fact]
    public void ConvertToUtc_applies_dst_for_london() =>
        Assert.Equal(11, new DateTime(2026, 7, 15, 12, 0, 0).ConvertToUtc("Europe/London").Hour);

    [Theory]
    [InlineData(1, 11)]
    [InlineData(7, 10)]
    public void ConvertToUtc_reverses_amsterdam_local_time(int month, int expectedHour)
    {
        var local = new DateTime(2026, month, 15, 12, 0, 0, DateTimeKind.Unspecified);
        Assert.Equal(expectedHour, local.ConvertToUtc("Europe/Amsterdam").Hour);
    }

    [Fact]
    public void ConvertToUtc_without_zone_is_identity() =>
        Assert.Equal(Utc(2026, 7, 15, 12), Utc(2026, 7, 15, 12).ConvertToUtc(null));

    [Theory]
    [InlineData(2024, 2, 29, true)]
    [InlineData(2026, 2, 28, true)]
    [InlineData(2026, 2, 27, false)]
    [InlineData(2026, 12, 31, true)]
    [InlineData(2026, 4, 30, true)]
    [InlineData(2026, 5, 30, false)]
    public void IsLastDayOfMonth(int y, int m, int d, bool expected) =>
        Assert.Equal(expected, new DateTime(y, m, d).IsLastDayOfMonth());

    [Theory]
    [InlineData(12, 31, true)]
    [InlineData(12, 30, false)]
    [InlineData(1, 31, false)]
    public void IsLastDayOfYear(int m, int d, bool expected) =>
        Assert.Equal(expected, new DateTime(2026, m, d).IsLastDayOfYear());

    [Fact]
    public void IsThisMonth_and_IsThisYear_compare_against_now()
    {
        var now = DateTime.UtcNow;
        Assert.True(now.IsThisMonth());
        Assert.True(now.IsThisYear());
        Assert.False(now.AddYears(-1).IsThisMonth());
        Assert.False(now.AddYears(-1).IsThisYear());
    }
}
