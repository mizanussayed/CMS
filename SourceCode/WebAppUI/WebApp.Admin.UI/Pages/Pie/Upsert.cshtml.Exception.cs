using Microsoft.AspNetCore.Mvc;

namespace WebApp.Admin.UI.Pages.Pie;

public partial class UpsertModel
{
    private delegate Task<IActionResult> ReturningFunction();

    private async Task<IActionResult> TryCatch(ReturningFunction returningFunction)
    {
        try
        {
            return await returningFunction();
        }
        catch (Exception ex)
        {
            _ = Task.Run(() => { _logger.LogError(ex, ex.Message); });
            TempData["ErrorMessage"] = ex.Message;
            TempData["StackTrace"] = ex.StackTrace;
            return RedirectToPage("/Error");
        }
    }
}