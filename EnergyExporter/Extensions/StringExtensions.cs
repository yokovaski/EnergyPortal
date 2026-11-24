using System.Text.Json;
using DatabaseInterface.Entities;

namespace EnergyExporter.Extensions;

public static class StringExtensions
{
    public static async Task<List<T>?> ReadAllTextToData<T>(this string filePath, CancellationToken cancellationToken) where T : new()
    {
        if (typeof(T).GetInterfaces().Contains(typeof(IMetric)))
        {
            if (typeof(T) == typeof(TenSecondMetric))
            {
                return (await MetricDto.FromFile<TenSecondMetric>(filePath, cancellationToken))
                    .Cast<T>()
                    .ToList();
            }
            
            if (typeof(T) == typeof(MinuteMetric))
            {
                return (await MetricDto.FromFile<MinuteMetric>(filePath, cancellationToken))
                    .Cast<T>()
                    .ToList();
            }

            if (typeof(T) == typeof(HourMetric))
            {
                return (await MetricDto.FromFile<HourMetric>(filePath, cancellationToken))
                    .Cast<T>()
                    .ToList();
            }
            
            throw new NotSupportedException($"Unsupported metric type {typeof(T).Name}");
        }
        
        var content = await File.ReadAllTextAsync(filePath, cancellationToken);
        var data = JsonSerializer.Deserialize<List<T>>(content);
        
        
        return data;
    }
}