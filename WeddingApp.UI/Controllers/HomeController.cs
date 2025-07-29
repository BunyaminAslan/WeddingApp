using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Diagnostics;
using WeddingApp.UI.Models;

namespace WeddingApp.UI.Controllers
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
        [Route("Sino/Photos/{rs}")]
        public IActionResult Photos(string rs) {

            if (rs != "cba-rs2133") return BadRequest();

            return View();

        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult SliderImageAdd() { 
            
            return View();
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
