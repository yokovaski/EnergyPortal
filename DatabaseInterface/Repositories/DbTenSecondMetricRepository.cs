using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DatabaseInterface.Entities;
using DatabaseInterface.Extensions;
using Microsoft.EntityFrameworkCore;

namespace DatabaseInterface.Repositories
{
    public class DbTenSecondMetricRepository : IMetricRepository
    {
        private readonly ApplicationDbContext dbContext;

        public DbTenSecondMetricRepository(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        private async Task StoreMetricWithoutSaving(IMetric metric)
        {
            if (!(metric is TenSecondMetric tenSecondMetric))
                throw new Exception($"metric should be of Type {nameof(TenSecondMetric)}");
            
            tenSecondMetric.Created = tenSecondMetric.Created == default ? 
                DateTime.UtcNow : tenSecondMetric.Created.ToUniversalTime();
            
            tenSecondMetric.Updated = DateTime.UtcNow;
            
            await dbContext.TenSecondMetrics.AddAsync(tenSecondMetric);
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
            string timeZoneId = null
            )
        {
            var query = dbContext.TenSecondMetrics
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

        public async Task<(long minSolar, long maxSolar)> GetMinAndMaxSolarToday(long rpiId, string timeZoneId)
        {
            var now = DateTime.UtcNow;
            var midnight = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Unspecified);
            var midnightUtc = midnight.ConvertToUtc(timeZoneId);

            var query = dbContext.TenSecondMetrics
                .Where(t => t.RaspberryPiId == rpiId)
                .Where(t => t.Created > midnightUtc)
                .Where(t => t.SolarNow > 0);

            var lowest = await query
                .OrderBy(t => t.SolarTotal)
                .FirstOrDefaultAsync();
            
            var highest = await query
                .OrderByDescending(t => t.SolarTotal)
                .FirstOrDefaultAsync();

            return (lowest?.SolarTotal ?? 0, highest?.SolarTotal ?? 0);
        }

        public async Task<TenSecondMetric> FirstOrDefaultAsync(Func<IQueryable<TenSecondMetric>, IQueryable<TenSecondMetric>> operation)
        {
            var query = operation(dbContext.TenSecondMetrics);
            return await query.FirstOrDefaultAsync();
        }
    }
}