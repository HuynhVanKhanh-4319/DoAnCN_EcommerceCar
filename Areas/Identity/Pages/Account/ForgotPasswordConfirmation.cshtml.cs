// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using CarBook.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CarBook.Areas.Identity.Pages.Account
{
    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    [AllowAnonymous]
    public class ForgotPasswordConfirmation : PageModel
    {
        private readonly IEmailSender _emailSender;
        private readonly UserManager<ApplicationUser> _userManager;

        public ForgotPasswordConfirmation(UserManager<ApplicationUser> userManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _emailSender = emailSender;
        }

        public async Task<IActionResult> OnPostAsync(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return Page();
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Không tìm thấy tài khoản với email này.");

                return RedirectToPage("./ForgotPassword");
            }
            await SendPasswordResetEmailAsync(user);

            return RedirectToPage("./ForgotPasswordConfirmation");
        }

        private async Task SendPasswordResetEmailAsync(ApplicationUser user)
        {

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetLink = Url.Page(
                "/Account/ResetPassword",
                pageHandler: null,
                values: new { token, email = user.Email },
                protocol: Request.Scheme);

            await _emailSender.SendEmailAsync(user.Email, "Reset Password",
                $"Vui lòng đặt lại mật khẩu của bạn bằng cách <a href='{resetLink}'>nhấp vào đây</a>.");
        }
    }
}
