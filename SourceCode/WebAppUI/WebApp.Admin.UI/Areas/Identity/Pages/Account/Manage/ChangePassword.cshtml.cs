using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApp.Admin.Service;
using WebApp.Admin.UI.Areas.Identity.Data;
using WebApp.Core.Model;
using WebApp.Core.Infrastructure;

namespace WebApp.Admin.UI.Areas.Identity.Pages.Account.Manage;

public class ChangePasswordModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ILogger<ChangePasswordModel> _logger;
    private readonly AuditLogService _auditLogService;
    private readonly EmailService _emailService;
    private readonly EmailTemplateService _emailTemplateService;

    public ChangePasswordModel(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ILogger<ChangePasswordModel> logger,
        AuditLogService auditLogService,
        EmailService emailService,
        EmailTemplateService emailTemplateService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
        _auditLogService = auditLogService;
        _emailService = emailService;
        _emailTemplateService = emailTemplateService;
    }

    [BindProperty]
    public InputModel Input { get; set; }

    [TempData]
    public string StatusMessage { get; set; }

    public class InputModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        var hasPassword = await _userManager.HasPasswordAsync(user);
        if (!hasPassword)
        {
            return RedirectToPage("./SetPassword");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        var changePasswordResult = await _userManager.ChangePasswordAsync(user, Input.OldPassword, Input.NewPassword);
        if (!changePasswordResult.Succeeded)
        {
            foreach (var error in changePasswordResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return Page();
        }

        await _signInManager.RefreshSignInAsync(user);
        _logger.LogInformation("User changed their password successfully.");
        StatusMessage = "Your password has been changed.";

        #region Audit Log
        var log = new LogModel
        {
            UserName = User.Identity.Name,
            UserRole = User.Claims.First(c => c.Type.Contains("role")).Value,
            IP = Utility.GetIPAddress(Request),
            TableName = "AspNetUsers",
            OldData = $"<deleted Id=\"{user.Id}\" Name=\"{user.UserName}\" Feature=\"ChangePassword\" />",
            NewData = $"<inserted Id=\"{user.Id}\" Name=\"{user.UserName}\" Feature=\"ChangePassword\" />"
        };
        _ = Task.Run(async () => { await _auditLogService.InsertAuditLog(log); });
        #endregion

        #region Send Email
        var emailTemplate = await _emailTemplateService.GetEmailTemplateByName("Reset Password");
        emailTemplate.Template = emailTemplate.Template.Replace("$fullName", user.FullName);
        emailTemplate.Template = emailTemplate.Template.Replace("$password", Input.NewPassword);

        _ = Task.Run(async () =>
        {
            await _emailService.SendEmail(new WebApp.Core.Model.EmailModel { To = user.Email, Subject = emailTemplate.Subject, Body = emailTemplate.Template });
        });
        #endregion

        return RedirectToPage();
    }
}