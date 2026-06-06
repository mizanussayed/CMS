using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WebApp.API.Endpoint.Resources;
using System;
using System.Threading.Tasks;

namespace WebApp.API.Endpoint.Controllers.V1;

public partial class AuditLogController
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

			if (returningFunction.Method.Name.Contains("GetAuditLogs"))
				Messages = ExceptionMessages.AuditLog_List;

			if (returningFunction.Method.Name.Contains("GetAuditLogById"))
				Messages = ExceptionMessages.AuditLog_Id;

			if (returningFunction.Method.Name.Contains("InsertAuditLog"))
				Messages = ExceptionMessages.AuditLog_Insert;

			if (returningFunction.Method.Name.Contains("DeleteAuditLog"))
				Messages = ExceptionMessages.AuditLog_Delete;

			if (returningFunction.Method.Name.Contains("Export"))
				Messages = ExceptionMessages.AuditLog_List;

			return StatusCode(StatusCodes.Status500InternalServerError, Messages);
		}
		finally
		{
			// Do clean up code here, if needed.
		}
	}
}