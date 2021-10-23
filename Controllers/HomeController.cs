 using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;  

namespace ScrumPoker.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        { 
            return View();
        }
         
        [HttpPost]
        public IActionResult Index(string userName)
        { 
            return View("Index");
        }
 
    }


}
