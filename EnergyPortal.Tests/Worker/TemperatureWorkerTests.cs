using EnergyWorker;

namespace EnergyPortal.Tests.Worker;

public class TemperatureWorkerTests
{
    [Fact]
    public void Parses_stations_and_air_temperature_from_knmi_file()
    {
        // Real file from dataset 10-minute-in-situ-meteorological-observations, 2026-11-20 06:00 UTC
        using var stream = File.OpenRead(Path.Combine(AppContext.BaseDirectory, "Worker", "knmi-10min-sample.nc"));

        var (stations, temperatures) = TemperatureWorker.Parse(stream);

        Assert.Equal(stations.Count, temperatures.Length);
        var deBilt = stations.FindIndex(s => s.Id == "06260");
        Assert.True(deBilt >= 0, string.Join(", ", stations.Select(s => s.Id)));
        Assert.Contains("BILT", stations[deBilt].Name.ToUpperInvariant());
        Assert.InRange(stations[deBilt].Latitude, 52, 52.2);
        Assert.InRange(stations[deBilt].Longitude, 5.1, 5.3);
        Assert.InRange(temperatures[deBilt], -30, 40);
    }
}
