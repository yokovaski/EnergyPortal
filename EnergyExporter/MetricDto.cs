using DatabaseInterface.Entities;

namespace EnergyExporter;

public class MetricDto : IMetric
{
    public long Id { get; set; }
    public long RaspberryPiId { get; set; }
    public int Mode { get; set; }
    public int UsageNow { get; set; }
    public int RedeliveryNow { get; set; }
    public int SolarNow { get; set; }
    public long UsageTotalHigh { get; set; }
    public long RedeliveryTotalHigh { get; set; }
    public long UsageTotalLow { get; set; }
    public long RedeliveryTotalLow { get; set; }
    public long SolarTotal { get; set; }
    public int UsageGasNow { get; set; }
    public long UsageGasTotal { get; set; }
    public DateTime Created { get; set; }
    public DateTime Updated { get; set; }
}