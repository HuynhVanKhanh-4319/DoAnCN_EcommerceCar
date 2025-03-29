using CarBook.Data;
using CarBook.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace CarBook.Areas.Admin.Controllers
{
    [Area("Admin")]
   [Route("admin/category")]
   
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _db;

        public CategoryController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        [Route("")]
        [Route("index")]

        public IActionResult Index()
        {
            var theLoaiList = _db.Categories.ToList();
            ViewBag.TheLoai = theLoaiList;
            return View();
        }

        [HttpGet]
        [Route("Create")]
        public IActionResult Create()
        {
            var category = new Category();
            return View("create",category);
        }

        [HttpPost]
        [Route("Create")]
       
        public IActionResult Create(Category category)
        {
            if (ModelState.IsValid)
            {
                _db.Categories.Add(category);
                _db.SaveChanges();
                return RedirectToAction("Index", "category");
            }
            return View();
        }

        [HttpGet]
        [Route("Edit")]
        public IActionResult Edit(int id)
        {
            if (id == 0)
            {
                return NotFound();
            }

            var theLoai = _db.Categories.Find(id);
            if (theLoai == null)
            {
                return NotFound();
            }

            return View(theLoai);
        }

        [HttpPost]
        [Route("Edit")]
        public IActionResult Edit(Category category)
        {
            if (ModelState.IsValid)
            {
                _db.Categories.Update(category);
                _db.SaveChanges();
                return RedirectToAction("Index","category");
            }

            return View();
        }

        [HttpGet]
        [Route("Delete/{id}")]
        public IActionResult Delete(int id)
        {
            if (id == 0)
            {
                return NotFound();
            }

            var theLoai = _db.Categories.Find(id);
            if (theLoai == null)
            {
                return NotFound();
            }

            _db.Categories.Remove(theLoai);
            _db.SaveChanges();
            return RedirectToAction("Index", "Category");
        }


  
     
    }
}