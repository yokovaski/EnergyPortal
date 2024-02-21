using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DatabaseInterface;
using DatabaseInterface.Entities;
using DatabaseInterface.Extensions;
using DatabaseInterface.Repositories;
using EnergyPortal.Extensions;
using EnergyPortal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace EnergyPortal.Controllers.WebApi
{
    [Authorize]
    [Route("webapi/v3/metrics")]
    public class MetricsWebApiController : Controller
    {
        private readonly ApplicationDbContext dbContext;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly DbTenSecondMetricRepository tenSecondMetricRepository;
        private readonly DbMinuteMetricRepository minuteMetricRepository;
        private readonly DbHourMetricRepository hourMetricRepository;
        private readonly DbSettingsRepository dbSettingsRepository;
        private readonly ILogger<MetricsWebApiController> logger;

        public MetricsWebApiController(
            ApplicationDbContext dbContext, 
            UserManager<ApplicationUser> userManager,
            DbTenSecondMetricRepository tenSecondMetricRepository,
            DbMinuteMetricRepository minuteMetricRepository, 
            DbHourMetricRepository hourMetricRepository,
            DbSettingsRepository dbSettingsRepository,
            ILogger<MetricsWebApiController> logger)
        {
            this.dbContext = dbContext;
            this.userManager = userManager;
            this.tenSecondMetricRepository = tenSecondMetricRepository;
            this.minuteMetricRepository = minuteMetricRepository;
            this.hourMetricRepository = hourMetricRepository;
            this.dbSettingsRepository = dbSettingsRepository;
            this.logger = logger;
        }
        
        [HttpGet("{timeGroup}")]
        public Task<IActionResult> GetMetrics(
            [FromQuery] DateTime from, 
            [FromQuery] DateTime? to = null,
            [FromRoute] TimeGroup timeGroup = TimeGroup.None)
        {
            from = from.ToUniversalTime();
            to = to?.ToUniversalTime();
            
            switch (timeGroup)
            {
                case TimeGroup.None:
                case TimeGroup.TenSeconds:
                    return GetMetrics(tenSecondMetricRepository, from, to, timeGroup);
                case TimeGroup.Minutes:
                    return GetMetrics(minuteMetricRepository, from, to, timeGroup);
                case TimeGroup.Hours:
                case TimeGroup.Days:
                case TimeGroup.Weeks:
                case TimeGroup.Months:
                case TimeGroup.Years:
                    return GetMetrics(hourMetricRepository, from, to, timeGroup);
                default:
                    throw new ArgumentOutOfRangeException(nameof(timeGroup), timeGroup, null);
            }
        }

        private bool TimeSpanTooLarge(DateTime start, DateTime? end, TimeGroup timeGroup)
        {
            var timeSpan = (end ?? DateTime.UtcNow) - start;
            
            switch (timeGroup)
            {
                case TimeGroup.None:
                case TimeGroup.TenSeconds:
                    return timeSpan > TimeSpan.FromHours(3);
                case TimeGroup.Minutes:
                    return timeSpan > TimeSpan.FromHours(12);
                case TimeGroup.Hours:
                    return timeSpan > TimeSpan.FromDays(30);
                case TimeGroup.Days:
                    return timeSpan > TimeSpan.FromDays(700);
                case TimeGroup.Weeks:
                case TimeGroup.Months:
                case TimeGroup.Years:
                    return false;
                default:
                    throw new ArgumentOutOfRangeException(nameof(timeGroup), timeGroup, null);
            }
        }
        
        private async Task<IActionResult> GetMetrics(
            IMetricRepository metricRepository, 
            DateTime start, 
            DateTime? end = null,
            TimeGroup timeGroup = TimeGroup.None)
        {
            if (TimeSpanTooLarge(start, end, timeGroup))
                return BadRequest("Time range is too large for the current resolution");
            
            if (end.HasValue && end.Value < start)
                return BadRequest("Time range is negative (To is smaller than From)");
            
            var user = await userManager.GetUserAsync(User);
            var raspberryPiId = await GetRaspberryPiId(user);

            if (!raspberryPiId.HasValue)
                return NotFound("Either the user does not exist or the user does not have a raspberry pi");

            var settings = await dbSettingsRepository.GetSettingsAsync(user.Id) ?? new Settings();

            var dbMetrics = (await metricRepository.GetMetrics(raspberryPiId.Value, start, end, settings.TimeZoneId))
                .ToList();

            if (!dbMetrics.Any())
                return Ok(new
                {
                    QueryTimestamp = DateTime.UtcNow.AddSeconds(1).ToString("o"),
                    Timestamps = new string[0],
                    Usage = new int[0],
                    Solar = new int[0],
                    Redelivery = new int[0],
                    Intake = new int[0],
                });

            var format = "";
            List<OutMetricModel> metrics;

            switch (timeGroup)
            {
                case TimeGroup.TenSeconds:
                case TimeGroup.Minutes:
                    format = "HH:mm:ss";

                    metrics = dbMetrics
                        .Select(d => new OutMetricModel
                        {
                            DateTime = d.Created,
                            Gas = d.UsageGasNow,
                            Intake = d.UsageNow,
                            Redelivery = d.RedeliveryNow,
                            Solar = d.SolarNow,
                            Usage = d.UsageNow + d.SolarNow - d.RedeliveryNow
                        })
                        .ToList();
                    break;
                case TimeGroup.Hours:
                    format = "MM-dd HH:mm";
                    
                    metrics = dbMetrics
                        .Select(d => new OutMetricModel
                        {
                            DateTime = d.Created,
                            Gas = d.UsageGasNow,
                            Intake = d.UsageNow,
                            Redelivery = d.RedeliveryNow,
                            Solar = d.SolarNow,
                            Usage = d.UsageNow + d.SolarNow - d.RedeliveryNow
                        })
                        .ToList();
                    break;
                case TimeGroup.Days:
                    metrics = dbMetrics
                        .GroupBy(m => m.Created.Date)
                        .Select(m => new
                        {
                            First = m.First(),
                            Last = m.Last(),
                            SolarTotalMin = m.Any(a => a.SolarTotal > 0) ? m.Where(a => a.SolarTotal > 0).Min(a => a.SolarTotal) : 0,
                            SolarTotalMax = m.Any(a => a.SolarTotal > 0) ? m.Where(a => a.SolarTotal > 0).Max(a => a.SolarTotal) : 0
                        })
                        .SelectWithNextOrDefault((c, n) =>
                        {
                            var current = c.First;
                            var next = n?.First ?? c.Last;
                            current.SolarTotal = c.SolarTotalMin > 0 ? c.SolarTotalMin : n?.SolarTotalMin ?? 0;
                            next.SolarTotal = n?.SolarTotalMin > 0 ? n.SolarTotalMin : c.SolarTotalMax;
                            
                            return current.ToOutMetricModel(next, settings);
                        })
                        .ToList();
                    
                    format = user.Settings.ShowDayName ? "yyyy-MM-dd (dddd)" : "yyyy-MM-dd";
                    break;
                // case TimeGroup.Weeks:
                    // break;
                case TimeGroup.Months:
                    metrics = dbMetrics
                        .GroupBy(m => new
                        {
                            m.Created.Date.Year,
                            m.Created.Month
                        })
                        .Select(m => new
                        {
                            First = m.First(),
                            Last = m.Last(),
                            SolarTotalMin = m.Any(a => a.SolarTotal > 0) ? m.Where(a => a.SolarTotal > 0).Min(a => a.SolarTotal) : 0,
                            SolarTotalMax = m.Any(a => a.SolarTotal > 0) ? m.Where(a => a.SolarTotal > 0).Max(a => a.SolarTotal) : 0
                        })
                        .SelectWithNextOrDefault((c, n) =>
                        {
                            var current = c.First;
                            var next = n?.First ?? c.Last;
                            current.SolarTotal = c.SolarTotalMin > 0 ? c.SolarTotalMin : n?.SolarTotalMin ?? 0;
                            next.SolarTotal = n?.SolarTotalMin > 0 ? n.SolarTotalMin : c.SolarTotalMax;
                            
                            return current.ToOutMetricModel(next, settings);
                        })
                        .ToList();

                    format = "yyyy-MM";
                    break;
                case TimeGroup.Years:
                    metrics = dbMetrics
                        .GroupBy(m => new
                        {
                            m.Created.Date.Year
                        })
                        .Select(m => new
                        {
                            First = m.First(),
                            Last = m.Last(),
                            SolarTotalMin = m.Any(a => a.SolarTotal > 0) ? m.Where(a => a.SolarTotal > 0).Min(a => a.SolarTotal) : 0,
                            SolarTotalMax = m.Any(a => a.SolarTotal > 0) ? m.Where(a => a.SolarTotal > 0).Max(a => a.SolarTotal) : 0
                        })
                        .SelectWithNextOrDefault((c, n) =>
                        {
                            var current = c.First;
                            var next = n?.First ?? c.Last;
                            current.SolarTotal = c.SolarTotalMin > 0 ? c.SolarTotalMin : n?.SolarTotalMin ?? 0;
                            next.SolarTotal = n?.SolarTotalMin > 0 ? n.SolarTotalMin : c.SolarTotalMax;
                            
                            return current.ToOutMetricModel(next, settings);
                        })
                        .ToList();

                    format = "yyyy";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(timeGroup), timeGroup, null);
            }
            
            var cultureInfo = new System.Globalization.CultureInfo("nl-NL");
            var timestamps = metrics.Select(m => m.DateTime.ToString(format, cultureInfo)).ToList();
            var usageList = metrics.Select(m => new object[] { EpochTime.GetIntDate(m.DateTime) * 1000, m.Usage.DivideByThousand() }).ToList();
            var intakeList = metrics.Select(m => new object[] { EpochTime.GetIntDate(m.DateTime) * 1000, m.Intake.DivideByThousand() }).ToList();
            var solarList = metrics.Select(m => new object[] { EpochTime.GetIntDate(m.DateTime) * 1000, m.Solar.DivideByThousand() }).ToList();
            var redeliveryList = metrics.Select(m => new object[] { EpochTime.GetIntDate(m.DateTime) * 1000, m.Redelivery.DivideByThousand() }).ToList();
            var gasList = metrics.Select(m => new object[] { EpochTime.GetIntDate(m.DateTime) * 1000, m.Gas.DivideByThousand() }).ToList();
            var usageCosts = metrics.Select(m => m.UsageCost).ToList();
            var intakeCosts = metrics.Select(m => m.IntakeCost).ToList();
            var redeliveryCosts = metrics.Select(m => m.RedeliveryCost).ToList();
            var gasCosts = metrics.Select(m => m.GasCost).ToList();

            var lastDateTime = metrics
                .OrderBy(m => m.DateTime)
                .Select(m => m.DateTime)
                .LastOrDefault();

            lastDateTime = lastDateTime.ConvertToUtc(settings.TimeZoneId);

            return Ok(new
            {
                QueryTimestamp = lastDateTime.AddSeconds(1).ToString("o"),
                Timestamps = timestamps,
                Usage = usageList,
                Solar = solarList,
                Redelivery = redeliveryList,
                Intake = intakeList,
                Gas = gasList,
                UsageCosts = usageCosts,
                IntakeCosts = intakeCosts,
                RedeliveryCosts = redeliveryCosts,
                GasCosts = gasCosts,
                Format = format,
                UserTimeZone = settings.TimeZoneId
            });
        }
        
        private async Task<long?> GetRaspberryPiId(ApplicationUser user = null)
        {
            user ??= await userManager.GetUserAsync(User);

            if (user == null)
                return null;

            return await dbContext.Users.Where(u => u.Id.Equals(user.Id))
                .Select(u => u.RaspberryPi.Id)
                .FirstOrDefaultAsync();
        }
    }
}