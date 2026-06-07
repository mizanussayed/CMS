using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApp.Admin.Service;
using Microsoft.AspNetCore.Authorization;
using WebApp.Core.Model;

namespace WebApp.Admin.UI.Pages.Appointment;

[Authorize(Roles = "SystemAdmin")]
public partial class ListModel : PageModel
{
    public List<AppointmentModel> Appointments { get; set; }

    private readonly ILogger<ListModel> _logger;
    private readonly AppointmentService _appointmentService;

    public ListModel(ILogger<ListModel> logger, AppointmentService appointmentService)
    {
        this._logger = logger;
        this._appointmentService = appointmentService;
    }

    public Task<IActionResult> OnGet() =>
    TryCatch(async () =>
    {
        Appointments = await _appointmentService.GetAllAppointments();
        return Page();
    });

    private delegate Task<IActionResult> ReturningFunction();
    private async Task<IActionResult> TryCatch(ReturningFunction returningFunction)
    {
        try { return await returningFunction(); }
        catch (Exception ex) { _ = Task.Run(() => { _logger.LogError(ex, ex.Message); }); return StatusCode(500); }
    }
}
