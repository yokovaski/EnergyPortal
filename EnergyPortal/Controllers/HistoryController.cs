using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnergyPortal.Controllers
{
    public class HistoryController : Controller
    {
        [Authorize]
        public IActionResult Index()
        {
            return View();
        }
    }
}