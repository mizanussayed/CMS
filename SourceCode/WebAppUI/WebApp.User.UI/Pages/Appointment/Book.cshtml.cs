using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApp.User.Service;
using WebApp.Core.Model;
using WebApp.Core.Infrastructure;

namespace WebApp.User.UI.Pages.Appointment;

public partial class BookModel : PageModel
{
    [BindProperty]
    public AppointmentModel Appointment { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public int? DoctorId { get; set; }

    public List<DoctorModel> Doctors { get; set; } = new();
    public DoctorModel? SelectedDoctor { get; set; }

    private readonly ILogger<BookModel> _logger;
    private readonly AppointmentService _appointmentService;
    private readonly EmailService _emailService;

    public BookModel(ILogger<BookModel> logger, AppointmentService appointmentService, EmailService emailService)
    {
        this._logger = logger;
        this._appointmentService = appointmentService;
        this._emailService = emailService;
    }

    public Task<IActionResult> OnGet() =>
    TryCatch(async () =>
    {
        Doctors = await _appointmentService.GetDoctors(1);
        if (DoctorId.HasValue && DoctorId.Value > 0)
        {
            SelectedDoctor = Doctors.FirstOrDefault(d => d.DoctorID == DoctorId.Value);
            Appointment.DoctorId = DoctorId.Value;
        }
        return Page();
    });

    public Task<IActionResult> OnPost() =>
    TryCatch(async () =>
    {
        if (!ModelState.IsValid)
        {
            Doctors = await _appointmentService.GetDoctors(1);
            return Page();
        }

        LogModel log = new LogModel { UserName = "patient1@example.com", UserRole = "Patient", IP = Utility.GetIPAddress(Request) };
        // UserId 1 = seeded patient; in a real app this comes from identity context
        Appointment.UserId = 1;
        int id = await _appointmentService.BookAppointment(Appointment, log);

        // Send email notification (non-blocking)
        _ = _emailService.SendEmailAsync(
            to: "patient1@example.com",
            subject: "Appointment Confirmed",
            body: $"Your appointment (ID: {id}) has been booked for {Appointment.AppointmentDate:dddd, dd MMMM yyyy HH:mm}. Status: Pending."
        );

        TempData["Success"] = $"Appointment booked successfully!";
        return RedirectToPage("/Appointment/List");
    });

    private delegate Task<IActionResult> ReturningFunction();
    private async Task<IActionResult> TryCatch(ReturningFunction returningFunction)
    {
        try { return await returningFunction(); }
        catch (Exception ex) { _ = Task.Run(() => _logger.LogError(ex, ex.Message)); return StatusCode(500); }
    }
}
