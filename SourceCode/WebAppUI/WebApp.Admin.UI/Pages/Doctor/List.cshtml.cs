using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApp.Admin.Service;
using Microsoft.AspNetCore.Authorization;
using WebApp.Core.Model;
using WebApp.Core.Infrastructure;

namespace WebApp.Admin.UI.Pages.Doctor;

[Authorize(Roles = "SystemAdmin")]
public partial class ListModel : PageModel
{
    public List<DoctorModel> Doctors { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public string? SpecializationFilter { get; set; }

    public int TotalPages { get; set; }
    public int TotalRecords { get; set; }
    public bool HasPreviousPage { get; set; }
    public bool HasNextPage { get; set; }
    public List<string> Specializations { get; set; } = new();

    private readonly ILogger<ListModel> _logger;
    private readonly DoctorService _doctorService;

    public ListModel(ILogger<ListModel> logger, DoctorService doctorService)
    {
        this._logger = logger;
        this._doctorService = doctorService;
    }

    public Task<IActionResult> OnGet() =>
    TryCatch(async () =>
    {
        var paged = await _doctorService.GetDoctors(PageNumber);
        TotalRecords = paged.TotalRecords;
        TotalPages = paged.TotalPages;
        HasPreviousPage = paged.HasPreviousPage;
        HasNextPage = paged.HasNextPage;

        Doctors = paged.Items ?? new();

        // Build distinct specializations from the current page items for filter dropdown
        // (In production this would come from a dedicated endpoint)
        Specializations = Doctors
            .Where(d => !string.IsNullOrWhiteSpace(d.Specialization))
            .Select(d => d.Specialization!)
            .Distinct()
            .OrderBy(s => s)
            .ToList();

        if (!string.IsNullOrWhiteSpace(SpecializationFilter))
            Doctors = Doctors.Where(d => d.Specialization == SpecializationFilter).ToList();

        return Page();
    });

    public Task<IActionResult> OnPostDelete(int doctorId) =>
    TryCatch(async () =>
    {
        LogModel log = new LogModel { UserName = User.Identity!.Name, UserRole = "SystemAdmin", IP = Utility.GetIPAddress(Request) };
        await _doctorService.DeleteDoctor(doctorId, log);
        TempData["Success"] = $"Doctor #{doctorId} deleted successfully.";
        return RedirectToPage();
    });

    private delegate Task<IActionResult> ReturningFunction();
    private async Task<IActionResult> TryCatch(ReturningFunction returningFunction)
    {
        try { return await returningFunction(); }
        catch (Exception ex) { _ = Task.Run(() => _logger.LogError(ex, ex.Message)); return StatusCode(500); }
    }
}
