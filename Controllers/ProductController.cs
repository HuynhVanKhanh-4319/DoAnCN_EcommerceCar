using CarBook.Data;
using CarBook.Models;
using EcommerceCar.Migrations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;



namespace CarBook.Controllers
{
    [Route("product")]
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const int PageSize = 9; 

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Route("index")]
        [Route("")]
        [HttpGet]
        public IActionResult Index(List<int> types, string brand = "all", string searchTerm = null, int page = 1)
        {
   
            var products = _context.Products.AsQueryable();

            if (types != null && types.Count > 0)
            {
                products = products.Where(p => types.Contains(p.CategoryId));
            }
            if (brand != "all")
            {
                products = products.Where(p => p.BrandId.ToString() == brand);
            }
            if (!string.IsNullOrEmpty(searchTerm))
            {
                products = products.Where(p => p.Name.ToLower().Contains(searchTerm.ToLower()));
            }
            int totalItems = products.Count();
            var paginatedProducts = products
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .Include(p => p.Reviews)
                .Include(p => p.Discounts) 
                .ToList()
                .Select(p => new
                {
                    Product = p,
                    AverageRating = p.Reviews.Any() ? Math.Round(p.Reviews.Average(r => r.Rating), 1) : 0,
                    Discount = p.Discounts
                        .Where(d => d.IsActive && DateTime.Now >= d.StartDate && DateTime.Now <= d.EndDate)
                        .FirstOrDefault()

                })
                .ToList();
            ViewBag.ProductsWithRatings = paginatedProducts;
            ViewBag.Categories = _context.Categories .Select(c => new { c.Id, c.Name }) .ToList();
            ViewBag.Brands = _context.Brands .Select(b => new { b.Id, b.Name }) .ToList();
            ViewBag.Types = types;
            ViewBag.Brand = brand;
            ViewBag.SearchTerm = searchTerm;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)PageSize);
            ViewBag.CurrentPage = page;

            return View();
        }


        [HttpGet]
        public IActionResult Autocomplete(string term)
        {
            var suggestions = _context.Products
                .Where(p => p.Name.Contains(term))
                .Select(p => new { p.Id, p.Name }) 
                .ToList();

            return Json(suggestions); 
        }

        [Route("detail/{id}")]
        public ActionResult Details(int id)
        {
            var product = _context.Products
                                  .Include(p => p.Images)
                                  .Include(p =>p.Reviews)
                                  .Include(P =>P.Discounts)
                                  .FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }
            var hasDiscount = product.Discounts != null && product.Discounts.Any();
            ViewBag.HasDiscount = hasDiscount;

            var discount = product.Discounts.FirstOrDefault();
            decimal priceAfterDiscount = product.Price;

            if (discount != null)
            {
                priceAfterDiscount = product.Price * (1 - discount.Percentage / 100);
            }

            ViewBag.PriceAfterDiscount = priceAfterDiscount;

            var paginatedProducts =_context.Products
                 .Include(p => p.Reviews)
                 .Include(p => p.Discounts)
                 .Where(p => p.Brand == product.Brand && p.Id != product.Id)
                 .ToList()
                 .Select(p => new
                 {
                     Product = p,
                     AverageRating = p.Reviews.Any() ? Math.Round(p.Reviews.Average(r => r.Rating), 1) : 0,
                     Discount = p.Discounts
                         .Where(d => d.IsActive && DateTime.Now >= d.StartDate && DateTime.Now <= d.EndDate)
                         .FirstOrDefault()

                 })
                 .ToList();
            ViewBag.ProductsWithRatings = paginatedProducts;
            ViewBag.Data = product;
            ViewBag.Reviews = product.Reviews ?? new List<Review>();
            return View("Detail", product);
        }
        [HttpGet]
        [Route("AddReview")]
        public IActionResult AddReview(int productId)
        {
            var model = new Review { ProductId = productId };
            return View(model);
        }
        [HttpPost]
        [Route("AddReview")]
        public IActionResult AddReview(int productId, string userName, int rating, string comment)
        {
            if (ModelState.IsValid)
            {
                var review = new Review
                {
                    ProductId = productId,
                    UserName = userName,
                    Rating = rating,
                    Comment = comment,
                    CreatedAt = DateTime.Now
                };

                _context.Reviews.Add(review);
                _context.SaveChanges();
                return RedirectToAction("Detail", new { id = productId });
            }
            else
            {
                return View("Detail", new { id = productId });
            }

        }


    }
}
