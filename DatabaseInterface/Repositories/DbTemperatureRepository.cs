using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DatabaseInterface.Entities;
using Microsoft.EntityFrameworkCore;

namespace DatabaseInterface.Repositories
{
    public class DbTemperatureRepository(ApplicationDbContext dbContext)
    {
        /// <summary>The station closest to the given point, or null when no station is known yet.</summary>
        public async Task<WeatherStation> NearestStationAsync(double latitude, double longitude)
        {
            // ~50 stations: picking in memory is simpler than a spatial query
            var stations = await dbContext.WeatherStations.AsNoTracking().ToListAsync();
            return stations.MinBy(s => Distance(latitude, longitude, s.Latitude, s.Longitude));
        }

        /// <summary>Hourly temperatures of a station, UTC, from (inclusive) to (exclusive).</summary>
        public Task<List<Temperature>> GetAsync(string stationId, DateTime from, DateTime to) => dbContext.Temperatures
            .AsNoTracking()
            .Where(t => t.StationId == stationId && t.Created >= from && t.Created < to)
            .OrderBy(t => t.Created)
            .ToListAsync();

        /// <summary>Squared distance on an equirectangular projection; good enough to rank stations within the Netherlands.</summary>
        internal static double Distance(double lat1, double lon1, double lat2, double lon2)
        {
            var x = (lon2 - lon1) * Math.Cos((lat1 + lat2) / 2 * Math.PI / 180);
            var y = lat2 - lat1;
            return x * x + y * y;
        }
    }
}
