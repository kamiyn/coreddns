using Microsoft.AspNetCore.Mvc;

namespace Coreddns.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return Content("It works!", "text/plain");
        }

        public IActionResult Error()
        {
            return Content("Error", "text/plain");
        }
    }
}
