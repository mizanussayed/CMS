using WebApp.Admin.UI;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, config) =>
{
    config.ReadFrom.Configuration(context.Configuration);
});

builder.ConfigureServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (!app.Environment.IsDevelopment())
//{
//app.UseExceptionHandler("/Error");
app.UseDeveloperExceptionPage();
app.UseHsts();
//}
app.UseHttpsRedirection();

app.UseStaticFiles();
//app.UseSerilogRequestLogging(); // Generates entry like this: HTTP "GET" "/" responded 200 in 265.8149 ms
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();

app.Run();