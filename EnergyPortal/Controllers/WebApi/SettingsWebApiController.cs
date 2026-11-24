using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
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

        private readonly DbTemperatureRepository temperatureRepository;
        private readonly IHttpClientFactory httpClientFactory;

        public SettingsWebApiController(
            UserManager<ApplicationUser> userManager,
            DbSettingsRepository dbSettingsRepository,
            DbTemperatureRepository temperatureRepository,
            IHttpClientFactory httpClientFactory)
        {
            this.userManager = userManager;
            this.dbSettingsRepository = dbSettingsRepository;
            this.temperatureRepository = temperatureRepository;
            this.httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public async Task<IActionResult> GetSettings()
        {
            var user = await userManager.GetUserAsync(User);

            if (user == null)
                return NotFound("User could not be found");

            var settings = await dbSettingsRepository.GetSettingsAsync(user.Id);
            if (settings is { Latitude: not null, Longitude: not null })
                settings.WeatherStation = (await temperatureRepository.NearestStationAsync(settings.Latitude.Value, settings.Longitude.Value))?.Name;
            return Ok(settings);
        }

        [HttpPut]
        public async Task<IActionResult> SetSettings([FromBody] Settings settings)
        {
            var user = await userManager.GetUserAsync(User);

            if (user == null)
                return NotFound("User could not be found");

            // Feed-in (teruglevering) tariffs can be negative under some contracts; other prices cannot.
            if (settings.GasPrice < 0 || settings.HighUsagePricePerKwh < 0 || settings.LowUsagePricePerKwh < 0 ||
                settings.ElectricityDeliveryPricePerMonth < 0 || settings.GasDeliveryPricePerMonth < 0)
                return BadRequest("Only feed-in tariffs can be negative");

            // Coordinates come only from the postcode lookup, never from the client
            var postcode = settings.Postcode?.Replace(" ", "").ToUpperInvariant();
            var stored = await dbSettingsRepository.GetSettingsAsync(user.Id);
            settings.Postcode = string.IsNullOrEmpty(postcode) ? null : postcode;
            settings.Latitude = null;
            settings.Longitude = null;

            if (settings.Postcode != null)
            {
                if (!Regex.IsMatch(settings.Postcode, "^[1-9][0-9]{3}[A-Z]{2}$"))
                    return BadRequest("Vul een postcode in zoals 1234 AB");

                if (stored?.Postcode == settings.Postcode && stored.Latitude.HasValue)
                {
                    settings.Latitude = stored.Latitude;
                    settings.Longitude = stored.Longitude;
                }
                else
                {
                    var point = await LookupPostcodeAsync(settings.Postcode);
                    if (point == null)
                        return BadRequest("Deze postcode kennen we niet");
                    (settings.Latitude, settings.Longitude) = point.Value;
                }
            }

            settings.UserId = user.Id;
            await dbSettingsRepository.SetSettingsAsync(settings);

            return Ok();
        }

        private record PdokResponse(PdokResult Response);
        private record PdokResult(List<PdokDoc> Docs);
        private record PdokDoc([property: JsonPropertyName("centroide_ll")] string CentroideLl);

        /// <summary>Latitude/longitude of a Dutch postcode via PDOK Locatieserver (public, no key).</summary>
        private async Task<(double, double)?> LookupPostcodeAsync(string postcode)
        {
            var url = $"https://api.pdok.nl/bzk/locatieserver/search/v3_1/free?q={postcode}&fq=type:postcode&rows=1&fl=centroide_ll";
            var result = await httpClientFactory.CreateClient().GetFromJsonAsync<PdokResponse>(url);
            // "POINT(lon lat)"
            var point = result?.Response?.Docs?.FirstOrDefault()?.CentroideLl;
            var match = point == null ? null : Regex.Match(point, @"POINT\(([-0-9.]+) ([-0-9.]+)\)");
            if (match is not { Success: true })
                return null;
            return (double.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture),
                double.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture));
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