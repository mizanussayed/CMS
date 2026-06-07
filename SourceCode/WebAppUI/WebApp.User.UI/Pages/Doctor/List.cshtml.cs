using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApp.User.Service;
using WebApp.Core.Model;

namespace WebApp.User.UI.Pages.Doctor;

public class ListModel : PageModel
{
    public List<DoctorModel> Doctors { get; set; } = new();
    public int TotalPages { get; set; }
    public bool HasPreviousPage { get; set; }
    public bool HasNextPage { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public string? SpecializationFilter { get; set; }

    public List<string> Specializations { get; set; } = new();

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
        var paged = await _appointmentService.GetDoctors(PageNumber);
        var all = paged;

        if (!string.IsNullOrWhiteSpace(SpecializationFilter))
            all = all.Where(d => d.Specialization == SpecializationFilter).ToList();

        Doctors = all;
        TotalPages = paged.Count > 0 ? (int)Math.Ceiling(paged.Count / 5.0) : 1;
        HasPreviousPage = PageNumber > 1;
        HasNextPage = PageNumber < TotalPages;

        // Build distinct specialization list for the filter dropdown
        Specializations = paged
            .Where(d => !string.IsNullOrWhiteSpace(d.Specialization))
            .Select(d => d.Specialization!)
            .Distinct()
            .OrderBy(s => s)
            .ToList();

        return Page();
    });

    private delegate Task<IActionResult> ReturningFunction();
    private async Task<IActionResult> TryCatch(ReturningFunction returningFunction)
    {
        try { return await returningFunction(); }
        catch (Exception ex) { _ = Task.Run(() => _logger.LogError(ex, ex.Message)); return StatusCode(500); }
    }
}
