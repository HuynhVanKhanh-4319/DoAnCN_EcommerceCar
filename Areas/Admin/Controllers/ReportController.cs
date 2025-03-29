using CarBook.Data;
using CarBook.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace CarBook.Areas.Admin.Controllers
{
    [Area("admin")]
    [Route("admin/report")]
    public class ReportController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Route("index")]
        [Route("")]
        public IActionResult Index()
        {
            // Thống kê doanh thu theo tháng
            var ordersByMonth = _context.Bills
                .Where(b => b.OrderDate >= DateTime.Now.AddYears(-1)) // Lọc trong 1 năm qua
                .GroupBy(b => b.OrderDate.Month)
                .Select(g => new
                {
                    Month = g.Key,
                    Total = g.Sum(b => b.Total)  // Tổng doanh thu theo tháng
                })
                .ToList();

            var chartModel = new ChartViewModel
            {
                Labels = ordersByMonth.Select(o => "Tháng " + o.Month).ToList(),
                Data = ordersByMonth.Select(o => o.Total).ToList()
            };

            // Thống kê số xe được đặt nhiều nhất
            var topVehicles = _context.DetailBills
                .GroupBy(d => d.ProductId) // Nhóm theo ProductId (xe)
                .Select(g => new
                {
                    ProductId = g.Key,
                    TotalQuantity = g.Sum(d => d.ProductId)
                })
                .OrderByDescending(g => g.TotalQuantity) // Sắp xếp theo số lượng giảm dần
                .Take(5) // Lấy top 5 xe được đặt nhiều nhất
                .ToList();

            // Tạo một danh sách để truyền vào View
            var topVehiclesList = topVehicles.Select(tv => new
            {
                ProductName = _context.Products.FirstOrDefault(p => p.Id == tv.ProductId)?.Name, // Tên xe
                TotalQuantity = tv.TotalQuantity // Số lượng xe được đặt
            }).ToList();

            // Truyền dữ liệu vào View
            ViewBag.TopVehicles = topVehiclesList;
            return View(chartModel);
        }
    }
}
