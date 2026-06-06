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
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly EmailService _emailService;
        private readonly EmailTemplateService _emailTemplateService;
        private readonly AuditLogService _auditLogService;

        public ForgotPasswordModel(UserManager<ApplicationUser> userManager, EmailService emailService, EmailTemplateService emailTemplateService, AuditLogService auditLogService)
        {
            _userManager = userManager;
            _emailService = emailService;
            _emailTemplateService = emailTemplateService;
            _auditLogService = auditLogService;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(Input.Email);
                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    return RedirectToPage("./ForgotPasswordConfirmation");
                }

                // Audit Log
                var log = new LogModel
                {
                    UserName = user.UserName,
                    UserRole = "",
                    IP = Utility.GetIPAddress(Request),
                    TableName = "AspNetUsers",
                    OldData = $"<deleted Id=\"{user.Id}\" Name=\"{user.UserName}\" Feature=\"ForgotPassword\" />",
                    NewData = $"<inserted Id=\"{user.Id}\" Name=\"{user.UserName}\" Feature=\"ForgotPassword\" />"
                };
                _ = Task.Run(async () => { await _auditLogService.InsertAuditLog(log); });

                // Send Email
                var callbackUrl = Url.Page(
                    "/Account/ResetPassword",
                    pageHandler: null,
                    values: new { area = "Identity", code = user.Id },
                    protocol: Request.Scheme);

                var emailTemplate = await _emailTemplateService.GetEmailTemplateByName("Forgot Password");
                emailTemplate.Template = emailTemplate.Template.Replace("$fullName", user.FullName);
                emailTemplate.Template = emailTemplate.Template.Replace("$callbackUrl", callbackUrl);

                _ = Task.Run(async () =>
                {
                    await _emailService.SendEmail(new EmailModel { To = Input.Email, Subject = emailTemplate.Subject, Body = emailTemplate.Template });
                });

                return RedirectToPage("./ForgotPasswordConfirmation");
            }

            return Page();
        }
    }
}