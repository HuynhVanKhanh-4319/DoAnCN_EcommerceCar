using CarBook.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace CarBook.Controllers
{
    [Route("home")]
    public class HomeController : Controller
    {
        [Route("index")]
        [Route("~/")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
