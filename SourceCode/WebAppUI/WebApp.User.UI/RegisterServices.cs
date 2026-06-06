using WebApp.Core.Infrastructure;
using WebApp.User.Service;

namespace WebApp.User.UI;

public static class RegisterServices
{
    public static void ConfigureServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddRazorPages();
        builder.Services.AddMemoryCache();
        builder.Services.AddHttpClient("ServiceAPI", c => { c.BaseAddress = new Uri(builder.Configuration.GetValue<string>("BaseAPIAddress")); });

        builder.Services.AddSingleton<SecurityHelper>();
        builder.Services.AddScoped<CategoryService>();
        builder.Services.AddScoped<PieService>();
    }
}