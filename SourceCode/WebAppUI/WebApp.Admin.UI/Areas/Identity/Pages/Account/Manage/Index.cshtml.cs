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

namespace WebApp.Admin.UI.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly EmailTemplateService _emailTemplateService;
        private readonly AuditLogService _auditLogService;
        private readonly EmailService _emailService;
        private readonly SMSService _smsService;

        public IndexModel(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, EmailTemplateService emailTemplateService, AuditLogService auditLogService, EmailService emailService, SMSService smsService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailTemplateService = emailTemplateService;
            _auditLogService = auditLogService;
            _emailService = emailService;
            _smsService = smsService;
        }

        private void Load(ApplicationUser user)
        {
            Input = new InputModel
            {
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber
            };
        }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            var user = (id == null) ? await _userManager.GetUserAsync(User) : await _userManager.FindByIdAsync(id);

            if (user == null)
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

            Load(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = (Input.UserId == null) ? await _userManager.GetUserAsync(User) : await _userManager.FindByIdAsync(Input.UserId);

            if (user == null)
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

            if (!ModelState.IsValid)
            {
                Load(user);
                return Page();
            }

            string oldEmail = user.Email;
            string oldFullName = user.FullName;
            StatusMessage = "";

            try
            {
                user.FullName = Input.FullName;
                if (Input.Email != oldEmail)
                {
                    user.Email = Input.Email;
                    user.EmailConfirmed = false;
                }
                user.UserName = Input.Email;
                user.PhoneNumber = Input.PhoneNumber;
                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    await _userManager.RemoveClaimAsync(user, new System.Security.Claims.Claim("FullName", oldFullName));
                    await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("FullName", Input.FullName));

                    // Audit Log
                    var log = new LogModel
                    {
                        UserName = User.Identity.Name,
                        UserRole = User.Claims.First(c => c.Type.Contains("role")).Value,
                        IP = Utility.GetIPAddress(Request),
                        TableName = "AspNetUsers",
                        OldData = $"<deleted Id=\"{user.Id}\" Name=\"{user.UserName}\" Feature=\"ProfileUpdate\" />",
                        NewData = $"<inserted Id=\"{user.Id}\" Name=\"{user.UserName}\" Feature=\"ProfileUpdate\" />"
                    };
                    _ = Task.Run(async () => { await _auditLogService.InsertAuditLog(log); });

                    if (Input.Email != oldEmail)
                    {
                        // Send Email
                        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                        var callbackUrl = Url.Page(
                            "/Account/ConfirmEmail",
                            pageHandler: null,
                            values: new { area = "Identity", userId = user.Id, code = code },
                            protocol: Request.Scheme);
                        callbackUrl = HtmlEncoder.Default.Encode(callbackUrl);

                        var emailTemplate = await _emailTemplateService.GetEmailTemplateByName("Confirm Email");
                        emailTemplate.Template = emailTemplate.Template.Replace("$fullName", Input.FullName);
                        emailTemplate.Template = emailTemplate.Template.Replace("$callbackUrl", callbackUrl);

                        _ = Task.Run(async () =>
                        {
                            await _emailService.SendEmail(new WebApp.Core.Model.EmailModel { To = Input.Email, Subject = emailTemplate.Subject, Body = emailTemplate.Template });
                        });

                        StatusMessage = "Verification email sent. Please check your email.";
                    }
                }
            }
			catch (Exception)
			{
                return RedirectToPage();
            }

            // All ok
            if (User.Identity.Name == Input.Email) await _signInManager.RefreshSignInAsync(user);
            StatusMessage = StatusMessage.Length > 0 ? (StatusMessage + Environment.NewLine) : StatusMessage;
            StatusMessage += "Your profile has been updated.";

            // Send SMS
            _ = Task.Run(async () =>
            {
                await _smsService.SendSMS(
                new SMSModel
                {
                    To = new List<string> {
                        Input.PhoneNumber
                    },
                    Content = "SMS from application."
                });
            });

            return RedirectToPage();
        }

        public class InputModel
        {
            public string UserId { get; set; }

            //[Required, Display(Name = "User Name")]
            //public string UserName { get; set; }

            [Required, EmailAddress, Display(Name = "Email")]
            public string Email { get; set; }

            [Required, Display(Name = "Full Name")]
            public string FullName { get; set; }

            [Required, Phone, Display(Name = "Phone Number")]
            public string PhoneNumber { get; set; }
        }
    }
}