using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApp.Core.Model;
using WebApp.User.Service;

namespace WebApp.User.UI.Pages.Category;

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
		this.Categories = new List<CategoryModel>();
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

    public Task<IActionResult> OnPostExport() =>
    TryCatch(async () =>
    {
        var exportFile = await _categoryService.Export();
        return File(exportFile.Data, exportFile.ContentType, exportFile.FileName);
    });
}