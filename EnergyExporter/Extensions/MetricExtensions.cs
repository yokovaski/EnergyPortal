using DatabaseInterface.Entities;

namespace EnergyExporter.Extensions;

public static class MetricExtensions
{
    public static List<MetricDto> ToMetricDtoList<T>(this IEnumerable<T> metrics) where T : IMetric =>
        metrics
            .Select(m => new MetricDto
            {
                Id = m.Id,
                RaspberryPiId = m.RaspberryPiId,
                Mode = m.Mode,
                UsageNow = m.UsageNow,
                RedeliveryNow = m.RedeliveryNow,
                SolarNow = m.SolarNow,
                UsageTotalHigh = m.UsageTotalHigh,
                RedeliveryTotalHigh = m.RedeliveryTotalHigh,
                UsageTotalLow = m.UsageTotalLow,
                RedeliveryTotalLow = m.RedeliveryTotalLow,
                SolarTotal = m.SolarTotal,
                UsageGasNow = m.UsageGasNow,
                UsageGasTotal = m.UsageGasTotal,
                Created = m.Created,
                Updated = m.Updated
            })
            .ToList();
}