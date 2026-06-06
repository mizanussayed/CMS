using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WebApp.API.Endpoint.Resources;
using System;
using System.Threading.Tasks;

namespace WebApp.API.Endpoint.Controllers.V1;

public partial class EmailTemplateController
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

			if (returningFunction.Method.Name.Contains("GetEmailTemplates"))
				Messages = ExceptionMessages.EmailTemplate_List;

			if (returningFunction.Method.Name.Contains("GetEmailTemplateById"))
				Messages = ValidationMessages.EmailTemplate_NotFoundId;

			if (returningFunction.Method.Name.Contains("GetEmailTemplateByName"))
				Messages = ValidationMessages.EmailTemplate_NotFoundId;

			if (returningFunction.Method.Name.Contains("UpdateEmailTemplate"))
				Messages = ExceptionMessages.EmailTemplate_Update;

			return StatusCode(StatusCodes.Status500InternalServerError, Messages);
		}
		finally
		{
			// Do clean up code here, if needed.
		}
	}
}