using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WebApp.API.Endpoint.Resources;
using System;
using System.Threading.Tasks;

namespace WebApp.API.Endpoint.Controllers.V2;

public partial class CategoryController
{
	private delegate Task<IActionResult> ReturningFunction();
	private string Messages = "";

	private async Task<IActionResult> TryCatch(ReturningFunction returningFunction)
	{
		try
		{
			return await returningFunction();
		}
		catch (Exception ex)
		{
			_ = Task.Run(() => { _logger.LogError(ex, ex.Message); });

			if (returningFunction.Method.Name.Contains("GetCategories"))
				Messages = ExceptionMessages.Category_List;

			if (returningFunction.Method.Name.Contains("GetDistinctCategories"))
				Messages = ExceptionMessages.Category_List;

			if (returningFunction.Method.Name.Contains("GetCategoryById"))
				Messages = ExceptionMessages.Category_Id;

			if (returningFunction.Method.Name.Contains("InsertCategory"))
				Messages = ExceptionMessages.Category_Insert;

			if (returningFunction.Method.Name.Contains("UpdateCategory"))
				Messages = ExceptionMessages.Category_Update;

			if (returningFunction.Method.Name.Contains("DeleteCategory"))
				Messages = ExceptionMessages.Category_Delete;

			if (returningFunction.Method.Name.Contains("GetCategoriesWithPies"))
				Messages = ExceptionMessages.Category_CategoriesWithPies;

			if (returningFunction.Method.Name.Contains("Export"))
				Messages = ExceptionMessages.Category_List;

			return StatusCode(StatusCodes.Status500InternalServerError, Messages);
		}
		finally
		{
			// Do clean up code here, if needed.
		}
	}
}