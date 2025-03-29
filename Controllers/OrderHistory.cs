using CarBook.Data;
using CarBook.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarBook.Controllers
{
    
    [Route("orderHistory")]
    [Authorize]
    public class OrderHistory : Controller
    {
     
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        public OrderHistory(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;

        }
        [Route("index")]
        [Route("")]
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User); 
            var bills = await _db.Bills
                .Where(b => b.ApplicationUserId == userId)  
                .Include(b => b.DetailBills) 
                .ThenInclude(db => db.Products) 
                .ToListAsync(); 

            return View(bills); 
        }
        [Route("Details")]
        //[HttpPost]
        public async Task<IActionResult> Details(int id)
        {
            var bill = await _db.Bills
                .Include(b => b.ApplicationUsers) 
                .Include(b => b.DetailBills)
                    .ThenInclude(db => db.Products) 
                .ThenInclude(p => p.Images) 
                .FirstOrDefaultAsync(b => b.Id == id);

            if (bill == null) return NotFound();

            return View(bill);
        }

    }
}
