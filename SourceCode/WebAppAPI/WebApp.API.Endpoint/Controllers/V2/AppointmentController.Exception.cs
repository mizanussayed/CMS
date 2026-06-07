using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WebApp.API.Endpoint.Resources;
using System;
using System.Threading.Tasks;

namespace WebApp.API.Endpoint.Controllers.V2;

public partial class AppointmentController
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

            if (returningFunction.Method.Name.Contains("BookAppointment"))
                Messages = ExceptionMessages.Appointment_Insert;

            if (returningFunction.Method.Name.Contains("GetAllAppointments"))
                Messages = ExceptionMessages.Appointment_GetAll;

            if (returningFunction.Method.Name.Contains("GetAppointmentsByUser"))
                Messages = ExceptionMessages.Appointment_GetByUser;

            if (returningFunction.Method.Name.Contains("UpdateAppointmentStatus"))
                Messages = ExceptionMessages.Appointment_UpdateStatus;

            if (returningFunction.Method.Name.Contains("CancelAppointment"))
                Messages = ExceptionMessages.Appointment_Cancel;

            return StatusCode(StatusCodes.Status500InternalServerError, Messages);
        }
        finally
        {
            // Do clean up code here, if needed.
        }
    }
}
