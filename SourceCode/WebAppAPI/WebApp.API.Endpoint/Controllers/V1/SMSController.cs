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
public partial class SMSController : ControllerBase
{
	private readonly ISecurityHelper _securityHelper;
	private readonly ILogger<SMSController> _logger;
	private readonly IConfiguration _config;
	private readonly ISMSSender _smsSender;

	public SMSController(ISecurityHelper securityHelper, ILogger<SMSController> logger, IConfiguration config, ISMSSender smsSender)
	{
		this._securityHelper = securityHelper;
		this._logger = logger;
		this._config = config;
		this._smsSender = smsSender;
	}

	[HttpPost("Send"), AllowAnonymous]
	public Task<IActionResult> Send([FromBody] SMSModel sms) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), sms.Content))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (sms == null)
			return BadRequest(ValidationMessages.SMS_Null);

		if (sms.To.Count == 0 || sms.Content == "")
			return BadRequest(ValidationMessages.SMS_Empty);
		#endregion

		await _smsSender.SendSMS(sms);
		return NoContent();
	});
}