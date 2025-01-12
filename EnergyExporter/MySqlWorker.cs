using System.Text.Json;
using Dapper;
using DatabaseInterface.Entities;
using EnergyExporter.Extensions;
using Microsoft.Extensions.Options;
using MySqlConnector;

namespace EnergyExporter;

public class MySqlWorker(
    ILogger<MySqlWorker> logger,
    IHostApplicationLifetime hostApplicationLifetime,
    MySqlConnection mySqlConnection,
    IOptions<ExporterOptions> exporterOptions
    ) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (exporterOptions.Value.OperationMode != OperationMode.Export)
        {
            throw new NotSupportedException("Only Export operation mode is supported for MySqlWorker");
        }

        await mySqlConnection.OpenAsync(stoppingToken);
        await ExportMetrics<TenSecondMetric>(stoppingToken);
        await ExportMetrics<MinuteMetric>(stoppingToken);
        await ExportMetrics<HourMetric>(stoppingToken);
        await ExportData<RaspberryPi>(stoppingToken);
        await ExportData<ApplicationUser>(stoppingToken);
        
        hostApplicationLifetime.StopApplication();
    }

    private async Task ExportMetrics<T>( CancellationToken stoppingToken) where T : IMetric
    {
        stoppingToken.ThrowIfCancellationRequested();
        var nameOfMetric = typeof(T).Name;
        List<MetricDto> allMetrics = [];
        
        try
        {
            long id = 0;
            var counter = 0;
            
            while(true)
            {
                var result = await mySqlConnection.QueryAsync<MetricDto>(
                    $"select * from {nameOfMetric}s WHERE Id > @id ORDER BY Id LIMIT 10000", new { id = id });
                var metrics = result.ToList();
                
                if (metrics.Count == 0)
                {
                    break;
                }
                
                logger.LogInformation("Fetched {Count} {Metric}s", metrics.Count, nameOfMetric);
                allMetrics.AddRange(metrics);
                id = metrics.Last().Id;

                if (allMetrics.Count <= 100_000)
                {
                    continue;
                }
                
                await WriteData(allMetrics, stoppingToken, counter++);
                allMetrics = [];
            }
            
            if (allMetrics.Count > 0)
            {
                await WriteData(allMetrics, stoppingToken, counter);
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to export {NameOfMetric}", nameOfMetric);
        }
    }

    private async Task WriteData<T>(List<T> data, CancellationToken cancellationToken, int counter = 0)
    {
        var dataName = typeof(T).Name;
        logger.LogInformation("Serializing {Count} {Data}s", data.Count, dataName);
        var serialized = data is List<IMetric> metrics
            ? JsonSerializer.Serialize(metrics.ToMetricDtoList())
            : JsonSerializer.Serialize(data);
        var filename = $"{dataName}_{counter}.json";
                    
        logger.LogInformation("Writing {Count} {Data}s to {Filename}", data.Count, dataName, filename);
        await File.WriteAllTextAsync(filename, serialized, cancellationToken);
        logger.LogInformation("Wrote {Count} {Data}s to {Filename}", data.Count, dataName, filename);
    } 

    private async Task ExportData<T>(CancellationToken cancellationToken)
    {
        var dataName = typeof(T).Name;
        var tableName = $"{dataName}s";

        if (dataName == nameof(ApplicationUser))
        {
            tableName = "AspNetUsers";
        }
        
        try
        {
            var result = await mySqlConnection.QueryAsync<T>(
                $"select * from {tableName} ORDER BY Id");
            var list = result.ToList();
                
            if (list.Count == 0)
            {
                logger.LogInformation("No more {Data} to fetch", dataName);
                return;
            }
                
            logger.LogInformation("Fetched {Count} {Data}", list.Count, dataName);

            await WriteData(list, cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to export {DataName}", dataName);
        }
    }
}