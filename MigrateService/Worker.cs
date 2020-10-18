using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DatabaseInterface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MigrateService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> logger;
        private readonly IServiceScopeFactory serviceScopeFactory;

        public Worker(ILogger<Worker> logger, IServiceScopeFactory serviceScopeFactory)
        {
            this.logger = logger;
            this.serviceScopeFactory = serviceScopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                logger.LogInformation("Started migration worker");

                using var scope = serviceScopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                logger.LogInformation("Starting migration...");
                await dbContext.Database.MigrateAsync(cancellationToken: stoppingToken);
                logger.LogInformation("Finished migration!");
            }
            catch (Exception e)
            {
                logger.LogError("Failed to run migration completely", e);
            }
        }
    }
}