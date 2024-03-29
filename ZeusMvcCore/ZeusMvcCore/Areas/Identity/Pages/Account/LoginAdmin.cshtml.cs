﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using ZeusMvcCore.Models;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace ZeusMvcCore.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LoginAdminModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ILogger<LoginModel> _logger;

        public LoginAdminModel(UserManager<User> userManager, SignInManager<User> signInManager, ILogger<LoginModel> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl = returnUrl ?? Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");

            //if (ModelState.IsValid)
            //{
            //    // This doesn't count login failures towards account lockout
            //    // To enable password failures to trigger account lockout, set lockoutOnFailure: true

            //    //var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: true);

            //    //if (result.Succeeded)
            //    //{
            //    //    _logger.LogInformation("User logged in.");
            //    //    return LocalRedirect(returnUrl);
            //    //}
            //    //if (result.RequiresTwoFactor)
            //    //{
            //    //    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
            //    //}
            //    //if (result.IsLockedOut)
            //    //{
            //    //    _logger.LogWarning("User account locked out.");
            //    //    return RedirectToPage("./Lockout");
            //    //}
            //    //else
            //    //{
            //    //    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            //    //    return Page();
            //    //}
            //}

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(Input.Email);

                if (user != null && await _userManager.CheckPasswordAsync(user, Input.Password))
                {
                    var identity = new ClaimsIdentity("AdminScheme");
                    identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, Input.Email));
                    identity.AddClaim(new Claim(ClaimTypes.Email, Input.Email));
                    identity.AddClaim(new Claim(ClaimTypes.Role, "Admin"));

                    await HttpContext.SignInAsync("AdminScheme", new ClaimsPrincipal(identity));
                    return LocalRedirect(returnUrl);
                }

                // If we got this far, something failed, redisplay form
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}
