using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebApp.Core.Model;
using WebApp.User.Service;
using System.ComponentModel;

namespace WebApp.User.UI.Pages.Pie;

public partial class ListModel : PageModel
{
    [BindProperty, DisplayName("Category")]
    public int CategoryId { get; set; }
    public SelectList? SelectList { get; set; }

    public List<PieModel> Pies { get; set; }

    // Pagination
    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;
    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }

    private readonly ILogger<ListModel> _logger;
    private readonly PieService _pieService;
    private readonly CategoryService _categoryService;

    public ListModel(ILogger<ListModel> logger, PieService pieService, CategoryService categoryService)
    {
        this._logger = logger;
        this._pieService = pieService;
        this._categoryService = categoryService;
		this.Pies = new List<PieModel>();
    }

    public Task<IActionResult> OnGet() =>
    TryCatch(async () =>
    {
        SelectList = new SelectList(await _categoryService.GetDistinctCategories(), nameof(CategoryModel.Id), nameof(CategoryModel.Name));

        var paginatedList = await _pieService.GetPies(PageNumber);
        Pies = paginatedList.Items;
        TotalRecords = paginatedList.TotalRecords;
        TotalPages = paginatedList.TotalPages;
		PageNumber = paginatedList.PageIndex;

		return Page();
    });

    public Task<IActionResult> OnPostFilter() =>
    TryCatch(async () =>
    {
        SelectList = new SelectList(await _categoryService.GetDistinctCategories(), nameof(CategoryModel.Id), nameof(CategoryModel.Name));
        
        Pies = await _pieService.GetPieByCategoryId(CategoryId);
        TotalRecords = Pies.Count;

        return Page();
    });

    public Task<IActionResult> OnPostExport() =>
    TryCatch(async () =>
    {
        var exportFile = await _pieService.Export();
        return File(exportFile.Data, exportFile.ContentType, exportFile.FileName);
    });
}