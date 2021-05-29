using System.Threading.Tasks;
using DatabaseInterface.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace EnergyPortal.Controllers.WebApi
{
    [Route("webapi/v3/session")]
    public class SessionController : Controller
    {
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly ILogger<SessionController> logger;

        public SessionController(SignInManager<ApplicationUser> signInManager, ILogger<SessionController> logger)
        {
            this.signInManager = signInManager;
            this.logger = logger;
        }
        
        [Authorize]
        [HttpGet]
        [Route("logout")]
        private async Task<IActionResult> Logout()
        {
            logger.LogInformation("User is trying to logout");
            
            await signInManager.SignOutAsync();
            return Ok();
        }
    }
}