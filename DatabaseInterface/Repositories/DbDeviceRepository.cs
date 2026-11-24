using System.Collections.Generic;
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
        
        public async Task<Dictionary<long, long>> StoreDevices(List<RaspberryPi> devices)
        {
            var mapping = new Dictionary<long, long>();
            
            foreach (var device in devices)
            {
                // Check if the device already exists
                var existingDevice = await dbContext.RaspberryPis.FirstOrDefaultAsync(u => u.UserId == device.UserId);

                if (existingDevice != null)
                {
                    mapping.Add(device.Id, existingDevice.Id);
                    continue;
                }

                var originalId = device.Id;
                device.Id = 0;
                dbContext.RaspberryPis.Add(device);
                await dbContext.SaveChangesAsync();
                
                existingDevice = await dbContext.RaspberryPis.FirstOrDefaultAsync(u => u.UserId == device.UserId);
                mapping.Add(originalId, existingDevice.Id);
            }

            return mapping;
        }
    }
}