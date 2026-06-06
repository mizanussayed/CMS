using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WebApp.API.Endpoint.Resources;
using WebApp.Core.Contract.Infrastructure;
using WebApp.Core.Model;
using System;
using System.Threading.Tasks;

namespace WebApp.API.Endpoint.Controllers.V1;

[ApiVersion("1.0")]
[Route("v{version:apiVersion}/[controller]")]
[ApiController]
public partial class EmailController : ControllerBase
{
	private readonly ISecurityHelper _securityHelper;
	private readonly ILogger<EmailController> _logger;
	private readonly IConfiguration _config;
	private readonly IEmailSender _emailSender;

	public EmailController(ISecurityHelper securityHelper, ILogger<EmailController> logger, IConfiguration config, IEmailSender emailSender)
	{
		this._securityHelper = securityHelper;
		this._logger = logger;
		this._config = config;
		this._emailSender = emailSender;
	}

	[HttpPost("Send"), AllowAnonymous]
	public Task<IActionResult> Send([FromBody] EmailModel email) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), email.To))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (email == null)
			return BadRequest(ValidationMessages.Email_Null);

		if (email.To == "" || email.Subject == "" || email.Body == "")
			return BadRequest(ValidationMessages.Email_Empty);
		#endregion

		await _emailSender.SendEmail(email);
		return NoContent();
	});
}