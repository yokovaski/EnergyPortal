using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DatabaseInterface.Entities;
using DatabaseInterface.Extensions;
using Microsoft.EntityFrameworkCore;

namespace DatabaseInterface.Repositories
{
    public class DbHourMetricRepository : IMetricRepository
    {
        private readonly ApplicationDbContext dbContext;

        public DbHourMetricRepository(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        
        private async Task StoreMetricWithoutSaving(IMetric metric)
        {
            if (!(metric is HourMetric hourMetric))
                throw new Exception($"metric should be of Type {nameof(HourMetric)}");
            
            hourMetric.Created = hourMetric.Created == default ? 
                DateTime.UtcNow : hourMetric.Created.ToUniversalTime();
            
            hourMetric.Updated = DateTime.UtcNow;
            
            await dbContext.HourMetrics.AddAsync(hourMetric);
        }

        public async Task StoreMetric(IMetric metric)
        {
            await StoreMetricWithoutSaving(metric);
            await dbContext.SaveChangesAsync();
        }

        public async Task StoreMetrics(IEnumerable<IMetric> metrics)
        {
            foreach (var metric in metrics)
                await StoreMetricWithoutSaving(metric);
            
            await dbContext.SaveChangesAsync();
        }

        public async Task<IEnumerable<IMetric>> GetMetrics(
            long rpiId, 
            DateTime? start = null, 
            DateTime? end = null,
            string timeZoneId = null)
        {
            var query = dbContext.HourMetrics
                .Where(t => t.RaspberryPiId == rpiId);

            if (start.HasValue)
                query = query.Where(t => t.Created >= start.Value);

            if (end.HasValue)
                query = query.Where(t => t.Created < end.Value);
            
            var results = await query
                .OrderBy(m => m.Created)
                .ToListAsync();

            if (timeZoneId == null)
                return results;

            foreach (var result in results)
                result.Created = result.Created.ConvertToTimeZone(timeZoneId);

            return results;
        }
    }
}