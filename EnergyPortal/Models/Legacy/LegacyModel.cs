using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EnergyPortal.Models.Legacy
{
    public class LegacyModel
    {
        [Required]
        [JsonPropertyName("data")]
        public ICollection<LegacyMetricModel> Data { get; set; }
        
        [Required]
        [JsonPropertyName("rpi_key")]
        public string RpiKey { get; set; }
    }
}