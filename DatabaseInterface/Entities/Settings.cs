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

        /// <summary>Dutch postcode; its coordinates pick the nearest KNMI station for outdoor temperature.</summary>
        public string Postcode { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        /// <summary>Name of the nearest KNMI station; filled by the API, not stored.</summary>
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public string WeatherStation { get; set; }

        [JsonIgnore]
        public virtual ApplicationUser User { get; set; }
    }
}