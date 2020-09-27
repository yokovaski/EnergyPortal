using System;
using System.ComponentModel.DataAnnotations;

namespace EnergyPortal.Models
{
    public class InMetricModel
    {
        [Required]
        public int Mode { get; set; }
        
        [Required]
        public int UsageNow { get; set; }
        
        [Required]
        public int RedeliveryNow { get; set; }
        
        [Required]
        public int SolarNow { get; set; }
        
        [Required]
        public long UsageTotalHigh { get; set; }
        
        [Required]
        public long RedeliveryTotalHigh { get; set; }
        
        [Required]
        public long UsageTotalLow { get; set; }
        
        [Required]
        public long RedeliveryTotalLow { get; set; }
        
        [Required]
        public long SolarTotal { get; set; }
        
        [Required]
        public int UsageGasNow { get; set; }
        
        [Required]
        public long UsageGasTotal { get; set; }

        public DateTime Created { get; set; }
    }
}