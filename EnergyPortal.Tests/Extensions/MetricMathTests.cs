using DatabaseInterface.Entities;
using EnergyPortal.Controllers.WebApi;
using EnergyPortal.Extensions;
using EnergyPortal.Models;
using EnergyPortal.Tests.Infrastructure;

namespace EnergyPortal.Tests.Extensions;

public class ToOutMetricModelTests
{
    private static readonly Settings Prices = new()
    {
        HighUsagePricePerKwh = 0.30m,
        LowUsagePricePerKwh = 0.20m,
        HighRedeliveryPricePerKwh = 0.10m,
        LowRedeliveryPricePerKwh = 0.05m,
        GasPrice = 1.00m,
        ElectricityDeliveryPricePerMonth = 31m,
        GasDeliveryPricePerMonth = 62m
    };

    private static (HourMetric current, HourMetric next) OneJanuaryDay(long solarDelta, long redeliveryDelta)
    {
        var current = TestDb.Metric<HourMetric>(1, new DateTime(2026, 1, 10), m =>
        {
            m.UsageTotalHigh = 1000; m.UsageTotalLow = 500;
            m.RedeliveryTotalHigh = 0; m.RedeliveryTotalLow = 0;
            m.UsageGasTotal = 1000; m.SolarTotal = 0;
        });
        var next = TestDb.Metric<HourMetric>(1, new DateTime(2026, 1, 11), m =>
        {
            m.UsageTotalHigh = 3000; m.UsageTotalLow = 1500;
            m.RedeliveryTotalHigh = redeliveryDelta; m.RedeliveryTotalLow = 0;
            m.UsageGasTotal = 3000; m.SolarTotal = solarDelta;
        });
        return (current, next);
    }

    [Fact]
    public void Computes_deltas_costs_and_prorated_fixed_fees()
    {
        var (current, next) = OneJanuaryDay(solarDelta: 2000, redeliveryDelta: 1000);

        var result = current.ToOutMetricModel(next, Prices);

        Assert.Equal(current.Created, result.DateTime);
        Assert.Equal(3000, result.Intake);
        Assert.Equal(1000, result.Redelivery);
        Assert.Equal(2000, result.Solar);
        Assert.Equal(4000, result.Usage); // intake + (solar - redelivery)
        Assert.Equal(2000, result.Gas);
        // 2 kWh * 0.30 + 1 kWh * 0.20 + 31 EUR/month * 1/31 month
        Assert.Equal(1.80m, result.IntakeCost);
        Assert.Equal(-0.10m, result.RedeliveryCost);
        Assert.Equal(1.70m, result.UsageCost);
        // 2 m3 * 1.00 + 62 EUR/month * 1/31 month
        Assert.Equal(4.00m, result.GasCost);
    }

    [Fact]
    public void Ignores_solar_when_it_is_lower_than_redelivery()
    {
        var (current, next) = OneJanuaryDay(solarDelta: 0, redeliveryDelta: 1000);

        var result = current.ToOutMetricModel(next, Prices);

        Assert.Equal(result.Intake, result.Usage);
    }

    [Fact]
    public void Same_timestamp_has_no_fixed_fee()
    {
        var current = TestDb.Metric<HourMetric>(1, new DateTime(2026, 1, 10));

        var result = current.ToOutMetricModel(current, new Settings { ElectricityDeliveryPricePerMonth = 10 });

        Assert.Equal(0m, result.IntakeCost);
        Assert.Equal(0, result.Usage);
    }
}

public class FillMissingTests
{
    private static OutMetricModel At(DateTime t) => new() { DateTime = t, Usage = 1 };
    private static readonly DateTime Ten = new(2026, 3, 1, 10, 0, 0);

    [Fact]
    public void None_returns_input_untouched()
    {
        var input = new List<OutMetricModel> { At(Ten) };
        Assert.Same(input, input.FillMissing(TimeGroup.None, Ten, Ten, false));
    }

    [Fact]
    public void Empty_input_generates_normalized_slots_for_whole_range()
    {
        var result = new List<OutMetricModel>().FillMissing(TimeGroup.Hours, Ten.AddMinutes(30), Ten.AddHours(3), false);

        Assert.Equal([Ten, Ten.AddHours(1), Ten.AddHours(2), Ten.AddHours(3)], result.Select(r => r.DateTime));
        Assert.All(result, r => Assert.Equal(0, r.Usage));
    }

    [Fact]
    public void Normalizes_metric_timestamps_to_the_group()
    {
        var result = new List<OutMetricModel> { At(Ten.AddMinutes(42).AddSeconds(7)) }
            .FillMissing(TimeGroup.Hours, Ten, Ten, false);

        Assert.Equal(Ten, Assert.Single(result).DateTime);
    }

    [Fact]
    public void TenSeconds_readings_off_the_range_start_get_no_empty_slots_in_between()
    {
        // Readings at :04/:14/:24, range starting at :07: slots must line up, no zeros interleaved
        var result = new List<OutMetricModel> { At(Ten.AddSeconds(4)), At(Ten.AddSeconds(14)), At(Ten.AddSeconds(24)) }
            .FillMissing(TimeGroup.TenSeconds, Ten.AddSeconds(7), Ten.AddSeconds(24), false);

        Assert.Equal([Ten, Ten.AddSeconds(10), Ten.AddSeconds(20)], result.Select(r => r.DateTime));
        Assert.All(result, r => Assert.Equal(1, r.Usage));
    }

    [Fact]
    public void Fills_gap_between_metrics()
    {
        var result = new List<OutMetricModel> { At(Ten), At(Ten.AddHours(2)) }
            .FillMissing(TimeGroup.Hours, Ten, Ten.AddHours(2), false);

        Assert.Equal([Ten, Ten.AddHours(1), Ten.AddHours(2)], result.Select(r => r.DateTime).Order());
    }

    [Fact]
    public void Fills_multi_slot_gap_and_returns_sorted()
    {
        var result = new List<OutMetricModel> { At(Ten.AddHours(3)), At(Ten) }
            .FillMissing(TimeGroup.Hours, Ten, Ten.AddHours(3), false);

        Assert.Equal([Ten, Ten.AddHours(1), Ten.AddHours(2), Ten.AddHours(3)], result.Select(r => r.DateTime));
    }

    [Fact]
    public void Without_showRealLast_stops_at_last_metric()
    {
        var result = new List<OutMetricModel> { At(Ten) }
            .FillMissing(TimeGroup.Hours, Ten, Ten.AddHours(5), false);

        Assert.Equal(Ten, Assert.Single(result).DateTime);
    }

    [Fact]
    public void ShowRealLast_does_not_overshoot_range_end()
    {
        var result = new List<OutMetricModel> { At(Ten) }
            .FillMissing(TimeGroup.Hours, Ten, Ten.AddHours(2).AddMinutes(30), showRealLast: true);

        Assert.Equal(Ten.AddHours(2), result.Last().DateTime);
    }

    [Fact]
    public void Skips_local_times_that_do_not_exist_on_dst_change()
    {
        // 2026-03-29 02:00-03:00 does not exist in Amsterdam.
        var night = new DateTime(2026, 3, 29, 0, 0, 0);
        var amsterdam = TimeZoneInfo.FindSystemTimeZoneById("Europe/Amsterdam");

        var result = new List<OutMetricModel>().FillMissing(TimeGroup.Hours, night, night.AddHours(4), false, amsterdam);

        Assert.Equal([0, 1, 3, 4], result.Select(r => r.DateTime.Hour));
    }

    [Fact]
    public void Keeps_first_metric_when_two_normalize_to_same_slot()
    {
        var first = At(Ten.AddMinutes(5));
        var result = new List<OutMetricModel> { first, At(Ten.AddMinutes(10)) }
            .FillMissing(TimeGroup.Hours, Ten, Ten, false);

        Assert.Same(first, Assert.Single(result));
    }

    [Fact]
    public void Fills_leading_gap_before_first_metric()
    {
        var result = new List<OutMetricModel> { At(Ten.AddHours(2)) }
            .FillMissing(TimeGroup.Hours, Ten, Ten.AddHours(2), false);

        Assert.Contains(result, r => r.DateTime == Ten);
        Assert.Contains(result, r => r.DateTime == Ten.AddHours(2) && r.Usage == 1);
    }

    [Fact]
    public void ShowRealLast_pads_until_end_of_range()
    {
        var result = new List<OutMetricModel> { At(Ten) }
            .FillMissing(TimeGroup.Hours, Ten, Ten.AddHours(2), showRealLast: true);

        Assert.Equal([Ten, Ten.AddHours(1), Ten.AddHours(2)], result.Select(r => r.DateTime));
    }

    [Theory]
    [InlineData(TimeGroup.TenSeconds)]
    [InlineData(TimeGroup.Minutes)]
    [InlineData(TimeGroup.Hours)]
    [InlineData(TimeGroup.Days)]
    [InlineData(TimeGroup.Weeks)]
    [InlineData(TimeGroup.Months)]
    [InlineData(TimeGroup.Years)]
    public void Every_group_steps_forward_and_normalizes(TimeGroup group)
    {
        var from = new DateTime(2026, 3, 18, 10, 17, 23);
        var to = from.AddYears(3);

        var result = new List<OutMetricModel>().FillMissing(group, from, group == TimeGroup.Years ? to : from.AddSeconds(1), true);

        Assert.NotEmpty(result);
        Assert.True(result[0].DateTime <= from);
    }

    [Fact]
    public void Unknown_group_throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new List<OutMetricModel>().FillMissing((TimeGroup)99, Ten, Ten, false));
    }
}
