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
        private readonly IServiceScopeFactory serviceScopeFactory;
        private readonly ILogger<Worker> logger;

        private DateTime lastMinuteUpdate = DateTime.MinValue;
        private DateTime lastHourUpdate = DateTime.MinValue;
        private DateTime lastDayUpdate = DateTime.MinValue;

        public Worker(ILogger<Worker> logger, IServiceScopeFactory serviceScopeFactory)
        {
            this.serviceScopeFactory = serviceScopeFactory;
            this.logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.WhenAll(WriteMinuteData(), WriteHourData());
                
                await Task.Delay(10000, stoppingToken);
            }
        }

        private Task WriteMinuteData()
        {
            return Task.Run(() =>
            {
                var changes = false;
                    
                logger.LogInformation($"Running at {DateTime.Now}");

                using var scope = serviceScopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                var raspberryPis = dbContext.RaspberryPis.ToList();

                foreach (var raspberryPi in raspberryPis)
                {
                    var lastMinuteMetric = dbContext.MinuteMetrics
                        .Where(r => r.RaspberryPiId.Equals(raspberryPi.Id))
                        .OrderBy(r => r.Created)
                        .LastOrDefault();

                    var metricsQuery = dbContext.TenSecondMetrics
                        .Where(t => t.RaspberryPiId.Equals(raspberryPi.Id));

                    if (lastMinuteMetric != null)
                    {
                        var last = lastMinuteMetric.Created;
                        var timestamp = new DateTime(last.Year, last.Month, last.Day, last.Hour, last.Minute, 0);
                        timestamp = timestamp.AddMinutes(1);
                        metricsQuery = metricsQuery.Where(t => t.Created >= timestamp);
                    }

                    var utcNow = DateTime.UtcNow;
                    var to = new DateTime(utcNow.Year, utcNow.Month, utcNow.Day, utcNow.Hour, utcNow.Minute, 0);

                    var metrics = metricsQuery
                        .Where(t => t.Created < to)
                        .ToList();
                        
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
                            RaspberryPi = raspberryPi,
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
                        
                    dbContext.MinuteMetrics.AddRange(minuteMetrics);
                }

                if (!changes)
                    return;
                    
                dbContext.SaveChanges();
                logger.LogInformation("Wrote minutes");
            });
        }
        
        private Task WriteHourData()
        {
            return Task.Run(() => 
            {
                var changes = false;

                using var scope = serviceScopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                var raspberryPis = dbContext.RaspberryPis.ToList();

                foreach (var raspberryPi in raspberryPis)
                {
                    var lastHourData = dbContext.HourMetrics
                        .Where(r => r.RaspberryPiId.Equals(raspberryPi.Id))
                        .OrderBy(r => r.Created)
                        .LastOrDefault();

                    var metricsQuery = dbContext.MinuteMetrics
                        .Where(t => t.RaspberryPiId.Equals(raspberryPi.Id));

                    if (lastHourData != null)
                    {
                        var last = lastHourData.Created;
                        var timestamp = new DateTime(last.Year, last.Month, last.Day, last.Hour, 0, 0);
                        timestamp = timestamp.AddHours(1);
                        metricsQuery = metricsQuery.Where(t => t.Created >= timestamp);
                    }

                    var utcNow = DateTime.UtcNow;
                    var to = new DateTime(utcNow.Year, utcNow.Month, utcNow.Day, utcNow.Hour, 0, 0);

                    var metrics = metricsQuery
                        .Where(t => t.Created < to)
                        .ToList();
                        
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
                            RaspberryPi = raspberryPi,
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
                        
                    dbContext.HourMetrics.AddRange(hourMetrics);
                }

                if (!changes)
                    return;
                
                dbContext.SaveChanges();
                logger.LogInformation("Wrote hours");
            });
        }
    }
}