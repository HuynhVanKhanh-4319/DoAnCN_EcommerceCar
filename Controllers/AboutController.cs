using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CarBook.Controllers
{
    [Route("about")]
    public class AboutController : Controller
    {
        [Route("index")]
        [Route("")]
        public IActionResult Index()
        {
            return View();
        }
        [Route("detail")]
        public IActionResult Detail()
        {
            return View("detail");
        }

    }
}
