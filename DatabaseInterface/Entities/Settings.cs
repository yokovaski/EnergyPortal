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
        
        public decimal GasPrice { get; set; }
        public decimal HighUsagePricePerKwh { get; set; }
        public decimal LowUsagePricePerKwh { get; set; }
        public decimal HighRedeliveryPricePerKwh { get; set; }
        public decimal LowRedeliveryPricePerKwh { get; set; }
        public decimal ElectricityDeliveryPricePerMonth { get; set; }
        public decimal GasDeliveryPricePerMonth { get; set; }

        [JsonIgnore]
        public virtual ApplicationUser User { get; set; }
    }
}