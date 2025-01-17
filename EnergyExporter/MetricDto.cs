using System.Text.Json;
using DatabaseInterface.Entities;

namespace EnergyExporter;

public class MetricDto : IMetric
{
    public long Id { get; set; }
    public long RaspberryPiId { get; set; }
    public int Mode { get; set; }
    public int UsageNow { get; set; }
    public int RedeliveryNow { get; set; }
    public int SolarNow { get; set; }
    public long UsageTotalHigh { get; set; }
    public long RedeliveryTotalHigh { get; set; }
    public long UsageTotalLow { get; set; }
    public long RedeliveryTotalLow { get; set; }
    public long SolarTotal { get; set; }
    public int UsageGasNow { get; set; }
    public long UsageGasTotal { get; set; }
    public DateTime Created { get; set; }
    public DateTime Updated { get; set; }

    public static async Task<List<T>?> FromFile<T>(string filePath, CancellationToken cancellationToken) where T : IMetric, new()
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("File not found", filePath);
        }
        
        var content = await File.ReadAllTextAsync(filePath, cancellationToken);

        if (!typeof(T).GetInterfaces().Contains(typeof(IMetric)))
        {
            throw new InvalidOperationException("Type does not implement IMetric");
        }
        
        var metrics = JsonSerializer.Deserialize<List<MetricDto>>(content);
        return metrics
            ?.Select(m => new T
            {
                Id = m.Id,
                RaspberryPiId = m.RaspberryPiId,
                Mode = m.Mode,
                UsageNow = m.UsageNow,
                RedeliveryNow = m.RedeliveryNow,
                SolarNow = m.SolarNow,
                UsageTotalHigh = m.UsageTotalHigh,
                RedeliveryTotalHigh = m.RedeliveryTotalHigh,
                UsageTotalLow = m.UsageTotalLow,
                RedeliveryTotalLow = m.RedeliveryTotalLow,
                SolarTotal = m.SolarTotal,
                UsageGasNow = m.UsageGasNow,
                UsageGasTotal = m.UsageGasTotal,
                Created = m.Created,
                Updated = m.Updated
            })
            .ToList();
    }
}