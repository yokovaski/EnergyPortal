using System;

namespace DatabaseInterface.Entities
{
    public interface IMetric
    {
        long Id { get; set; }
        long RaspberryPiId { get; set; }
        int Mode { get; set; }
        int UsageNow { get; set; }
        int RedeliveryNow { get; set; }
        int SolarNow { get; set; }
        long UsageTotalHigh { get; set; }
        long RedeliveryTotalHigh { get; set; }
        long UsageTotalLow { get; set; }
        long RedeliveryTotalLow { get; set; }
        long SolarTotal { get; set; }
        int UsageGasNow { get; set; }
        long UsageGasTotal { get; set; }
        DateTime Created { get; set; }
        DateTime Updated { get; set; }
    }
}