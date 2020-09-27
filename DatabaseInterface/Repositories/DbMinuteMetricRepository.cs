using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DatabaseInterface.Entities;
using DatabaseInterface.Extensions;
using Microsoft.EntityFrameworkCore;

namespace DatabaseInterface.Repositories
{
    public class DbMinuteMetricRepository : IMetricRepository
    {
        private readonly ApplicationDbContext dbContext;

        public DbMinuteMetricRepository(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        
        private async Task StoreMetricWithoutSaving(IMetric metric)
        {
            if (!(metric is MinuteMetric minuteMetric))
                throw new Exception($"metric should be of Type {nameof(MinuteMetric)}");
            
            minuteMetric.Created = minuteMetric.Created == default ? 
                DateTime.UtcNow : minuteMetric.Created.ToUniversalTime();
            
            minuteMetric.Updated = DateTime.UtcNow;
            
            await dbContext.MinuteMetrics.AddAsync(minuteMetric);
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
            var query = dbContext.MinuteMetrics
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