using CarBook.Data;
using CarBook.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarBook.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("admin/discount")]
    public class DiscountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DiscountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Danh sách giảm giá
        [HttpGet]
        [Route("index")]
        [Route("")]
        public IActionResult Index()
        {
            var discounts = _context.discounts
                .Include(d => d.Product)
                .Select(d => new
                {
                    d.Id,
                    ProductName = d.Product.Name,
                    d.Percentage,
                    d.StartDate,
                    d.EndDate,
                    IsActive = d.StartDate <= DateTime.Now && d.EndDate >= DateTime.Now // Kiểm tra giảm giá còn hiệu lực
                })
                .ToList();

            return View(discounts); // Truyền danh sách giảm giá vào view
        }

       
       [HttpGet]
        [Route("create")]
        public IActionResult Create()
        {
            ViewBag.Products = _context.Products
                .Select(p => new { p.Id, p.Name })
                .ToList();

            return View();
        }

        [HttpPost]
        [Route("create")]
        public IActionResult Create(int productId, decimal percentage, DateTime startDate, DateTime endDate)
        {
            if (ModelState.IsValid)
            {
                var discount = new Discount
                {
                    ProductId = productId,
                    Percentage = percentage,
                    StartDate = startDate,
                    EndDate = endDate
                };

                _context.discounts.Add(discount);
                _context.SaveChanges();

                return RedirectToAction("Index");
            }

            ViewBag.Products = _context.Products
                .Select(p => new { p.Id, p.Name })
                .ToList();

            return View();
        }

        // Sửa giảm giá
        [HttpGet]
        [Route("edit/{id}")]
        public IActionResult Edit(int id)
        {
            var discount = _context.discounts.FirstOrDefault(d => d.Id == id);
            if (discount == null) return NotFound();

            ViewBag.Products = _context.Products
                .Select(p => new { p.Id, p.Name })
                .ToList();

            return View(discount);
        }

        [HttpPost]
        [Route("edit/{id}")]
        public IActionResult Edit(int id, int productId, decimal percentage, DateTime startDate, DateTime endDate)
        {
            var discount = _context.discounts.FirstOrDefault(d => d.Id == id);
            if (discount == null) return NotFound();

            if (ModelState.IsValid)
            {
                discount.ProductId = productId;
                discount.Percentage = percentage;
                discount.StartDate = startDate;
                discount.EndDate = endDate;

                _context.discounts.Update(discount);
                _context.SaveChanges();

                return RedirectToAction("Index");
            }

            ViewBag.Products = _context.Products
                .Select(p => new { p.Id, p.Name })
                .ToList();

            return View(discount);
        }

        // Xóa giảm giá
        [HttpPost]
        [Route("delete/{id}")]
        public IActionResult Delete(int id)
        {
            var discount = _context.discounts.FirstOrDefault(d => d.Id == id);
            if (discount == null) return NotFound();

            _context.discounts.Remove(discount);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}

