using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApp.Admin.Service;
using WebApp.Admin.UI.Areas.Identity.Data;
using WebApp.Core.Model;
using WebApp.Core.Infrastructure;

namespace WebApp.Admin.UI.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ResetPasswordModel : PageModel
    {
        [BindProperty]
        public InputModel Input { get; set; }

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AuditLogService _auditLogService;
        private readonly EmailService _emailService;
        private readonly EmailTemplateService _emailTemplateService;

        public ResetPasswordModel(UserManager<ApplicationUser> userManager, AuditLogService auditLogService, EmailService emailService, EmailTemplateService emailTemplateService)
        {
            _userManager = userManager;
            _auditLogService = auditLogService;
            _emailService = emailService;
            _emailTemplateService = emailTemplateService;
        }

        public async Task<IActionResult> OnGetAsync(string code = null)
        {
            if (code == null)
            {
                return BadRequest("A code must be supplied for password reset.");
            }

            var user = await _userManager.FindByIdAsync(code);

            if (user == null)
                return NotFound($"Unable to load user.");

            Input = new InputModel
            {
                Email = user.Email,
                Code = code
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToPage("./ResetPasswordConfirmation");
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, Input.Password);
            if (result.Succeeded)
            {
                // Audit Log
                var log = new LogModel
                {
                    UserName = user.UserName,
                    UserRole = "",
                    IP = Utility.GetIPAddress(Request),
                    TableName = "AspNetUsers",
                    OldData = $"<deleted Id=\"{user.Id}\" Name=\"{user.UserName}\" Feature=\"ResetPassword\" />",
                    NewData = $"<inserted Id=\"{user.Id}\" Name=\"{user.UserName}\" Feature=\"ResetPassword\" />"
                };
                _ = Task.Run(async () => { await _auditLogService.InsertAuditLog(log); });

                // Send Email
                var emailTemplate = await _emailTemplateService.GetEmailTemplateByName("Reset Password");
                emailTemplate.Template = emailTemplate.Template.Replace("$fullName", user.FullName);
                emailTemplate.Template = emailTemplate.Template.Replace("$password", Input.Password);
                _ = Task.Run(async () =>
                {
                    await _emailService.SendEmail(new EmailModel { To = Input.Email, Subject = emailTemplate.Subject, Body = emailTemplate.Template });
                });

                return RedirectToPage("./ResetPasswordConfirmation");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return Page();
        }
    }

    public class InputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Required]
        public string Code { get; set; }
    }
}