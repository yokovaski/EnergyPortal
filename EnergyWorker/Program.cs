using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DatabaseInterface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace EnergyWorker
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.RollingFile("logs/worker-{Date}.log", LogEventLevel.Information, fileSizeLimitBytes: 10000000)
                .CreateLogger();
        
            try
            {
                Log.Information("Starting up");
                var host = CreateHostBuilder(args).Build();
                await ApplyMigrationsWithRetry(host);
                await host.RunAsync();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application start-up failed");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddDbContext<ApplicationDbContext>(options =>
                    {
                        var connectionString = hostContext.Configuration.GetConnectionString("DefaultConnection");
                        options
                            .UseNpgsql(connectionString,
                                assembly => assembly.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName))
                            .UseSnakeCaseNamingConvention();
                    });
                    services.AddHostedService<Worker>();
                });

        private static async Task ApplyMigrationsWithRetry(IHost host)
        {
            const int maxAttempts = 20;

            for (var attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    using var scope = host.Services.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    await dbContext.Database.MigrateAsync();
                    Log.Information("Database migration completed");
                    return;
                }
                catch (Exception ex)
                {
                    if (attempt == maxAttempts)
                    {
                        throw;
                    }

                    Log.Warning(ex, "Database migration attempt {Attempt} failed; retrying", attempt);
                    await Task.Delay(TimeSpan.FromSeconds(3));
                }
            }
        }
    }
}