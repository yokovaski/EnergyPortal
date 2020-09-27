using DatabaseInterface;
using Microsoft.EntityFrameworkCore;

namespace EnergyWorker
{
    public class ApplicationDbContextFactory
    {
        private readonly DbContextOptions<ApplicationDbContext> dbContextOptions;

        public ApplicationDbContextFactory(DbContextOptions<ApplicationDbContext> dbContextOptions)
        {
            this.dbContextOptions = dbContextOptions;
        }

        public ApplicationDbContext CreateDbContext()
        {
            return new ApplicationDbContext(dbContextOptions);
        }
    }
}