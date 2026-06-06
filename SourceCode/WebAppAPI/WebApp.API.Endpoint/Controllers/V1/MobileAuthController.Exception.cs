using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WebApp.API.Endpoint.Resources;
using System;
using System.Threading.Tasks;

namespace WebApp.API.Endpoint.Controllers.V1;

public partial class MobileAuthController
{
	private delegate Task<IActionResult> ReturningFunctionWithTask();
	private delegate IActionResult ReturningFunctionWithoutTask();
	private string Messages = "";

	private async Task<IActionResult> TryCatch(ReturningFunctionWithTask returningFunction)
	{
		try
		{
			return await returningFunction();
		}
		catch (Exception ex)
		{
			_ = Task.Run(() => { _logger.LogError(ex, ex.Message); });

			if (returningFunction.Method.Name.Contains("Register"))
				Messages = ExceptionMessages.MobileAuth_Register;

			if (returningFunction.Method.Name.Contains("Login"))
				Messages = ExceptionMessages.MobileAuth_Login;

			if (returningFunction.Method.Name.Contains("ChangePassword"))
				Messages = ExceptionMessages.MobileAuth_ChangePassword;

			if (returningFunction.Method.Name.Contains("GetToken"))
				Messages = ExceptionMessages.MobileAuth_GetToken;

			return StatusCode(StatusCodes.Status500InternalServerError, Messages);
		}
		finally
		{
			// Do clean up code here, if needed.
		}
	}

	private IActionResult TryCatch(ReturningFunctionWithoutTask returningFunction)
	{
		try
		{
			return returningFunction();
		}
		catch (Exception ex)
		{
			_ = Task.Run(() => { _logger.LogError(ex, ex.Message); });

			if (returningFunction.Method.Name.Contains("GetCurrentUser"))
				Messages = ExceptionMessages.MobileAuth_GetCurrentUser;

			return StatusCode(StatusCodes.Status500InternalServerError, Messages);
		}
		finally
		{
			// Do clean up code here, if needed.
		}
	}
}