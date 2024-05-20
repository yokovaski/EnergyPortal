using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DatabaseInterface;
using DatabaseInterface.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EnergyWorker
{
    public class Worker : BackgroundService
    {
        private readonly IServiceProvider serviceProvider;
        private readonly ILogger<Worker> logger;

        private DateTime lastMinuteUpdate = DateTime.MinValue;
        private DateTime lastHourUpdate = DateTime.MinValue;
        private DateTime lastDayUpdate = DateTime.MinValue;

        public Worker(ILogger<Worker> logger, IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
            this.logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Starting worker");
            
            while (!stoppingToken.IsCancellationRequested)
            {
                logger.LogInformation("Running at {Time}", DateTime.Now);
                
                try
                {
                    List<long> raspberryPiIds;

                    using (var scope = serviceProvider.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();
                        raspberryPiIds = await dbContext.RaspberryPis.Select(r => r.Id).ToListAsync(stoppingToken);
                    }

                    await Task.WhenAll(
                        WriteMinuteData(raspberryPiIds, stoppingToken), 
                        WriteHourData(raspberryPiIds, stoppingToken),
                        DeleteOldMinuteMetrics(raspberryPiIds, stoppingToken),
                        DeleteOldTenSecondMetrics(raspberryPiIds, stoppingToken));
                    
                    await Task.Delay(10000, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // stop gracefully
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Exception encountered while running worker");
                }
            }
            
            logger.LogInformation("Stopping worker");
        }

        private async Task DeleteOldMinuteMetrics(ICollection<long> raspberryPiIds, CancellationToken cancellationToken)
        {
            await Task.Yield();

            const int batchSize = 1000;
            var totalDeleted = 0;
            
            foreach (var raspberryPiId in raspberryPiIds)
            {
                var deleted = 0;
                
                do
                {
                    using var scope = serviceProvider.CreateScope();
                    var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();
                    var threeMonthsAgo = DateTime.UtcNow.AddMonths(-3).Date;
                
                    var oldestHour = await dbContext.HourMetrics
                        .Where(h => h.RaspberryPiId == raspberryPiId)
                        .OrderBy(h => h.Created)
                        .LastOrDefaultAsync(cancellationToken);
                
                    if (oldestHour == null || oldestHour.Created < threeMonthsAgo)
                        continue;

                    var metrics = dbContext.MinuteMetrics
                        .Where(t => t.Created < threeMonthsAgo)
                        .OrderBy(t => t.Created)
                        .Take(batchSize)
                        .ToList();

                    deleted = metrics.Count;
                    
                    if (deleted == 0)
                        continue;

                    dbContext.MinuteMetrics.RemoveRange(metrics);
                    await dbContext.SaveChangesAsync(cancellationToken);
                    totalDeleted += deleted;
                } while (deleted == batchSize);
            }
                
            if (totalDeleted > 0)
                logger.LogInformation($"Removed {totalDeleted} rows of MinuteMetrics");
        }

        private async Task DeleteOldTenSecondMetrics(ICollection<long> raspberryPiIds, CancellationToken cancellationToken)
        {
            await Task.Yield();

            const int batchSize = 1000;
            var totalDeleted = 0;
            
            foreach (var raspberryPiId in raspberryPiIds)
            {
                var deleted = 0;
                
                do
                {
                    using var scope = serviceProvider.CreateScope();
                    var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();
                    var twoWeeksAgo = DateTime.UtcNow.AddDays(-14).Date;
                
                    var oldestHour = await dbContext.HourMetrics
                        .Where(h => h.RaspberryPiId == raspberryPiId)
                        .OrderBy(h => h.Created)
                        .LastOrDefaultAsync(cancellationToken);
                
                    if (oldestHour == null || oldestHour.Created < twoWeeksAgo)
                        continue;

                    var metrics = dbContext.TenSecondMetrics
                        .Where(t => t.Created < twoWeeksAgo)
                        .OrderBy(t => t.Created)
                        .Take(batchSize)
                        .ToList();

                    deleted = metrics.Count;
                    
                    if (deleted == 0)
                        continue;
                
                    dbContext.TenSecondMetrics.RemoveRange(metrics);
                    await dbContext.SaveChangesAsync(cancellationToken);
                    totalDeleted += deleted;
                } while (deleted == batchSize);
            }
            
            if (totalDeleted > 0)
                logger.LogInformation($"Removed {totalDeleted} rows of TenSecondMetrics");
        }

        private async Task WriteMinuteData(ICollection<long> raspberryPiIds, CancellationToken cancellationToken)
        {
            await Task.Yield();
            
            var changes = false;

            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

            foreach (var raspberryPiId in raspberryPiIds)
            {
                var lastMinuteMetric = await dbContext.MinuteMetrics
                    .Where(r => r.RaspberryPiId.Equals(raspberryPiId))
                    .OrderBy(r => r.Created)
                    .LastOrDefaultAsync(cancellationToken);

                var metricsQuery = dbContext.TenSecondMetrics
                    .Where(t => t.RaspberryPiId.Equals(raspberryPiId));

                if (lastMinuteMetric != null)
                {
                    var last = lastMinuteMetric.Created;
                    var timestamp = new DateTime(last.Year, last.Month, last.Day, last.Hour, last.Minute, 0, DateTimeKind.Utc);
                    timestamp = timestamp.AddMinutes(1);
                    metricsQuery = metricsQuery.Where(t => t.Created >= timestamp);
                }

                var utcNow = DateTime.UtcNow;
                var to = new DateTime(utcNow.Year, utcNow.Month, utcNow.Day, utcNow.Hour, utcNow.Minute, 0, DateTimeKind.Utc);

                var metrics = await metricsQuery
                    .Where(t => t.Created < to)
                    .ToListAsync(cancellationToken);
                    
                if (!metrics.Any())
                    continue;

                changes = true;

                var minuteMetrics = metrics.GroupBy(t => new
                    {
                        t.Created.Year,
                        t.Created.Month,
                        t.Created.Day,
                        t.Created.Hour,
                        t.Created.Minute
                    })
                    .Select(t => new MinuteMetric()
                    {
                        Created = new DateTime(t.Key.Year, t.Key.Month, t.Key.Day, t.Key.Hour, t.Key.Minute, 0, DateTimeKind.Utc),
                        Updated = DateTime.UtcNow,
                        RaspberryPiId = raspberryPiId,
                        UsageNow = Convert.ToInt32(Math.Round(t.Average(a => a.UsageNow))),
                        UsageTotalLow = t.Max(a => a.UsageTotalLow),
                        UsageTotalHigh = t.Max(a => a.UsageTotalHigh),
                        RedeliveryNow = Convert.ToInt32(Math.Round(t.Average(a => a.RedeliveryNow))),
                        RedeliveryTotalLow = t.Max(a => a.RedeliveryTotalLow),
                        RedeliveryTotalHigh = t.Max(a => a.RedeliveryTotalHigh),
                        SolarNow = Convert.ToInt32(Math.Round(t.Average(a => a.SolarNow))),
                        SolarTotal = t.Max(a => a.SolarTotal),
                        UsageGasNow = Convert.ToInt32(Math.Round(t.Average(a => a.UsageGasNow))),
                        UsageGasTotal = t.Max(a => a.UsageGasTotal),
                        Mode = (int) t.Max(a => a.Mode)
                    });
                    
                await dbContext.MinuteMetrics.AddRangeAsync(minuteMetrics, cancellationToken);
            }

            if (!changes)
                return;
                
            await dbContext.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Wrote minutes");
        }
        
        private async Task WriteHourData(ICollection<long> raspberryPiIds, CancellationToken cancellationToken)
        {
            await Task.Yield();
            
            var changes = false;

            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

            foreach (var raspberryPiId in raspberryPiIds)
            {
                var lastHourData = await dbContext.HourMetrics
                    .Where(r => r.RaspberryPiId.Equals(raspberryPiId))
                    .OrderBy(r => r.Created)
                    .LastOrDefaultAsync(cancellationToken: cancellationToken);

                var metricsQuery = dbContext.MinuteMetrics
                    .Where(t => t.RaspberryPiId.Equals(raspberryPiId));

                if (lastHourData != null)
                {
                    var last = lastHourData.Created;
                    var timestamp = new DateTime(last.Year, last.Month, last.Day, last.Hour, 0, 0, DateTimeKind.Utc);
                    timestamp = timestamp.AddHours(1);
                    metricsQuery = metricsQuery.Where(t => t.Created >= timestamp);
                }

                var utcNow = DateTime.UtcNow;
                var to = new DateTime(utcNow.Year, utcNow.Month, utcNow.Day, utcNow.Hour, 0, 0, DateTimeKind.Utc);

                var metrics = await metricsQuery
                    .Where(t => t.Created < to)
                    .ToListAsync(cancellationToken: cancellationToken);
                    
                if (!metrics.Any())
                    continue;

                changes = true;

                var hourMetrics = metrics.GroupBy(t => new
                    {
                        t.Created.Year,
                        t.Created.Month,
                        t.Created.Day,
                        t.Created.Hour
                    })
                    .Select(t => new HourMetric()
                    {
                        Created = new DateTime(t.Key.Year, t.Key.Month, t.Key.Day, t.Key.Hour, 0, 0, DateTimeKind.Utc),
                        Updated = DateTime.UtcNow,
                        RaspberryPiId = raspberryPiId,
                        UsageNow = Convert.ToInt32(Math.Round(t.Average(a => a.UsageNow))),
                        UsageTotalLow = t.Max(a => a.UsageTotalLow),
                        UsageTotalHigh = t.Max(a => a.UsageTotalHigh),
                        RedeliveryNow = Convert.ToInt32(Math.Round(t.Average(a => a.RedeliveryNow))),
                        RedeliveryTotalLow = t.Max(a => a.RedeliveryTotalLow),
                        RedeliveryTotalHigh = t.Max(a => a.RedeliveryTotalHigh),
                        SolarNow = Convert.ToInt32(Math.Round(t.Average(a => a.SolarNow))),
                        SolarTotal = t.Max(a => a.SolarTotal),
                        UsageGasNow = Convert.ToInt32(Math.Round(t.Average(a => a.UsageGasNow))),
                        UsageGasTotal = t.Max(a => a.UsageGasTotal),
                        Mode = (int) t.Max(a => a.Mode)
                    });
                    
                await dbContext.HourMetrics.AddRangeAsync(hourMetrics, cancellationToken);
            }

            if (!changes)
                return;
            
            await dbContext.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Wrote hours");
        }
    }
}