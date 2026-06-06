using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AspNetCore.ReCaptcha;
using WebApp.Admin.UI.Areas.Identity.Data;
using WebApp.Admin.Service;
using WebApp.Core.Model;

namespace WebApp.Admin.UI.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
	[ValidateReCaptcha]
	public class LoginModel : PageModel
    {
        [BindProperty]
        public InputModel Input { get; set; }
        public string ReturnUrl { get; set; }
        [TempData]
        public string ErrorMessage { get; set; }

        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly AuthService _authService;

        public LoginModel(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, ILogger<LoginModel> logger, AuthService authService)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
            _authService = authService;
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            if (!ModelState.IsValid)
            {
                ModelState.AddModelError(string.Empty, "Please select 'I'm not a robot'.");
                return Page();
            }
            else
            {
                var result = await _signInManager.PasswordSignInAsync(Input.UserName, Input.Password, Input.RememberMe, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in.");

                    var user = await _userManager.FindByNameAsync(Input.UserName);
                    UserInfoModel userInfo = new UserInfoModel
                    {
                        Id = user.Id,
                        UserName = user.UserName,
                        Name = user.FullName,
                        Email = user.Email,
                        Role = (await _userManager.GetRolesAsync(user)).First()
                    };
                    TokenModel token = await _authService.GetToken(userInfo);
                    Response.Cookies.Append("X-Token", token.JwtToken, new CookieOptions { Expires = token.Expires, HttpOnly = true });
                    Response.Cookies.Append("X-RefreshToken", token.RefreshToken, new CookieOptions { Expires = token.RefreshTokenExpires, HttpOnly = true });

                    return LocalRedirect(returnUrl);
                }
                if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    return RedirectToPage("./Lockout");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return Page();
                }
            }
        }

        public class InputModel
        {
            [Required]
            [Display(Name = "User Name")]
            public string UserName { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }
    }
}