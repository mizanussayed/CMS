using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApp.Admin.Service;
using Microsoft.AspNetCore.Authorization;

namespace WebApp.Admin.UI.Pages.Report;

[Authorize(Roles = "SystemAdmin")]
public partial class ApplicationLogModel : PageModel
{
    public List<WebApp.Core.Model.ApplicationLogModel> ApplicationLogs { get; set; }

    // Pagination
    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;
    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }

    private readonly ILogger<ApplicationLogModel> _logger;
    private readonly ApplicationLogService _applicationLogService;

    public ApplicationLogModel(ILogger<ApplicationLogModel> logger, ApplicationLogService applicationLogService)
    {
        this._logger = logger;
        this._applicationLogService = applicationLogService;
    }

    public Task<IActionResult> OnGet() =>
    TryCatch(async () =>
    {
        var paginatedList = await _applicationLogService.GetApplicationLogs(PageNumber);
        ApplicationLogs = paginatedList.Items;
        TotalRecords = paginatedList.TotalRecords;
        TotalPages = paginatedList.TotalPages;
		PageNumber = paginatedList.PageIndex;
		return Page();
    });

    public Task<IActionResult> OnPostDelete(int id) =>
    TryCatch(async () =>
    {
        await _applicationLogService.DeleteApplicationLog(id);
        return RedirectToPage("/Report/ApplicationLog", new { c = "rpt", p = "appl" });
    });

    public Task<IActionResult> OnPostExport() =>
    TryCatch(async () =>
    {
        var exportFile = await _applicationLogService.Export();
        return File(exportFile.Data, exportFile.ContentType, exportFile.FileName);
    });
}