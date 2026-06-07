using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApp.User.Service;
using WebApp.Core.Model;
using WebApp.Core.Infrastructure;

namespace WebApp.User.UI.Pages.Appointment;

public partial class BookModel : PageModel
{
	[BindProperty]
	public AppointmentModel Appointment { get; set; }

	private readonly ILogger<BookModel> _logger;
	private readonly AppointmentService _appointmentService;

	public BookModel(ILogger<BookModel> logger, AppointmentService appointmentService)
	{
		this._logger = logger;
		this._appointmentService = appointmentService;
	}


	public Task<IActionResult> OnGet() =>
	TryCatch(async () =>
	{
		// For now, assume user id 1 (seeded patient). Later wire to auth context.
		return Page();
	});

	public Task<IActionResult> OnPost() =>
	TryCatch(async () =>
	{
		LogModel log = new LogModel { UserName = "patient1@example.com", IP = Utility.GetIPAddress(Request) };
		// In a real app, UserId is derived from identity; using 1 (seeded) for now
		Appointment.UserId = 1;
		int id = await _appointmentService.BookAppointment(Appointment, log);
		return RedirectToPage("/Appointment/List");
	});

	private delegate Task<IActionResult> ReturningFunction();
	private async Task<IActionResult> TryCatch(ReturningFunction returningFunction)
	{
		try { return await returningFunction(); }
		catch (Exception ex) { _ = Task.Run(() => { _logger.LogError(ex, ex.Message); }); return StatusCode(500); }
	}
}
