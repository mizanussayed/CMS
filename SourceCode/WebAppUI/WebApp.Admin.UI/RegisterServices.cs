using AspNetCore.ReCaptcha;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebApp.Admin.Service;
using WebApp.Admin.Service.Base;
using WebApp.Admin.UI.Areas.Identity.Data;
using WebApp.Core.Infrastructure;

namespace WebApp.Admin.UI;

public static class RegisterServices
{
    public static void ConfigureServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddHttpClient("SMSAPI", c => { c.BaseAddress = new Uri(builder.Configuration.GetValue<string>("SMSSettings:SMSBaseAPIAddress")); });

        builder.Services.AddDbContext<MembershipDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("MembershipDatabase")));
        builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.SignIn.RequireConfirmedAccount = true;

            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequiredLength = 6;

            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.Lockout.MaxFailedAccessAttempts = 1000;
            options.Lockout.AllowedForNewUsers = true;

            options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+#";
            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<MembershipDbContext>()
        .AddDefaultUI()
        .AddDefaultTokenProviders();

        builder.Services.AddRazorPages();
        builder.Services.AddControllersWithViews(); // TODO: May be for Identity, not sure
        builder.Services.AddReCaptcha(builder.Configuration.GetSection("ReCaptcha"));
        builder.Services.AddMemoryCache();
        builder.Services.AddHttpClient("ServiceAPI", c => { c.BaseAddress = new Uri(builder.Configuration.GetValue<string>("BaseAPIAddress")); });

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<IContextAccessor, ContextAccessor>();
        builder.Services.AddSingleton<SecurityHelper>();
        builder.Services.AddScoped<EmailService>();
        builder.Services.AddScoped<SMSService>();
        builder.Services.AddScoped<AuditLogService>();
        builder.Services.AddScoped<ApplicationLogService>();
        builder.Services.AddScoped<AuthService>();
        builder.Services.AddScoped<CategoryService>();
        builder.Services.AddScoped<PieService>();
        builder.Services.AddScoped<EmailTemplateService>();
    }
}