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
        public async Task<IActionResult> GetTotalToday()
        {
            var user = await userManager.GetUserAsync(User);

            if (user == null)
                return BadRequest();

            var settings = await settingsRepository.GetSettingsAsync(user.Id) ?? new Settings();

            var raspberryPiId = await dbContext.Users.Where(u => u.Id.Equals(user.Id))
                .Select(u => u.RaspberryPi.Id)
                .FirstOrDefaultAsync();

            var now = DateTime.UtcNow.Date;
            var midnight = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Unspecified);
            var midnightUtc = midnight.ConvertToUtc(settings.TimeZoneId);
            
            var firstToday = await tenSecondMetricRepository
                .FirstOrDefaultAsync(q => q
                    .Where(t => t.RaspberryPiId == raspberryPiId)
                    .Where(t => t.Created > midnightUtc));

            var lastToday = await tenSecondMetricRepository
                .FirstOrDefaultAsync(q => q
                    .Where(t => t.RaspberryPiId == raspberryPiId)
                    .Where(t => t.Created > midnightUtc)
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

            var list = new List<IMetric>
            {
                firstToday,
                lastToday
            };

            var (minSolar, maxSolar) = await tenSecondMetricRepository
                .GetMinAndMaxSolarToday(raspberryPiId, settings.TimeZoneId);

            var difference = list
                .SelectWithPrevious((previous, current) => new
                {
                    UsageTotalHigh = DifferenceInKiloWatt(current.UsageTotalHigh, previous.UsageTotalHigh),
                    UsageTotalLow = DifferenceInKiloWatt(current.UsageTotalLow, previous.UsageTotalLow),
                    RedeliveryTotalHigh = DifferenceInKiloWatt(current.RedeliveryTotalHigh, previous.RedeliveryTotalHigh),
                    RedeliveryTotalLow = DifferenceInKiloWatt(current.RedeliveryTotalLow, previous.RedeliveryTotalLow),
                    TotalSolar = DifferenceInKiloWatt(maxSolar, minSolar),
                    TotalGas = DifferenceInKiloWatt(current.UsageGasTotal, previous.UsageGasTotal),
                })
                .Last();

            return Ok(difference);
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
                NotFound("Could not find metrics");

            return Ok(lastMetric);
        }

        public async Task<long?> GetRaspberryPiId(ApplicationUser applicationUser = null)
        {
            var user = applicationUser ?? await userManager.GetUserAsync(User);

            if (user == null)
                return null;

            return await dbContext.Users.Where(u => u.Id.Equals(user.Id))
                .Select(u => u.RaspberryPi.Id)
                .FirstOrDefaultAsync();
        }
    }
}