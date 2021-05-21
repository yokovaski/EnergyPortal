using DatabaseInterface.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace EnergyPortal.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddDbRepositories(this IServiceCollection services)
        {
            services.AddScoped<DbTenSecondMetricRepository, DbTenSecondMetricRepository>();
            services.AddScoped<DbMinuteMetricRepository, DbMinuteMetricRepository>();
            services.AddScoped<DbHourMetricRepository, DbHourMetricRepository>();
            services.AddScoped<DbSettingsRepository, DbSettingsRepository>();
            services.AddScoped<DbDeviceRepository, DbDeviceRepository>();
        }
    }
}