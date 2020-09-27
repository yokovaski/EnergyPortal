using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EnergyPortal.Models.Legacy
{
    public class LegacyMetricModel
    {
        [Required]
        [JsonPropertyName("mode")]
        public string Mode { get; set; }
        
        [Required]
        [JsonPropertyName("usage_now")]
        public string UsageNow { get; set; }
        
        [Required]
        [JsonPropertyName("redelivery_now")]
        public string RedeliveryNow { get; set; }
        
        [Required]
        [JsonPropertyName("solar_now")]
        public int SolarNow { get; set; }
        
        [Required]
        [JsonPropertyName("usage_total_high")]
        public string UsageTotalHigh { get; set; }
        
        [Required]
        [JsonPropertyName("redelivery_total_high")]
        public string RedeliveryTotalHigh { get; set; }
        
        [Required]
        [JsonPropertyName("usage_total_low")]
        public string UsageTotalLow { get; set; }
        
        [Required]
        [JsonPropertyName("redelivery_total_low")]
        public string RedeliveryTotalLow { get; set; }
        
        [Required]
        [JsonPropertyName("solar_total")]
        public long SolarTotal { get; set; }
        
        [Required]
        [JsonPropertyName("usage_gas_now")]
        public string UsageGasNow { get; set; }
        
        [Required]
        [JsonPropertyName("usage_gas_total")]
        public string UsageGasTotal { get; set; }

        [JsonPropertyName("unix_timestamp")]
        public int UnixTimeStamp { get; set; }
    }
}