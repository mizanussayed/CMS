using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApp.Admin.Service;
using Microsoft.AspNetCore.Authorization;
using WebApp.Core.Model;

namespace WebApp.Admin.UI.Pages.Settings.EmailTemplate;

[Authorize(Roles = "SystemAdmin")]
public partial class ListModel : PageModel
{
    public List<EmailTemplateModel> EmailTemplates { get; set; }

    // Pagination
    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;
    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }

    private readonly ILogger<ListModel> _logger;
    private readonly EmailTemplateService _emailTemplateService;

    public ListModel(ILogger<ListModel> logger, EmailTemplateService emailTemplateService)
    {
        this._logger = logger;
        this._emailTemplateService = emailTemplateService;
    }

    public Task<IActionResult> OnGet() =>
    TryCatch(async () =>
    {
        var paginatedList = await _emailTemplateService.GetEmailTemplates(PageNumber);
        EmailTemplates = paginatedList.Items;
        TotalRecords = paginatedList.TotalRecords;
        TotalPages = paginatedList.TotalPages;
		PageNumber = paginatedList.PageIndex;
		return Page();
    });
}