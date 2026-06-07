using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace WebApp.API.Endpoint.Controllers.V1;

public partial class DoctorController
{
    private delegate Task<IActionResult> ReturningFunction();
    private string Messages = "";

    private async Task<IActionResult> TryCatch(ReturningFunction returningFunction)
    {
        try
        {
            return await returningFunction();
        }
        catch (Exception ex)
        {
            _ = Task.Run(() => { _logger.LogError(ex, ex.Message); });
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing the request.");
        }
        finally
        {
        }
    }
}
