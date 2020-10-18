using DatabaseInterface;
using IdentityServer4.EntityFramework.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EnergyWorker
{
    public class ApplicationDbContextFactory
    {
        private readonly DbContextOptions<ApplicationDbContext> dbContextOptions;
        private readonly IOptions<OperationalStoreOptions> operationalStoreOptions;

        public ApplicationDbContextFactory(
            DbContextOptions<ApplicationDbContext> dbContextOptions,
            IOptions<OperationalStoreOptions> operationalStoreOptions)
        {
            this.dbContextOptions = dbContextOptions;
            this.operationalStoreOptions = operationalStoreOptions;
        }

        public ApplicationDbContext CreateDbContext()
        {
            return new ApplicationDbContext(dbContextOptions, operationalStoreOptions);
        }
    }
}