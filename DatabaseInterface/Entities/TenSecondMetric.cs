using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DatabaseInterface.Entities
{
    public class TenSecondMetric : IMetric
    {
        [Key]
        public long Id { get; set; }
        
        [JsonIgnore]
        public virtual RaspberryPi RaspberryPi { get; set; }
        
        [JsonIgnore]
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
}