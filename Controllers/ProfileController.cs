using System.Threading.Tasks;
using CarBook.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;

namespace CarBook.Controllers
{
    [Authorize]
    [Route("profile")]
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfileController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }
        [HttpGet]
        [Route("Edit")]
        [Authorize]
        public async Task<IActionResult> Edit()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var model = new EditProfileViewModel
            {
                Name = user.Name,
                Address = user.Address,
                PhoneNumber = user.PhoneNumber,
               CitizenID = user.CitizenID,
                ExistingImagePath = user.Image
            };

            return View(model);
        }

        [HttpPost]
        [Route("Edit")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditProfileViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return NotFound();
                }

                // Cập nhật thông tin người dùng
                user.Name = model.Name;
                user.Address = model.Address;
                user.PhoneNumber = model.PhoneNumber;
                user.CitizenID = model.CitizenID;

                // Xử lý upload file hình ảnh
                if (model.Image != null)
                {
                    // Đường dẫn thư mục để lưu hình ảnh
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/profiles");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // Tạo tên file duy nhất
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + model.Image.FileName;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // Lưu file vào thư mục
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.Image.CopyToAsync(fileStream);
                    }

                    // Xóa hình ảnh cũ nếu có
                    if (!string.IsNullOrEmpty(user.Image))
                    {
                        var oldFilePath = Path.Combine(uploadsFolder, user.Image);
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    // Lưu đường dẫn hình ảnh vào database
                    user.Image = uniqueFileName;
                }

                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    return RedirectToAction(nameof(Edit));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }




    }

}

