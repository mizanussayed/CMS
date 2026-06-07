using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApp.Admin.Service;
using Microsoft.AspNetCore.Authorization;
using WebApp.Core.Model;

namespace WebApp.Admin.UI.Pages.Doctor;

[Authorize(Roles = "SystemAdmin")]
public partial class ListModel : PageModel
{
    public List<DoctorModel> Doctors { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

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
        Doctors = paged.Items;
        return Page();
    });
}
