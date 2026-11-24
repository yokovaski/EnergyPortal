using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using EnergyPortal.Models;
using Microsoft.AspNetCore.Authorization;

namespace EnergyPortal.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> logger;

        public HomeController(ILogger<HomeController> logger)
        {
            this.logger = logger;
        }

        // All app pages serve the same Vue entry; vue-router picks the page.
        [Authorize]
        [HttpGet("/")]
        [HttpGet("/history")]
        [HttpGet("/settings")]
        public IActionResult Index()
        {
            return View();
        }

        // Old URL of the settings page
        public IActionResult Settings()
        {
            return RedirectPermanent("/settings");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(int? code = null)
        {
            ViewData["StatusCode"] = code;
            return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
        }
    }
}