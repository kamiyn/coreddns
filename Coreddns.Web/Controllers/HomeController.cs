using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Coreddns.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger _logger;
        public HomeController(
            ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            _logger.LogDebug("It works!");
            return Content("It works!", "text/plain");
        }

        public IActionResult Error()
        {
            // _logger.LogInformation("Error is coming");
            return Content("Error", "text/plain");
        }
    }
}
