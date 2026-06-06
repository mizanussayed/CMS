using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApp.Core.Model;
using WebApp.Core.Resources;
using WebApp.Admin.Service;
using Microsoft.AspNetCore.Authorization;
using WebApp.Core.Infrastructure;

namespace WebApp.Admin.UI.Pages.Category;

[Authorize(Roles = "SystemAdmin")]
public partial class UpsertModel : PageModel
{
    [BindProperty]
    public CategoryModel Category { get; set; }

    public string ErrorMessage { get; set; }
    public string SuccessMessage { get; set; }

    private readonly ILogger<UpsertModel> _logger;
    private readonly CategoryService _categoryService;

    public UpsertModel(ILogger<UpsertModel> logger, CategoryService categoryService)
    {
        this._logger = logger;
        this._categoryService = categoryService;
    }

    public Task<IActionResult> OnGet(int id) =>
    TryCatch(async () =>
    {
        if (id == 0)
        {
            Category = new CategoryModel();
        }
        else
        {
            Category = await _categoryService.GetCategoryById(id);
        }

        return Page();
    });

    public Task<IActionResult> OnPost() =>
    TryCatch(async () =>
    {
        if (await ValidatePost() == false) return Page();

        LogModel logModel = new LogModel();
        logModel.UserName = User.Identity.Name;
        logModel.UserRole = User.Claims.First(c => c.Type.Contains("role")).Value;
        logModel.IP = Utility.GetIPAddress(Request);

        if (Category.Id == 0)
        {
            Category.CreatedBy = User.Identity.Name;
            await _categoryService.InsertCategory(Category, logModel);
            SuccessMessage = InformationMessages.Saved;
        }
        else
        {
            Category.LastModifiedBy = User.Identity.Name;
            await _categoryService.UpdateCategory(Category.Id, Category, logModel);
            SuccessMessage = InformationMessages.Updated;
        }

        return Page();
    });
}