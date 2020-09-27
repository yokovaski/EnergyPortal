using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DatabaseInterface.Entities
{
    public class RaspberryPi
    {
        [Key]
        public long Id { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public string RpiKey { get; set; }
        
        [JsonIgnore]
        public virtual ApplicationUser User { get; set; }
        
        public string UserId { get; set; }

        [JsonIgnore]
        public virtual ICollection<TenSecondMetric> TenSecondMetrics { get; set; }
        
        [JsonIgnore]
        public virtual ICollection<MinuteMetric> MinuteMetrics { get; set; }
        
        [JsonIgnore]
        public virtual ICollection<HourMetric> HourMetrics { get; set; }
    }
}