using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Admin.UI.Areas.Identity.Data;

public class MembershipDbContext : IdentityDbContext<ApplicationUser>
{
    public MembershipDbContext(DbContextOptions<MembershipDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Seeding a 'SystemAdmin' role to AspNetRoles table
        modelBuilder.Entity<IdentityRole>().HasData(
        new IdentityRole
        {
            Id = "1",
            Name = "SystemAdmin",
            NormalizedName = "SYSTEMADMIN".ToUpper()
        });

        // Seeding a user to AspNetUsers table
        modelBuilder.Entity<ApplicationUser>().HasData(
        new ApplicationUser
        {
            Id = "e1ae1f42-75b2-4604-97ec-10f844b1962f", // primary key
            UserName = "sharif2kb@yahoo.com",
            NormalizedUserName = "SHARIF2KB@YAHOO.COM",
            Email = "sharif2kb@yahoo.com",
            NormalizedEmail = "SHARIF2KB@YAHOO.COM",
            EmailConfirmed = true,
            PasswordHash = new PasswordHasher<ApplicationUser>().HashPassword(null, "123456"),
            PhoneNumber = "01712925546",
            PhoneNumberConfirmed = true,
            FullName = "Shariful Islam"
        });

        // Seeding relation between the user and role to AspNetUserRoles table
        modelBuilder.Entity<IdentityUserRole<string>>().HasData(
            new IdentityUserRole<string>
            {
                RoleId = "1",
                UserId = "e1ae1f42-75b2-4604-97ec-10f844b1962f"
            }
        );

        // Seeding a claim to AspNetUserClaims table
        modelBuilder.Entity<IdentityUserClaim<string>>().HasData(
            new IdentityUserClaim<string>
            {
                Id = 1,
                UserId = "e1ae1f42-75b2-4604-97ec-10f844b1962f",
                ClaimType = "FullName",
                ClaimValue = "Sharif Ahmed"
            }
        );
    }
}