using System.Linq;
using System.Threading.Tasks;
using DatabaseInterface.Entities;
using Microsoft.EntityFrameworkCore;

namespace DatabaseInterface.Repositories
{
    public class DbSettingsRepository
    {
        private readonly ApplicationDbContext dbContext;

        public DbSettingsRepository(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<Settings> GetSettingsAsync(string userId)
        {
            return await dbContext.Settings.FirstOrDefaultAsync(s => s.UserId.Equals(userId));
        }

        public async Task SetSettingsAsync(Settings settings)
        {
            var dbSettings = await GetSettingsAsync(settings.UserId);

            if (dbSettings == null)
            {
                await dbContext.Settings.AddAsync(settings);
                await dbContext.SaveChangesAsync();
                return;
            }

            dbSettings.SolarSystem = settings.SolarSystem;
            dbSettings.ShowDayName = settings.ShowDayName;
            dbSettings.TimeZoneId = settings.TimeZoneId;
            dbSettings.HighUsagePricePerKwh = settings.HighUsagePricePerKwh;
            dbSettings.LowUsagePricePerKwh = settings.LowUsagePricePerKwh;
            dbSettings.HighRedeliveryPricePerKwh = settings.HighRedeliveryPricePerKwh;
            dbSettings.LowRedeliveryPricePerKwh = settings.LowRedeliveryPricePerKwh;
            dbSettings.GasPrice = settings.GasPrice;
            dbSettings.ElectricityDeliveryPricePerMonth = settings.ElectricityDeliveryPricePerMonth;
            dbSettings.GasDeliveryPricePerMonth = settings.GasDeliveryPricePerMonth;
            
            await dbContext.SaveChangesAsync();
        } 
    }
}