using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApp.Core.Model;
using WebApp.User.Service;

namespace WebApp.User.UI.Pages.Pie;

public partial class DisplayModel : PageModel
{
    public PieModel Pie { get; set; }
    public string ImageURL { get; set; }

    private readonly ILogger<DisplayModel> _logger;
    private readonly PieService _pieService;

    public DisplayModel(ILogger<DisplayModel> logger, PieService pieService)
    {
        this._logger = logger;
        this._pieService = pieService;
		this.Pie = new PieModel();
		this.ImageURL = "";

	}

    public Task<IActionResult> OnGet(int id) =>
    TryCatch(async () =>
    {
        Pie = await _pieService.GetPieById(id);
        ImageURL = Path.Combine("../../images/", (string.IsNullOrEmpty(Pie.ImageUrl) ? "NoImage.jpg" : "pie/" + Pie.ImageUrl));
        ViewData["Title"] = Pie.Name;
        return Page();
    });
}