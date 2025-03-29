using Microsoft.AspNetCore.Mvc;

namespace CarBook.Areas.Admin.Controllers
{
    [Area("admin")]
    [Route("admin/dashboard")]
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
