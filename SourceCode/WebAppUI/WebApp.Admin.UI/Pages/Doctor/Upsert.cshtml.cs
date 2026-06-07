using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApp.Admin.Service;
using Microsoft.AspNetCore.Authorization;
using WebApp.Core.Model;
using WebApp.Core.Infrastructure;

namespace WebApp.Admin.UI.Pages.Doctor;

[Authorize(Roles = "SystemAdmin")]
public partial class UpsertModel : PageModel
{
    [BindProperty]
    public DoctorModel Doctor { get; set; }

    private readonly ILogger<UpsertModel> _logger;
    private readonly DoctorService _doctorService;

    public UpsertModel(ILogger<UpsertModel> logger, DoctorService doctorService)
    {
        this._logger = logger;
        this._doctorService = doctorService;
    }

    public Task<IActionResult> OnGet(int id) =>
    TryCatch(async () =>
    {
        Doctor = (id == 0) ? new DoctorModel() : await _doctorService.GetDoctorById(id);
        return Page();
    });

    public Task<IActionResult> OnPost() =>
    TryCatch(async () =>
    {
        LogModel log = new LogModel { UserName = User.Identity.Name, UserRole = User.Claims.First(c => c.Type.Contains("role")).Value, IP = Utility.GetIPAddress(Request) };

        if (Doctor.DoctorID == 0)
        {
            await _doctorService.InsertDoctor(Doctor, log);
        }
        else
        {
            await _doctorService.UpdateDoctor(Doctor.DoctorID, Doctor, log);
        }

        return RedirectToPage("/Doctor/List");
    });

    private delegate Task<IActionResult> ReturningFunction();
    private async Task<IActionResult> TryCatch(ReturningFunction returningFunction)
    {
        try { return await returningFunction(); }
        catch (Exception ex) { _ = Task.Run(() => { _logger.LogError(ex, ex.Message); }); return StatusCode(500); }
    }
}
