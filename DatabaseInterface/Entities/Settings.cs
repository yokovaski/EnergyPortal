using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DatabaseInterface.Entities
{
    public class Settings
    {
        private const string DefaultTimeZoneId = "Europe/Amsterdam";
        
        [Key]
        public long Id { get; set; }

        public string TimeZoneId { get; set; } = DefaultTimeZoneId;
        
        public bool SolarSystem { get; set; }
        public bool ShowDayName { get; set; }
        public string UserId { get; set; }
        
        public double GasPrice { get; set; }
        public double HighUsagePricePerKwh { get; set; }
        public double LowUsagePricePerKwh { get; set; }
        public double HighRedeliveryPricePerKwh { get; set; }
        public double LowRedeliveryPricePerKwh { get; set; }
        public double ElectricityDeliveryPricePerYear { get; set; }
        public double GasDeliveryPricePerYear { get; set; }

        [JsonIgnore]
        public virtual ApplicationUser User { get; set; }
    }
}