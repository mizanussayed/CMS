using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WebApp.API.Endpoint.Resources;
using System;
using System.Threading.Tasks;

namespace WebApp.API.Endpoint.Controllers.V2;

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

            if (returningFunction.Method.Name.Contains("GetDoctors"))
                Messages = ExceptionMessages.Doctor_List;

            if (returningFunction.Method.Name.Contains("GetDoctorById"))
                Messages = ExceptionMessages.Doctor_Id;

            if (returningFunction.Method.Name.Contains("InsertDoctor"))
                Messages = ExceptionMessages.Doctor_Insert;

            if (returningFunction.Method.Name.Contains("UpdateDoctor"))
                Messages = ExceptionMessages.Doctor_Update;

            return StatusCode(StatusCodes.Status500InternalServerError, Messages);
        }
        finally
        {
            // Do clean up code here, if needed.
        }
    }
}
