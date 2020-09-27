using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DatabaseInterface;
using DatabaseInterface.Entities;
using DatabaseInterface.Repositories;
using EnergyPortal.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EnergyPortal.Controllers.Api
{
    [Route("api/v3/energy")]
    [ApiController]
    public class EnergyApiController : Controller
    {
        private readonly DbTenSecondMetricRepository dbTenSecondMetricRepository;
        private readonly ApplicationDbContext dbContext;
        
        public EnergyApiController(
            ApplicationDbContext dbContext,
            DbTenSecondMetricRepository dbTenSecondMetricRepository)
        {
            this.dbTenSecondMetricRepository = dbTenSecondMetricRepository;
            this.dbContext = dbContext;
        }
        
        [HttpPost]
        public async Task<IActionResult> Create(
            [FromHeader] string rpiKey,
            [FromBody] InMetricArrayModel inMetricsCollection)
        {
            var raspberryPi = await dbContext.RaspberryPis.FirstOrDefaultAsync(r => r.RpiKey.Equals(rpiKey));

            if (raspberryPi == null)
                return BadRequest();

            var tenSecondMetrics = inMetricsCollection.Metrics
                .Select(metric => new TenSecondMetric
                {
                    UsageNow = metric.UsageNow,
                    RedeliveryNow = metric.RedeliveryNow,
                    SolarNow = metric.SolarNow,
                    UsageGasNow = metric.UsageGasNow,
                    UsageTotalHigh = metric.UsageTotalHigh,
                    RedeliveryTotalHigh = metric.RedeliveryTotalHigh,
                    UsageTotalLow = metric.UsageTotalLow,
                    RedeliveryTotalLow = metric.RedeliveryTotalLow,
                    UsageGasTotal = metric.UsageGasTotal,
                    SolarTotal = metric.SolarTotal,
                    Mode = metric.Mode,
                    Created = metric.Created,
                    RaspberryPi = raspberryPi
                })
                .ToList();

            await dbTenSecondMetricRepository.StoreMetrics(tenSecondMetrics);

            return Created("", "");
        }
    }
}