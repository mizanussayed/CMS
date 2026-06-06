using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WebApp.API.Endpoint.Resources;
using System;
using System.Threading.Tasks;

namespace WebApp.API.Endpoint.Controllers.V1;

public partial class PieController
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

			if (returningFunction.Method.Name.Contains("GetPies"))
				Messages = ExceptionMessages.Pie_List;

			if (returningFunction.Method.Name.Contains("GetPieById"))
				Messages = ValidationMessages.Pie_NotFoundId;

			if (returningFunction.Method.Name.Contains("GetPieByCategoryId"))
				Messages = ExceptionMessages.Pie_CategoryId;

			if (returningFunction.Method.Name.Contains("InsertPie"))
				Messages = ExceptionMessages.Pie_Insert;

			if (returningFunction.Method.Name.Contains("UpdatePie"))
				Messages = ExceptionMessages.Pie_Update;

			if (returningFunction.Method.Name.Contains("DeletePie"))
				Messages = ExceptionMessages.Pie_Delete;

			if (returningFunction.Method.Name.Contains("Export"))
				Messages = ExceptionMessages.Pie_List;

			return StatusCode(StatusCodes.Status500InternalServerError, Messages);
		}
		finally
		{
			// Do clean up code here, if needed.
		}
	}
}