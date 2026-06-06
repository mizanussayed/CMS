// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using WebApp.Admin.Service;
using WebApp.Admin.UI.Areas.Identity.Data;

namespace WebApp.Admin.UI.Areas.Identity.Pages.Account.Manage
{
    public class EmailModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly EmailService _emailService;
        private readonly EmailTemplateService _emailTemplateService;

        public EmailModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            EmailService emailService,
            EmailTemplateService emailTemplateService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
            _emailTemplateService = emailTemplateService;
        }

        public string Email { get; set; }

        public bool IsEmailConfirmed { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "New email")]
            public string NewEmail { get; set; }
        }

        private async Task LoadAsync(ApplicationUser user)
        {
            var email = await _userManager.GetEmailAsync(user);
            Email = email;

            Input = new InputModel
            {
                NewEmail = email,
            };

            IsEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user);
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostChangeEmailAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var email = await _userManager.GetEmailAsync(user);
            if (Input.NewEmail != email)
            {
                // Send Email
                var userId = await _userManager.GetUserIdAsync(user);
                var code = await _userManager.GenerateChangeEmailTokenAsync(user, Input.NewEmail);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ConfirmEmailChange",
                    pageHandler: null,
                    values: new { area = "Identity", userId = userId, email = Input.NewEmail, code = code },
                    protocol: Request.Scheme);
                callbackUrl = HtmlEncoder.Default.Encode(callbackUrl);

                var emailTemplate = await _emailTemplateService.GetEmailTemplateByName("Confirm Email");
                emailTemplate.Template = emailTemplate.Template.Replace("$fullName", user.FullName);
                emailTemplate.Template = emailTemplate.Template.Replace("$callbackUrl", callbackUrl);

                _ = Task.Run(async () =>
                {
                    await _emailService.SendEmail(new WebApp.Core.Model.EmailModel { To = Input.NewEmail, Subject = emailTemplate.Subject, Body = emailTemplate.Template });
                });

                StatusMessage = "Confirmation link to change email sent. Please check your email.";
                return RedirectToPage();
            }

            StatusMessage = "Your email is unchanged.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostSendVerificationEmailAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            // Send Email
            var userId = await _userManager.GetUserIdAsync(user);
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { area = "Identity", userId = userId, code = code },
                protocol: Request.Scheme);
            callbackUrl = HtmlEncoder.Default.Encode(callbackUrl);

            var emailTemplate = await _emailTemplateService.GetEmailTemplateByName("Confirm Email");
            emailTemplate.Template = emailTemplate.Template.Replace("$fullName", user.FullName);
            emailTemplate.Template = emailTemplate.Template.Replace("$callbackUrl", callbackUrl);

            _ = Task.Run(async () =>
            {
                await _emailService.SendEmail(new WebApp.Core.Model.EmailModel { To = Input.NewEmail, Subject = emailTemplate.Subject, Body = emailTemplate.Template });
            });

            StatusMessage = "Verification email sent. Please check your email.";
            return RedirectToPage();
        }
    }
}
