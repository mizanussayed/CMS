using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using WebApp.Admin.Service;
using WebApp.Admin.UI.Areas.Identity.Data;
using WebApp.Core.Model;
using WebApp.Core.Infrastructure;

namespace WebApp.Admin.UI.Areas.Identity.Pages.Account;

public class RegisterModel : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; }
    public string? ReturnUrl { get; set; }

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ILogger<RegisterModel> _logger;
    private readonly EmailService _emailService;
    private readonly EmailTemplateService _emailTemplateService;
    private readonly AuditLogService _auditLogService;

    public RegisterModel(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ILogger<RegisterModel> logger, EmailService emailService, EmailTemplateService emailTemplateService, AuditLogService auditLogService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
        _emailService = emailService;
        _emailTemplateService = emailTemplateService;
        _auditLogService = auditLogService;
    }

    public void OnGet(string? returnUrl)
    {
        ReturnUrl = returnUrl;
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl)
    {
        returnUrl = returnUrl ?? Url.Content("~/");

        if (ModelState.IsValid)
        {
            var user = new ApplicationUser
            {
                UserName = Input.Email,
                FullName = Input.FullName,
                Email = Input.Email,
                PhoneNumber = Input.PhoneNumber
            };

            var result = await _userManager.CreateAsync(user, Input.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "SystemAdmin");
                await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("FullName", Input.FullName));

                _logger.LogInformation("User created a new account with password.");

                // Audit Log
                var log = new LogModel
                {
                    UserName = User.Identity.Name == null ? user.UserName : User.Identity.Name,
                    UserRole = User.Identity.Name == null ? "SystemAdmin" : User.Claims.First(c => c.Type.Contains("role")).Value,
                    IP = Utility.GetIPAddress(Request),
                    TableName = "AspNetUsers",
                    OldData = User.Identity.Name == null ? null : $"<deleted Id=\"{user.Id}\" Name=\"{user.UserName}\" Feature=\"Register\" />",
                    NewData = $"<inserted Id=\"{user.Id}\" Name=\"{user.UserName}\" Feature=\"Register\" />"
                };
                _ = Task.Run(async () => { await _auditLogService.InsertAuditLog(log); });

                #region Send Email
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = user.Id, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);
                callbackUrl = HtmlEncoder.Default.Encode(callbackUrl);

                var emailTemplate = await _emailTemplateService.GetEmailTemplateByName("Confirm Email");
                emailTemplate.Template = emailTemplate.Template.Replace("$fullName", Input.FullName);
                emailTemplate.Template = emailTemplate.Template.Replace("$callbackUrl", callbackUrl);

                _ = Task.Run(async () =>
                {
                    await _emailService.SendEmail(new EmailModel { To = Input.Email, Subject = emailTemplate.Subject, Body = emailTemplate.Template });
                });
                #endregion

                if (_userManager.Options.SignIn.RequireConfirmedAccount)
                {
                    return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                }
                else
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return LocalRedirect(returnUrl);
                }
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        // If we got this far, something failed, redisplay form
        return Page();
    }

    public class InputModel
    {
        [Required, Display(Name = "Name")]
        public string FullName { get; set; }

        [Required, EmailAddress, Display(Name = "Email")]
        public string Email { get; set; }

        [Required, Phone, Display(Name = "Phone")]
        public string PhoneNumber { get; set; }

        [Required, StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; }
    }
}
