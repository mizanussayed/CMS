using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApp.User.Service;
using WebApp.Core.Model;

namespace WebApp.User.UI.Pages.Appointment;

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
        // For now, assume user id 1 (seeded patient). Later wire to auth context.
        Appointments = await _appointmentService.GetMyAppointments(1);
        return Page();
    });

    // Helper TryCatch for consistency with other pages
    private delegate Task<IActionResult> ReturningFunction();
    private async Task<IActionResult> TryCatch(ReturningFunction returningFunction)
    {
        try { return await returningFunction(); }
        catch (Exception ex) { _ = Task.Run(() => { _logger.LogError(ex, ex.Message); }); return StatusCode(500); }
    }
}
