using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApp.Admin.Service;
using Microsoft.AspNetCore.Authorization;
using WebApp.Core.Model;

namespace WebApp.Admin.UI.Pages.Report;

[Authorize(Roles = "SystemAdmin")]
public partial class AuditLogModel : PageModel
{
    public List<LogModel> AuditLogs { get; set; }

    // Pagination
    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;
    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }

    private readonly ILogger<AuditLogModel> _logger;
    private readonly AuditLogService _auditLogService;

    public AuditLogModel(ILogger<AuditLogModel> logger, AuditLogService auditLogService)
    {
        this._logger = logger;
        this._auditLogService = auditLogService;
    }

    public Task<IActionResult> OnGet() =>
    TryCatch(async () =>
    {
        var paginatedList = await _auditLogService.GetAuditLogs(PageNumber);
        AuditLogs = paginatedList.Items;
        TotalRecords = paginatedList.TotalRecords;
        TotalPages = paginatedList.TotalPages;
		PageNumber = paginatedList.PageIndex;
		return Page();
    });

    public Task<IActionResult> OnPostDelete(int id) =>
    TryCatch(async () =>
    {
        await _auditLogService.DeleteAuditLog(id);
        return RedirectToPage("/Report/AuditLog", new { c = "rpt", p = "adtl" });
    });

    public Task<IActionResult> OnPostExport() =>
    TryCatch(async () =>
    {
        var exportFile = await _auditLogService.Export();
        return File(exportFile.Data, exportFile.ContentType, exportFile.FileName);
    });
}