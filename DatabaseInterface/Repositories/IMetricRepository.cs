using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DatabaseInterface.Entities;

namespace DatabaseInterface.Repositories
{
    public interface IMetricRepository
    {
        Task StoreMetric(IMetric metric);
        Task StoreMetrics(IEnumerable<IMetric> metric);
        Task<IEnumerable<IMetric>> GetMetrics(long rpiId, DateTime? start = null, DateTime? end = null, string timeZoneId = null);
    }
}