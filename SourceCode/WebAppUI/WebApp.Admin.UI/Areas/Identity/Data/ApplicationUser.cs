using Microsoft.AspNetCore.Identity;

namespace WebApp.Admin.UI.Areas.Identity.Data;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; }
}