using CarBook.Data;
using CarBook.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CarBook.Models;

namespace CarBook.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("admin/brand")]
  
    public class BrandController : Controller
    {
        private readonly ApplicationDbContext _db;

        public BrandController(ApplicationDbContext db)
        {
            _db = db;
        }

        
        [HttpGet("")]
        [HttpGet("index")]
        public IActionResult Index()
        {
            var brandList = _db.Brands.ToList(); 
            ViewBag.BrandList = brandList;
            return View();
        }

       
        [HttpGet("create")]
        public IActionResult Create()
        {
            return View(new Brand());
        }

        
        [HttpPost("create")]
   
        public IActionResult Create(Brand brand)
        {
            if (ModelState.IsValid)
            {
                _db.Brands.Add(brand);
                _db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(brand);
        }

       
        [HttpGet("edit/{id:int}")]
        public IActionResult Edit(int id)
        {
            var brand = _db.Brands.Find(id);
            if (brand == null)
            {
                return NotFound();
            }

            return View(brand);
        }

      
        [HttpPost("edit/{id:int}")]

        public IActionResult Edit(int id, Brand brand)
        {
       

            if (ModelState.IsValid)
            {
                _db.Brands.Update(brand);
                _db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(brand);
        }


        [HttpGet]
        [Route("Delete/{id}")]
        public IActionResult Delete(int id)
        {
            if (id == 0)
            {
                return NotFound();
            }

            var brand = _db.Brands.Find(id);
            if (brand == null)
            {
                return NotFound();
            }

            _db.Brands.Remove(brand);
            _db.SaveChanges();
            return RedirectToAction("Index", "brand");
        }

      
    }
}
