using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using DatabaseInterface;
using DatabaseInterface.Entities;
using DatabaseInterface.Extensions;
using DatabaseInterface.Repositories;
using EnergyPortal.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EnergyPortal.Controllers.WebApi
{
    [Authorize]
    [Route("webapi/v3/energy")]
    public class EnergyWebApiController : Controller
    {
        private readonly ApplicationDbContext dbContext;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly DbTenSecondMetricRepository tenSecondMetricRepository;
        private readonly DbSettingsRepository settingsRepository;

        public EnergyWebApiController(
            ApplicationDbContext dbContext, 
            UserManager<ApplicationUser> userManager,
            DbTenSecondMetricRepository tenSecondMetricRepository,
            DbSettingsRepository settingsRepository)
        {
            this.dbContext = dbContext;
            this.userManager = userManager;
            this.tenSecondMetricRepository = tenSecondMetricRepository;
            this.settingsRepository = settingsRepository;
        }

        [HttpGet("total-today")]
        public Task<IActionResult> GetTotalToday() => GetTotalSinceMidnight(daysAgo: 0);

        /// <summary>Yesterday from midnight until 24 hours ago: the comparison for today's totals.</summary>
        [HttpGet("total-yesterday-until-now")]
        public Task<IActionResult> GetTotalYesterdayUntilNow() => GetTotalSinceMidnight(daysAgo: 1);

        private async Task<IActionResult> GetTotalSinceMidnight(int daysAgo)
        {
            var user = await userManager.GetUserAsync(User);

            if (user == null)
                return BadRequest();

            var settings = await settingsRepository.GetSettingsAsync(user.Id) ?? new Settings();

            var raspberryPiId = await GetRaspberryPiId(user);

            if (!raspberryPiId.HasValue)
                return NotFound("The user does not have a raspberry pi");

            // Midnight in the user's time zone, not the UTC date (which is a day off around midnight).
            var untilUtc = DateTime.UtcNow.AddDays(-daysAgo);
            var localMidnight = untilUtc.ConvertToTimeZone(settings.TimeZoneId).Date;
            var fromUtc = DateTime.SpecifyKind(localMidnight.ConvertToUtc(settings.TimeZoneId), DateTimeKind.Utc);

            var firstToday = await tenSecondMetricRepository
                .FirstOrDefaultAsync(q => q
                    .Where(t => t.RaspberryPiId == raspberryPiId)
                    .Where(t => t.Created > fromUtc && t.Created <= untilUtc)
                    .OrderBy(t => t.Created));

            var lastToday = await tenSecondMetricRepository
                .FirstOrDefaultAsync(q => q
                    .Where(t => t.RaspberryPiId == raspberryPiId)
                    .Where(t => t.Created > fromUtc && t.Created <= untilUtc)
                    .OrderByDescending(t => t.Created));

            if (firstToday == null || lastToday == null)
                return Ok(new
                {
                    UsageTotalHigh = (double) 0,
                    UsageTotalLow = (double) 0,
                    RedeliveryTotalHigh = (double) 0,
                    RedeliveryTotalLow = (double) 0,
                    TotalSolar = (double) 0,
                    TotalGas = (double) 0
                });

            var (minSolar, maxSolar) = await tenSecondMetricRepository
                .GetMinAndMaxSolar(raspberryPiId.Value, fromUtc, untilUtc);

            return Ok(new
            {
                UsageTotalHigh = DifferenceInKiloWatt(lastToday.UsageTotalHigh, firstToday.UsageTotalHigh),
                UsageTotalLow = DifferenceInKiloWatt(lastToday.UsageTotalLow, firstToday.UsageTotalLow),
                RedeliveryTotalHigh = DifferenceInKiloWatt(lastToday.RedeliveryTotalHigh, firstToday.RedeliveryTotalHigh),
                RedeliveryTotalLow = DifferenceInKiloWatt(lastToday.RedeliveryTotalLow, firstToday.RedeliveryTotalLow),
                TotalSolar = DifferenceInKiloWatt(maxSolar, minSolar),
                TotalGas = DifferenceInKiloWatt(lastToday.UsageGasTotal, firstToday.UsageGasTotal),
            });
        }

        private double DifferenceInKiloWatt(double newer, double older)
        {
            return Math.Round((newer - older) / 1000.0, 3);
        }

        [HttpGet("totals")]
        public async Task<IActionResult> GetEnergyTotals()
        {
            var raspberryPiId = await GetRaspberryPiId();

            if (!raspberryPiId.HasValue)
                return NotFound("Either the user does not exist or the user does not have a raspberry pi");

            var lastMetric = await dbContext.TenSecondMetrics
                .Where(t => t.RaspberryPiId == raspberryPiId.Value)
                .OrderBy(t => t.Created)
                .LastOrDefaultAsync();

            if (lastMetric == null)
                return NotFound("Could not find metrics");

            // The inverter reports a zero total while asleep or unreachable: show its last known total instead.
            // Limited to two days so users without solar don't scan the whole table on every poll.
            if (lastMetric.SolarTotal == 0)
                lastMetric.SolarTotal = await dbContext.TenSecondMetrics
                    .Where(t => t.RaspberryPiId == raspberryPiId.Value && t.SolarTotal > 0 && t.Created > lastMetric.Created.AddDays(-2))
                    .OrderByDescending(t => t.Created)
                    .Select(t => t.SolarTotal)
                    .FirstOrDefaultAsync();

            return Ok(lastMetric);
        }

        public async Task<long?> GetRaspberryPiId(ApplicationUser applicationUser = null)
        {
            var user = applicationUser ?? await userManager.GetUserAsync(User);

            if (user == null)
                return null;

            return await dbContext.Users.Where(u => u.Id.Equals(user.Id))
                .Select(u => (long?) u.RaspberryPi.Id)
                .FirstOrDefaultAsync();
        }
    }
}