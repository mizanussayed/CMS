using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Admin.UI.Pages;

[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
[IgnoreAntiforgeryToken]
public class ErrorModel : PageModel
{
    public string ErrorMessage { get; set; }
    public string StackTrace { get; set; }

    private ILogger<ErrorModel> _logger;

    public ErrorModel(ILogger<ErrorModel> logger)
    {
        this._logger = logger;
    }

    public async void OnGet()
    {
        this.ErrorMessage = (TempData["ErrorMessage"] == null) ? "" : TempData["ErrorMessage"].ToString();
        this.StackTrace = (TempData["StackTrace"] == null) ? "" : TempData["StackTrace"].ToString();

        await Task.Run(() => { _logger.LogError("{ErrorMessage}: " + Environment.NewLine + this.StackTrace, this.ErrorMessage); });
    }
}