using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DatabaseInterface.Entities;
using DatabaseInterface.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EnergyPortal.Controllers.WebApi
{
    [Authorize]
    [Route("webapi/v3/settings")]
    public class SettingsWebApiController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly DbSettingsRepository dbSettingsRepository;

        public SettingsWebApiController(
            UserManager<ApplicationUser> userManager,
            DbSettingsRepository dbSettingsRepository)
        {
            this.userManager = userManager;
            this.dbSettingsRepository = dbSettingsRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetSettings()
        {
            var user = await userManager.GetUserAsync(User);

            if (user == null)
                return NotFound("User could not be found");

            var settings = await dbSettingsRepository.GetSettingsAsync(user.Id);
            return Ok(settings);
        }

        [HttpPut]
        public async Task<IActionResult> SetSettings([FromBody] Settings settings)
        {
            var user = await userManager.GetUserAsync(User);

            if (user == null)
                return NotFound("User could not be found");

            settings.UserId = user.Id;
            await dbSettingsRepository.SetSettingsAsync(settings);

            return Ok();
        }

        /// <summary>
        /// Method to get all the possible timezones in the world
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("time-zones")]
        public IActionResult GetTimeZones()
        {
            var timeZones = TimeZoneConverter.TZConvert.KnownIanaTimeZoneNames;
            var timeZoneList = new List<object>();

            foreach (var timeZone in timeZones)
            {
                timeZoneList.Add(new
                {
                    Id = timeZone,
                    DisplayName = timeZone
                });
            }
            
            return Ok(new
            {
                timeZones = timeZoneList
            });
        }
    }
}