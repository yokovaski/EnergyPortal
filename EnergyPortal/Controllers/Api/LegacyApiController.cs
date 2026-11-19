using System.Globalization;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DatabaseInterface.Entities;
using DatabaseInterface.Repositories;
using EnergyPortal.Models.Legacy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace EnergyPortal.Controllers.Api
{
    [Route("v2/energy")]
    public class LegacyApiController : Controller
    {
        private readonly ILogger<LegacyApiController> logger;
        private readonly DbTenSecondMetricRepository dbTenSecondMetricRepository;
        private readonly DbDeviceRepository dbDeviceRepository;

        public LegacyApiController(
            ILogger<LegacyApiController> logger, 
            DbTenSecondMetricRepository dbTenSecondMetricRepository,
            DbDeviceRepository dbDeviceRepository)
        {
            this.logger = logger;
            this.dbTenSecondMetricRepository = dbTenSecondMetricRepository;
            this.dbDeviceRepository = dbDeviceRepository;
        }
        
        [HttpPost]
        public async Task<IActionResult> StoreEnergy([FromBody] LegacyModel legacyModel)
        {
            var raspberryPi = await dbDeviceRepository.GetDevice(legacyModel.RpiKey);

            if (raspberryPi == null)
                return Unauthorized();

            var tenSecondMetrics = legacyModel.Data
                .Select(metric => new TenSecondMetric
                {
                    UsageNow = (int) double.Parse(metric.UsageNow, CultureInfo.InvariantCulture),
                    RedeliveryNow = (int) double.Parse(metric.RedeliveryNow, CultureInfo.InvariantCulture),
                    SolarNow = metric.SolarNow,
                    UsageGasNow = (int) double.Parse(metric.UsageGasNow, CultureInfo.InvariantCulture),
                    UsageTotalHigh = (long) double.Parse(metric.UsageTotalHigh, CultureInfo.InvariantCulture),
                    RedeliveryTotalHigh = (long) double.Parse(metric.RedeliveryTotalHigh, CultureInfo.InvariantCulture),
                    UsageTotalLow = (long) double.Parse(metric.UsageTotalLow, CultureInfo.InvariantCulture),
                    RedeliveryTotalLow = (long) double.Parse(metric.RedeliveryTotalLow, CultureInfo.InvariantCulture),
                    UsageGasTotal = (long) double.Parse(metric.UsageGasTotal, CultureInfo.InvariantCulture),
                    SolarTotal = metric.SolarTotal,
                    Mode = int.Parse(metric.Mode, CultureInfo.InvariantCulture),
                    Created = GetDateTimeFromUnixTimeStamp(metric.UnixTimeStamp),
                    RaspberryPi = raspberryPi
                })
                .ToList();

            await dbTenSecondMetricRepository.StoreMetrics(tenSecondMetrics);

            return Created("", "");
        }

        private static DateTime GetDateTimeFromUnixTimeStamp(int unixTimeStamp)
        {
            var epoch = new DateTime(1970,1,1,0,0,0,0,System.DateTimeKind.Utc);
            return epoch.AddSeconds(unixTimeStamp).ToUniversalTime();
        }
    }
}