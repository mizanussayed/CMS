using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace WebApp.API.Persistence.Identity;

public class MembershipDbContext : IdentityDbContext<ApplicationUser>
{
	public MembershipDbContext(DbContextOptions<MembershipDbContext> options) : base(options)
	{
	}
}