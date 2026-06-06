using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApp.Admin.Service;
using WebApp.Admin.UI.Areas.Identity.Data;
using WebApp.Core.Model;
using WebApp.Core.Infrastructure;

namespace WebApp.Admin.UI.Areas.Identity.Pages.Account.Manage
{
    [Authorize(Roles = "SystemAdmin")]
    public partial class UserManagementModel : PageModel
    {
        public IList<ApplicationUser> Users { get; set; }

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly AuditLogService _auditLogService;

        public UserManagementModel(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, AuditLogService auditLogService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _auditLogService = auditLogService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            Users = await _userManager.GetUsersInRoleAsync("SystemAdmin");
            Users = this.Users.OrderBy(x => x.UserName).ToList();
            return Page();
        }

        public async Task<IActionResult> OnPostLockUnlock(string id)
        {
            var oldData = "";
            var newData = "";

            var user = await _userManager.FindByIdAsync(id);

            if (user.LockoutEnd != null && user.LockoutEnd > DateTime.Now)
            {
                user.LockoutEnd = DateTime.Now;
                oldData = $"<deleted Id=\"{user.Id}\" Name=\"{user.UserName}\" Locked=\"True\" />";
                newData = $"<inserted Id=\"{user.Id}\" Name=\"{user.UserName}\" Locked=\"False\" />";
            }
            else
            {
                user.LockoutEnd = DateTime.Now.AddYears(100);
                oldData = $"<deleted Id=\"{user.Id}\" Name=\"{user.UserName}\" Locked=\"False\" />";
                newData = $"<inserted Id=\"{user.Id}\" Name=\"{user.UserName}\" Locked=\"True\" />";
            }

            await _userManager.UpdateAsync(user);

            // Audit Log
            var log = new LogModel
            {
                UserName = User.Identity.Name,
                UserRole = User.Claims.First(c => c.Type.Contains("role")).Value,
                IP = Utility.GetIPAddress(Request),
                TableName = "AspNetUsers",
                OldData = oldData,
                NewData = newData
            };
            _ = Task.Run(async () => { await _auditLogService.InsertAuditLog(log); });

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDelete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            await _userManager.DeleteAsync(user);

            // Audit Log
            var log = new LogModel
            {
                UserName = User.Identity.Name,
                UserRole = User.Claims.First(c => c.Type.Contains("role")).Value,
                IP = Utility.GetIPAddress(Request),
                TableName = "AspNetUsers",
                OldData = $"<deleted Id=\"{user.Id}\" Name=\"{user.UserName}\" />",
                NewData = null
            };
            _ = Task.Run(async () => { await _auditLogService.InsertAuditLog(log); });

            return RedirectToPage();
        }
    }
}