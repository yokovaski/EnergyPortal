using DatabaseInterface;
using DatabaseInterface.Repositories;
using EnergyExporter;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MySqlConnector;
using Npgsql;

var builder = Host.CreateApplicationBuilder(args);
builder.Configuration.AddJsonFile("appsettings.json");

builder.Services.Configure<ExporterOptions>(builder.Configuration.GetSection("ExporterOptions"));

builder.Services.AddTransient(x =>
    new MySqlConnection(builder.Configuration.GetConnectionString("MySQL")));

builder.Services.AddDbContext<ApplicationDbContext>((options) =>
{
    var connectionString = builder.Configuration.GetConnectionString("PostgreSQL");
    options
        .UseNpgsql(connectionString,
            assembly => assembly.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName))
        .UseSnakeCaseNamingConvention();
});
builder.Services.AddScoped<DbTenSecondMetricRepository, DbTenSecondMetricRepository>();
builder.Services.AddScoped<DbMinuteMetricRepository, DbMinuteMetricRepository>();
builder.Services.AddScoped<DbHourMetricRepository, DbHourMetricRepository>();
builder.Services.AddScoped<DbDeviceRepository, DbDeviceRepository>();
builder.Services.AddScoped<DbUserRepository, DbUserRepository>();

var exporterOptions = builder.Services.BuildServiceProvider().GetRequiredService<IOptions<ExporterOptions>>().Value;

if (exporterOptions.DatabaseType == ExporterDatabase.MySQL)
{
    builder.Services.AddHostedService<MySqlWorker>();
}
else if (exporterOptions.DatabaseType == ExporterDatabase.PostgreSQL)
{
    builder.Services.AddHostedService<PostgresWorker>();
}

var host = builder.Build();
host.Run();