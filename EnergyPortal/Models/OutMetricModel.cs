using System;
using DatabaseInterface.Entities;

namespace EnergyPortal.Models
{
    public class OutMetricModel
    {
        public long Usage { get; set; }
        public long Intake { get; set; }
        public long Redelivery { get; set; }
        public long Solar { get; set; }
        public long Gas { get; set; }
        public decimal UsageCost { get; set; }
        public decimal IntakeCost { get; set; }
        public decimal RedeliveryCost { get; set; }
        public decimal GasCost { get; set; }
        public DateTime DateTime { get; set; }
    }
}