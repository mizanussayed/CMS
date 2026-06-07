using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApp.User.Service;
using WebApp.Core.Model;
using WebApp.Core.Infrastructure;

namespace WebApp.User.UI.Pages.Appointment;

public partial class ListModel : PageModel
{
    public List<AppointmentModel>? Appointments { get; set; }
    public Dictionary<int, string> DoctorNames { get; set; } = new();

    private readonly ILogger<ListModel> _logger;
    private readonly AppointmentService _appointmentService;
    private readonly EmailService _emailService;

    public ListModel(ILogger<ListModel> logger, AppointmentService appointmentService, EmailService emailService)
    {
        this._logger = logger;
        this._appointmentService = appointmentService;
        this._emailService = emailService;
    }

    public Task<IActionResult> OnGet() =>
    TryCatch(async () =>
    {
        // UserId = 1 (seeded patient); in a real app derive from identity
        Appointments = await _appointmentService.GetMyAppointments(1);
        await BuildDoctorMap();
        return Page();
    });

    public Task<IActionResult> OnPostCancel(int appointmentId) =>
    TryCatch(async () =>
    {
        LogModel log = new LogModel { UserName = "patient1@example.com", UserRole = "Patient", IP = Utility.GetIPAddress(Request) };
        await _appointmentService.CancelAppointment(appointmentId, log);

        _ = _emailService.SendEmailAsync(
            to: "patient1@example.com",
            subject: "Appointment Cancelled",
            body: $"Your appointment (ID: {appointmentId}) has been cancelled."
        );

        TempData["Success"] = $"Appointment #{appointmentId} has been cancelled.";
        return RedirectToPage();
    });

    private async Task BuildDoctorMap()
    {
        try
        {
            var doctors = await _appointmentService.GetDoctors(1);
            DoctorNames = doctors.ToDictionary(d => d.DoctorID, d => d.Name);
        }
        catch { /* graceful degradation – show ID if lookup fails */ }
    }

    private delegate Task<IActionResult> ReturningFunction();
    private async Task<IActionResult> TryCatch(ReturningFunction returningFunction)
    {
        try { return await returningFunction(); }
        catch (Exception ex) { _ = Task.Run(() => _logger.LogError(ex, ex.Message)); return StatusCode(500); }
    }
}
