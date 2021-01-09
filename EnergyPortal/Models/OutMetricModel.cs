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
        public double IntakeCost { get; set; }
        public double RedeliveryCost { get; set; }
        public double GasCost { get; set; }
        public DateTime DateTime { get; set; }
    }
}