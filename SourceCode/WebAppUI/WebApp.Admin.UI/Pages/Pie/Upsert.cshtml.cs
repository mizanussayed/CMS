using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApp.Core.Model;
using WebApp.Admin.Service;
using System.ComponentModel;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebApp.Core.Resources;
using Microsoft.AspNetCore.Authorization;
using WebApp.Core.Infrastructure;

namespace WebApp.Admin.UI.Pages.Pie;

[Authorize(Roles = "SystemAdmin")]
public partial class UpsertModel : PageModel
{
    [BindProperty]
    public PieModel Pie { get; set; }

    [BindProperty, DisplayName("Expiry Date")]
    public string ExpiryDate { get; set; }

    [BindProperty, DisplayName("Pie Image")]
    public IFormFile? PieImage { get; set; }

    public string ErrorMessage { get; set; }
    public string SuccessMessage { get; set; }
    public string ImageURL { get; set; }
    public SelectList CategorySelectList { get; set; }

    private readonly ILogger<UpsertModel> _logger;
    private readonly IConfiguration _config;
    private readonly PieService _pieService;
    private readonly CategoryService _categoryService;

    public UpsertModel(ILogger<UpsertModel> logger, IConfiguration config, PieService pieService, CategoryService categoryService)
    {
        this._logger = logger;
        this._config = config;
        this._pieService = pieService;
        this._categoryService = categoryService;
    }

    public Task<IActionResult> OnGet(int id) =>
    TryCatch(async () =>
    {
        if (id == 0)
        {
            Pie = new PieModel();
        }
        else
        {
            Pie = await _pieService.GetPieById(id);
        }

        await PopulatePageElements();
        return Page();
    });

    public Task<IActionResult> OnPost() =>
    TryCatch(async () =>
    {
        if (await ValidatePost() == false)
        {
            await PopulatePageElements();
            return Page();
        }

        if (PieImage != null) Pie.ImageUrl = await UploadFile();
        Pie.ExpiryDate = dateExpiryDate;

        LogModel logModel = new LogModel();
        logModel.UserName = User.Identity.Name;
        logModel.UserRole = User.Claims.First(c => c.Type.Contains("role")).Value;
        logModel.IP = Utility.GetIPAddress(Request);

        if (Pie.Id == 0)
        {
            Pie.CreatedBy = User.Identity.Name;
            await _pieService.InsertPie(Pie, logModel);
            SuccessMessage = InformationMessages.Saved;
        }
        else
        {
            Pie.LastModifiedBy = User.Identity.Name;
            await _pieService.UpdatePie(Pie.Id, Pie, logModel);
            SuccessMessage = InformationMessages.Updated;
        }

        await PopulatePageElements();
        return Page();
    });

    #region "Helper Methods"
    private async Task PopulatePageElements()
    {
        CategorySelectList = new SelectList(await _categoryService.GetDistinctCategories(), nameof(CategoryModel.Id), nameof(CategoryModel.Name));
        ImageURL = _config["UserRootURL"] + "images/" + (string.IsNullOrEmpty(Pie.ImageUrl) ? "NoImage.jpg" : "pie/" + Pie.ImageUrl);
    }

    private async Task<string> UploadFile()
    {
        string uploadFolder = "";
        string uniqueFileName = "";
        string filePath = "";

        uploadFolder = Path.Combine(_config["UserRootPath"], "images\\pie");

        // Delete existing image
        if (!string.IsNullOrEmpty(Pie.ImageUrl))
            System.IO.File.Delete(Path.Combine(uploadFolder, Pie.ImageUrl));

        // Upload new image
        uniqueFileName = Guid.NewGuid().ToString() + "-" + PieImage.FileName;
        filePath = Path.Combine(uploadFolder, uniqueFileName);
        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await PieImage.CopyToAsync(fileStream);
        }

        return uniqueFileName;
    }
    #endregion
}