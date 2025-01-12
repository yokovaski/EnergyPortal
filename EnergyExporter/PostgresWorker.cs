using System.Reflection;
using System.Text.Json;
using DatabaseInterface.Entities;
using DatabaseInterface.Repositories;
using Microsoft.Extensions.Options;

namespace EnergyExporter;

public class PostgresWorker(
    ILogger<MySqlWorker> logger,
    IHostApplicationLifetime hostApplicationLifetime,
    DbTenSecondMetricRepository tenSecondMetricRepository,
    DbMinuteMetricRepository minuteMetricRepository,
    DbHourMetricRepository hourMetricRepository,
    DbDeviceRepository deviceRepository,
    DbUserRepository userRepository,
    IOptions<ExporterOptions> exporterOptions
    ) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (exporterOptions.Value.OperationMode != OperationMode.Import)
        {
            throw new NotSupportedException("Only Import operation mode is supported for PostgresWorker");
        }

        await ImportData<ApplicationUser>(new Dictionary<long, long>(), stoppingToken);
        var deviceMapping = await ImportData<RaspberryPi>(new Dictionary<long, long>(), stoppingToken);
        await ImportData<TenSecondMetric>(deviceMapping, stoppingToken);
        await ImportData<MinuteMetric>(deviceMapping, stoppingToken);
        await ImportData<HourMetric>(deviceMapping, stoppingToken);
        
        hostApplicationLifetime.StopApplication();
    }
    
    private async Task<Dictionary<long, long>> ImportData<T>(Dictionary<long, long> deviceMapping,
        CancellationToken stoppingToken)
    {
        stoppingToken.ThrowIfCancellationRequested();
        
        var nameOfData = typeof(T).Name;
        
        // Get path of executing assembly
        var path = Assembly.GetExecutingAssembly().Location;
        var directory = Path.GetDirectoryName(path);

        if (directory == null)
        {
            throw new InvalidOperationException("Failed to get directory of executing assembly");
        }
        
        // Find all files in the current directory that match the metric name
        var files = Directory.GetFiles(directory, $"{nameOfData}_*.json");

        var mapping = new Dictionary<long, long>();
        
        foreach (var file in files)
        {
            // TODO add support for MetricsDto type.
            var content = await File.ReadAllTextAsync(file, stoppingToken);
            var data = JsonSerializer.Deserialize<List<T>>(content);
            
            if (data == null)
            {
                logger.LogWarning("Failed to deserialize {Filename}", file);
                continue;
            }

            var localMapping = await StoreData(data, deviceMapping);

            foreach (var (key, value) in localMapping)
            {
                mapping[key] = value;
            }
            
            logger.LogInformation("Imported {Count} {Data}s from {Filename}", data.Count, nameOfData, file);
        }

        return mapping;
    }

    private async Task<Dictionary<long, long>> StoreData<T>(List<T> data, Dictionary<long, long> deviceMapping)
    {
        var mapping = new Dictionary<long, long>();
        MarkDateTimesAsUtc(data);
        
        switch (typeof(T).Name)
        {
            case nameof(TenSecondMetric):
                if (data is not List<TenSecondMetric> tenSecondMetrics)
                {
                    throw new InvalidOperationException("Failed to cast data to List<TenSecondMetric>");
                }
                
                FixDeviceMapping(tenSecondMetrics, deviceMapping);
                await tenSecondMetricRepository.StoreMetrics(tenSecondMetrics);
                break;
            case nameof(MinuteMetric):
                if (data is not List<TenSecondMetric> minuteMetrics)
                {
                    throw new InvalidOperationException("Failed to cast data to List<TenSecondMetric>");
                }
                
                FixDeviceMapping(minuteMetrics, deviceMapping);
                await minuteMetricRepository.StoreMetrics(minuteMetrics);
                break;
            case nameof(HourMetric):
                if (data is not List<TenSecondMetric> hourMetrics)
                {
                    throw new InvalidOperationException("Failed to cast data to List<TenSecondMetric>");
                }
                
                FixDeviceMapping(hourMetrics, deviceMapping);
                await hourMetricRepository.StoreMetrics(hourMetrics);
                break;
            case nameof(RaspberryPi):
                mapping = await deviceRepository.StoreDevices(data as List<RaspberryPi>);
                break;
            case nameof(ApplicationUser):
                await userRepository.StoreUsers(data as List<ApplicationUser>);
                break;
        }

        return mapping;
    }

    private static void MarkDateTimesAsUtc<T>(IEnumerable<T> list)
    {
        var properties = typeof(T).GetProperties()
            .Where(p => p.PropertyType == typeof(DateTime) || p.PropertyType == typeof(DateTime?))
            .ToList();

        foreach (var entry in list)
        {
            foreach (var property in properties)
            {
                if (property.PropertyType == typeof(DateTime))
                {
                    var value = (DateTime)property.GetValue(entry)!;
                    property.SetValue(entry, DateTime.SpecifyKind(value, DateTimeKind.Utc));
                }
                else if (property.PropertyType == typeof(DateTime?))
                {
                    var value = (DateTime?)property.GetValue(entry);
                    if (value.HasValue)
                    {
                        property.SetValue(entry, DateTime.SpecifyKind(value.Value, DateTimeKind.Utc));
                    }
                }
            }
        }
    }

    private void FixDeviceMapping<T>(List<T> metrics, Dictionary<long, long> mapping) where T : class, IMetric
    {
        var groupedMetrics = metrics
            .GroupBy(m => m.RaspberryPiId)
            .Select(g => new
            {
                RaspberryPiId = g.Key,
                Metrics = g.ToList()
            })
            .ToList();
        
        try
        {
            foreach (var entry in metrics)
            {
                entry.RaspberryPiId = mapping[entry.RaspberryPiId];
            }
        }
        catch (Exception e)
        {
            logger.LogWarning("Failed to fix device mapping for {Type}: {Exception}", typeof(T).Name, e.Message);
            throw;
        }
    }
}