using System.Threading.Tasks;
using DatabaseInterface.Entities;
using Microsoft.EntityFrameworkCore;

namespace DatabaseInterface.Repositories
{
    public class DbDeviceRepository
    {
        private readonly ApplicationDbContext dbContext;

        public DbDeviceRepository(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<RaspberryPi> GetDevice(string deviceKey)
        {
            return await dbContext.RaspberryPis.FirstOrDefaultAsync(r => r.RpiKey.Equals(deviceKey));
        }
    }
}