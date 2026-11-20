using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using DatabaseInterface;
using DatabaseInterface.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PureHDF;

namespace EnergyWorker
{
    /// <summary>
    /// Stores the hourly air temperature of every KNMI station from the Open Data API
    /// (dataset 10-minute-in-situ-meteorological-observations, one NetCDF file per 10 minutes, all stations).
    /// Only the file on the whole hour is fetched: 1 API request per hour, well inside the quota of 1000/hour.
    /// </summary>
    public class TemperatureWorker(IServiceProvider serviceProvider, IConfiguration configuration, ILogger<TemperatureWorker> logger)
        : BackgroundService
    {
        private const string FilesUrl =
            "https://api.dataplatform.knmi.nl/open-data/v1/datasets/10-minute-in-situ-meteorological-observations/versions/1.0/files/";

        private static readonly TimeSpan Interval = TimeSpan.FromMinutes(10);
        private static readonly TimeSpan Backfill = TimeSpan.FromDays(7);
        private const int MaxFilesPerRun = 50;

        private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromMinutes(1) };

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var apiKey = configuration["Knmi:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                logger.LogWarning("Knmi:ApiKey is not set; outdoor temperature is not collected");
                return;
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await RunOnceAsync(apiKey, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (HttpRequestException e) when (e.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    // Stored hours are kept; the next run continues where this one stopped
                    logger.LogWarning("KNMI rate limit reached; retrying in {Interval}", Interval);
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Fetching KNMI temperatures failed");
                }

                try
                {
                    await Task.Delay(Interval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }

        internal async Task RunOnceAsync(string apiKey, CancellationToken cancellationToken)
        {
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var now = DateTime.UtcNow;
            var last = await dbContext.Temperatures.MaxAsync(t => (DateTime?)t.Created, cancellationToken);
            var hour = FloorHour(last.HasValue && last.Value > now - Backfill ? last.Value.AddHours(1) : now - Backfill);

            for (var files = 0; hour <= now && files < MaxFilesPerRun; hour = hour.AddHours(1), files++)
            {
                var file = await DownloadAsync(apiKey, $"KMDS__OPER_P___10M_OBS_L2_{hour:yyyyMMddHHmm}.nc", cancellationToken);
                if (file == null)
                {
                    // Recent: not published yet, the next run tries again. Older: a gap at KNMI, skip it.
                    if (now - hour < TimeSpan.FromHours(3))
                        break;
                    continue;
                }

                var (stations, values) = Parse(file);
                await SaveAsync(dbContext, hour, stations, values, cancellationToken);
            }
        }

        private static DateTime FloorHour(DateTime t) => new(t.Year, t.Month, t.Day, t.Hour, 0, 0, DateTimeKind.Utc);

        private record DownloadUrl(string TemporaryDownloadUrl);

        private async Task<Stream> DownloadAsync(string apiKey, string fileName, CancellationToken cancellationToken)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, FilesUrl + fileName + "/url");
            request.Headers.TryAddWithoutValidation("Authorization", apiKey);
            using var response = await Http.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;
            response.EnsureSuccessStatusCode();

            var url = (await response.Content.ReadFromJsonAsync<DownloadUrl>(cancellationToken)).TemporaryDownloadUrl;
            // The download itself is a pre-signed URL and does not count against the quota
            var bytes = await Http.GetByteArrayAsync(url, cancellationToken);
            return new MemoryStream(bytes);
        }

        /// <summary>Stations and their air temperature (°C, NaN when not measured) from a KNMI 10-minute NetCDF file.</summary>
        internal static (List<WeatherStation> Stations, double[] Temperatures) Parse(Stream stream)
        {
            using var file = H5File.Open(stream, false);
            var ids = file.Dataset("station").Read<string[]>();
            var names = file.Dataset("stationname").Read<string[]>();
            var lat = file.Dataset("lat").Read<double[]>();
            var lon = file.Dataset("lon").Read<double[]>();
            // Dimensions (station, time) with a single time step
            var ta = file.Dataset("ta").Read<double[]>();

            var stations = ids.Select((id, i) => new WeatherStation
            {
                Id = id.Trim(), Name = names[i].Trim(), Latitude = lat[i], Longitude = lon[i]
            }).ToList();
            return (stations, ta);
        }

        private async Task SaveAsync(ApplicationDbContext dbContext, DateTime hour, List<WeatherStation> stations, double[] values,
            CancellationToken cancellationToken)
        {
            var known = await dbContext.WeatherStations.ToDictionaryAsync(s => s.Id, cancellationToken);
            var count = 0;

            for (var i = 0; i < stations.Count; i++)
            {
                // Fill value (NaN or the NetCDF default 9.97e36) means the station does not measure temperature
                if (double.IsNaN(values[i]) || Math.Abs(values[i]) > 100)
                    continue;

                var station = stations[i];
                if (known.TryGetValue(station.Id, out var existing))
                {
                    existing.Name = station.Name;
                    existing.Latitude = station.Latitude;
                    existing.Longitude = station.Longitude;
                }
                else
                {
                    dbContext.WeatherStations.Add(station);
                    known[station.Id] = station;
                }

                dbContext.Temperatures.Add(new Temperature { StationId = station.Id, Created = hour, Value = Math.Round(values[i], 1) });
                count++;
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Stored {Count} KNMI temperatures for {Hour:u}", count, hour);
        }
    }
}
