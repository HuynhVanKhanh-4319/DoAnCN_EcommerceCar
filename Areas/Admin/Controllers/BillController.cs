using CarBook.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarBook.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("admin/bill")]
    public class BillController : Controller
    {

        private readonly ApplicationDbContext _db;

        public BillController(ApplicationDbContext db)
        {
            _db = db;
        }
        [Route("index")]
        [Route("")]
        public async Task<IActionResult> Index(int page = 1, int pageSize = 20)
        {
            var totalBills = await _db.Bills.CountAsync();
            var bills = await _db.Bills
                             .Include(b => b.ApplicationUsers)
                             .OrderBy(b => b.Id)
                             .Skip((page - 1) * pageSize)         
                             .Take(pageSize)                      
                             .ToListAsync();
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalBills / pageSize);
            return View(bills);
        }
        [Route("details")]
        public async Task<IActionResult> Details(int id)
        {
            var bill = await _db.Bills
                .Include(b => b.ApplicationUsers) // Bao gồm thông tin người dùng
                .Include(b => b.DetailBills) // Bao gồm các chi tiết hóa đơn
                .ThenInclude(db => db.Products) // Bao gồm sản phẩm trong chi tiết hóa đơn
                .ThenInclude(p => p.Images) // Bao gồm hình ảnh sản phẩm
                .FirstOrDefaultAsync(b => b.Id == id);
            
            if (bill == null) return NotFound();
           
            // Chuyển đổi DetailBills thành List
            bill.DetailBills = bill.DetailBills.ToList();

            return View(bill);
        }


    }
}
