using CarBook.Data;
using CarBook.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CarBook.Areas.Admin.Controllers
{
    [Area("admin")]
    [Route("admin/product")]
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(ApplicationDbContext dbContext, IWebHostEnvironment webHostEnvironment)
        {
            _dbContext = dbContext;
            _webHostEnvironment = webHostEnvironment;
        }

        [Route("index")]
        [Route("")]
        public async Task<IActionResult> Index()
        {
            
            ViewBag.data = await _dbContext.Products.ToListAsync();
            return View();
            
        }

        [Route("add")]
        public IActionResult Add()
        {
            var product = new Product();
            ViewBag.categories = _dbContext.Categories.ToList();
            ViewBag.Brands = _dbContext.Brands.ToList();
            return View("add", product);
        }

        [Route("add/save")]
        [HttpPost]
        public async Task<IActionResult> Add(Product product, List<IFormFile> images)
        {
            foreach (var image in images)
            {
                product.Images.Add(new ProductImage() { Id = 0, Path = image.FileName });
                var path = Path.Combine(_webHostEnvironment.WebRootPath, "images/products", image.FileName);
                using (var fileStream = new FileStream(path, FileMode.Create))
                {
                    await image.CopyToAsync(fileStream);
                }
            }

            await _dbContext.Products.AddAsync(product);
            await _dbContext.SaveChangesAsync();
            return RedirectToAction("index", "product");
        }
        [HttpGet]
        [Route("update")]
        public IActionResult Update(int id)
        {
            if (id == 0)
            {
                return NotFound();
            }

            var product = _dbContext.Products.Include(p => p.Images).FirstOrDefault(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            ViewBag.categories = _dbContext.Categories.ToList();
            ViewBag.Brands = _dbContext.Brands.ToList();

            return View(product);
        }

        [HttpPost]
        [Route("update")]
        public async Task<IActionResult> Update(Product product, List<IFormFile> images, int id)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var productFromDb = _dbContext.Products.Include(p => p.Images).FirstOrDefault(p => p.Id == id);

                if (productFromDb == null) 
                {
                    return NotFound();
                }


                if (productFromDb.Images != null) 
                {
                    foreach (var oldImage in productFromDb.Images.ToList())
                    {
                        _dbContext.ProductImages.Remove(oldImage);

                        var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, "images/products", oldImage.Path);
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }
                }
                if (images != null && images.Count > 0)
                {
                    foreach (var image in images)
                    {
                        var newImagePath = Path.Combine(_webHostEnvironment.WebRootPath, "images/products", image.FileName);
                        using (var fileStream = new FileStream(newImagePath, FileMode.Create))
                        {
                            await image.CopyToAsync(fileStream);
                        }
                        productFromDb.Images.Add(new ProductImage() { Path = image.FileName });
                    }
                }


                productFromDb.Name = product.Name;
                productFromDb.Description = product.Description;
                productFromDb.Price = product.Price;
                productFromDb.Amount = product.Amount;
                productFromDb.CategoryId = product.CategoryId;
                productFromDb.BrandId = product.BrandId;
                productFromDb.UpdatedAt = DateTime.Now;

                _dbContext.Products.Update(productFromDb);
                await _dbContext.SaveChangesAsync(); 
                return RedirectToAction("index");
            }

           
            ViewBag.categories = _dbContext.Categories.ToList(); 
            ViewBag.Brands = _dbContext.Brands.ToList(); 
            return View(product);
        }



        [Route("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _dbContext.Products.FirstOrDefaultAsync(x => x.Id == id);

            if (product == null)
            {
                return NotFound();
            }
            product.Images.Clear();
            _dbContext.Entry(product).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
            _dbContext.Products.Remove(product);
            await _dbContext.SaveChangesAsync();

            return RedirectToAction("Index");
        }
        [HttpPost]
        [Route("update-status")]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            if (id <= 0 || string.IsNullOrEmpty(status))
            {
                TempData["ErrorMessage"] = "Dữ liệu không hợp lệ.";
                return RedirectToAction("Index");
            }

            var product = await _dbContext.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (product == null)
            {
                TempData["ErrorMessage"] = "Sản phẩm không tồn tại.";
                return RedirectToAction("Index");
            }

           
            product.Status = status;
            _dbContext.Products.Update(product);
            await _dbContext.SaveChangesAsync();

            TempData["SuccessMessage"] = "Cập nhật trạng thái sản phẩm thành công!";
            return RedirectToAction("Index");
        }

    }
}