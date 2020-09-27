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
                    UsageNow = (int) double.Parse(metric.UsageNow),
                    RedeliveryNow = (int) double.Parse(metric.RedeliveryNow),
                    SolarNow = metric.SolarNow,
                    UsageGasNow = (int) double.Parse(metric.UsageGasNow),
                    UsageTotalHigh = (long) double.Parse(metric.UsageTotalHigh),
                    RedeliveryTotalHigh = (long) double.Parse(metric.RedeliveryTotalHigh),
                    UsageTotalLow = (long) double.Parse(metric.UsageTotalLow),
                    RedeliveryTotalLow = (long) double.Parse(metric.RedeliveryTotalLow),
                    UsageGasTotal = (long) double.Parse(metric.UsageGasTotal),
                    SolarTotal = metric.SolarTotal,
                    Mode = int.Parse(metric.Mode),
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