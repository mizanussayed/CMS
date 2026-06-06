using Microsoft.AspNetCore.Identity;

namespace WebApp.API.Persistence.Identity;

public class ApplicationUser : IdentityUser
{
	public string FullName { get; set; }
}