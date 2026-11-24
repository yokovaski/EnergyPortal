using System;
using System.ComponentModel.DataAnnotations;

namespace DatabaseInterface.Entities
{
    /// <summary>A KNMI station that reports air temperature. Id is the WMO-style code from the KNMI files, e.g. "06260".</summary>
    public class WeatherStation
    {
        [Key]
        public string Id { get; set; }
        public string Name { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    /// <summary>Air temperature (°C, 1.5 m) of a station on the whole hour, UTC.</summary>
    public class Temperature
    {
        public string StationId { get; set; }
        public DateTime Created { get; set; }
        public double Value { get; set; }
    }
}
