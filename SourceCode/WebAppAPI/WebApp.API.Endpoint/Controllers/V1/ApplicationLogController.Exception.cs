using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WebApp.API.Endpoint.Resources;
using System;
using System.Threading.Tasks;

namespace WebApp.API.Endpoint.Controllers.V1;

public partial class ApplicationLogController
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

			if (returningFunction.Method.Name.Contains("GetApplicationLogs"))
				Messages = ExceptionMessages.ApplicationLog_List;

			if (returningFunction.Method.Name.Contains("GetApplicationLogById"))
				Messages = ExceptionMessages.ApplicationLog_Id;

			if (returningFunction.Method.Name.Contains("DeleteApplicationLog"))
				Messages = ExceptionMessages.ApplicationLog_Delete;

			if (returningFunction.Method.Name.Contains("Export"))
				Messages = ExceptionMessages.ApplicationLog_List;

			return StatusCode(StatusCodes.Status500InternalServerError, Messages);
		}
		finally
		{
			// Do clean up code here, if needed.
		}
	}
}