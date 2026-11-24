using System.Linq;
using System.Threading.Tasks;
using DatabaseInterface;
using DatabaseInterface.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EnergyPortal.Controllers.WebApi
{
    /// <summary>Who is logged in, for the app shell (initials) and the Account rows on Instellingen.</summary>
    [Authorize]
    [Route("webapi/v3/account")]
    public class AccountWebApiController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager) : Controller
    {
        [HttpGet]
        public async Task<IActionResult> GetAccount()
        {
            var user = await userManager.GetUserAsync(User);

            if (user == null)
                return NotFound("User could not be found");

            var rpiKey = await dbContext.RaspberryPis
                .Where(r => r.UserId == user.Id)
                .Select(r => r.RpiKey)
                .FirstOrDefaultAsync();

            return Ok(new
            {
                user.Email,
                user.Name,
                user.EmailConfirmed,
                user.TwoFactorEnabled,
                RpiKey = rpiKey
            });
        }
    }
}
