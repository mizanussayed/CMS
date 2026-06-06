using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApp.Admin.Service;
using Microsoft.AspNetCore.Authorization;
using WebApp.Core.Model;
using WebApp.Core.Infrastructure;

namespace WebApp.Admin.UI.Pages.Category;

[Authorize(Roles = "SystemAdmin")]
public partial class ListModel : PageModel
{
    public List<CategoryModel> Categories { get; set; }

    // Pagination
    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;
    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }

    private readonly ILogger<ListModel> _logger;
    private readonly CategoryService _categoryService;

    public ListModel(ILogger<ListModel> logger, CategoryService categoryService)
    {
        this._logger = logger;
        this._categoryService = categoryService;
    }

    public Task<IActionResult> OnGet() =>
    TryCatch(async () =>
    {
        var paginatedList = await _categoryService.GetCategories(PageNumber);
        Categories = paginatedList.Items;
        TotalRecords = paginatedList.TotalRecords;
        TotalPages = paginatedList.TotalPages;
		PageNumber = paginatedList.PageIndex;
		return Page();
    });

    public Task<IActionResult> OnPostDelete(int id) =>
    TryCatch(async () =>
    {
        LogModel logModel = new LogModel();
        logModel.UserName = User.Identity.Name;
        logModel.UserRole = User.Claims.First(c => c.Type.Contains("role")).Value;
        logModel.IP = Utility.GetIPAddress(Request);

        await _categoryService.DeleteCategory(id, logModel);
        return RedirectToPage("/Category/List", new { c = "cat", p = "catl" });
    });

    public Task<IActionResult> OnPostExport() =>
    TryCatch(async () =>
    {
        var exportFile = await _categoryService.Export();
        return File(exportFile.Data, exportFile.ContentType, exportFile.FileName);
    });
}