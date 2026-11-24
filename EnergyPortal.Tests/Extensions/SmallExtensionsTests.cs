using DatabaseInterface.Extensions;
using EnergyPortal.Extensions;

namespace EnergyPortal.Tests.Extensions;

public class SmallExtensionsTests
{
    [Theory]
    [InlineData(0L, 0)]
    [InlineData(1234L, 1.234)]
    [InlineData(-500L, -0.5)]
    [InlineData(1L, 0.001)]
    public void DivideByThousand_converts_watt_to_kilowatt(long value, double expected) =>
        Assert.Equal((decimal)expected, value.DivideByThousand());

    [Fact]
    public void Round_uses_bankers_rounding_like_Math_Round() =>
        Assert.Equal(1.24m, 1.245m.Round(2));

    [Fact]
    public void SelectWithPrevious_pairs_consecutive_items()
    {
        Assert.Equal([3, 5], new[] { 1, 2, 3 }.SelectWithPrevious((p, c) => p + c));
        Assert.Empty(new[] { 1 }.SelectWithPrevious((p, c) => p + c));
        Assert.Empty(Array.Empty<int>().SelectWithPrevious((p, c) => p + c));
    }

    [Fact]
    public void SelectWithNextOrDefault_ends_with_default_next()
    {
        Assert.Equal(["1>2", "2>3", "3>"],
            new[] { "1", "2", "3" }.SelectWithNextOrDefault((c, n) => $"{c}>{n}"));
        Assert.Empty(Array.Empty<string>().SelectWithNextOrDefault((c, n) => c + n));
    }

    [Fact]
    public void GetTimeZoneInfo_falls_back_to_utc()
    {
        Assert.Same(TimeZoneInfo.Utc, ((string)null).GetTimeZoneInfo());
        Assert.Same(TimeZoneInfo.Utc, "Not/AZone".GetTimeZoneInfo());
        Assert.Equal(TimeSpan.FromHours(1), "Europe/Amsterdam".GetTimeZoneInfo().BaseUtcOffset);
    }
}
