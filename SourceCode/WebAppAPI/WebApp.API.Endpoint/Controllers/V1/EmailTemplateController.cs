using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WebApp.API.Endpoint.Resources;
using WebApp.Core.Constant;
using WebApp.Core.Contract.Infrastructure;
using WebApp.Core.Contract.Persistence;
using WebApp.Core.Model;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace WebApp.API.Endpoint.Controllers.V1;

[ApiVersion("1.0", Deprecated = true)]
[Route("v{version:apiVersion}/[controller]")]
[ApiController]
public partial class EmailTemplateController : ControllerBase
{
	private readonly ISecurityHelper _securityHelper;
	private readonly ILogger<EmailTemplateController> _logger;
	private readonly IConfiguration _config;
	private readonly IEmailTemplateRepository _emailTemplateRepository;

	public EmailTemplateController(ISecurityHelper securityHelper, ILogger<EmailTemplateController> logger, IConfiguration config, IEmailTemplateRepository emailTemplateRepository)
	{
		this._securityHelper = securityHelper;
		this._logger = logger;
		this._config = config;
		this._emailTemplateRepository = emailTemplateRepository;
	}

	[HttpGet("GetByName/{name}"), AllowAnonymous]
	public Task<IActionResult> GetEmailTemplateByName(string name) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), name))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		var result = await _emailTemplateRepository.GetEmailTemplateByName(name);
		if (result == null)
			return NotFound(String.Format(ValidationMessages.EmailTemplate_NotFoundName, name));
		#endregion

		return Ok(result);
	});

	[HttpGet, Authorize(Policy = Constants.SystemAdmin)]
	public Task<IActionResult> GetEmailTemplates(int pageNumber) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), pageNumber.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (pageNumber < 0)
			return BadRequest(String.Format(ValidationMessages.EmailTemplate_InvalidPageNumber, pageNumber));

		var result = await _emailTemplateRepository.GetEmailTemplates(pageNumber);
		if (result == null)
			return NotFound(ValidationMessages.EmailTemplate_NotFoundList);
		#endregion

		return Ok(result);
	});

	[HttpGet("{id:int}"), Authorize(Policy = Constants.SystemAdmin)]
	public Task<IActionResult> GetEmailTemplateById(int id) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (id <= 0)
			return BadRequest(String.Format(ValidationMessages.EmailTemplate_InvalidId, id));

		var result = await _emailTemplateRepository.GetEmailTemplateById(id);
		if (result == null)
			return NotFound(String.Format(ValidationMessages.EmailTemplate_NotFoundId, id));
		#endregion

		return Ok(result);
	});

	[HttpPut("Update/{id:int}"), Authorize(Policy = Constants.SystemAdmin)]
	public Task<IActionResult> UpdateEmailTemplate(int id, [FromBody] Dictionary<string, object> PostData) =>
	TryCatch(async () =>
	{
		EmailTemplateModel emailTemplate = PostData["Data"] == null ? null : JsonSerializer.Deserialize<EmailTemplateModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (emailTemplate == null) return BadRequest(ValidationMessages.EmailTemplate_Null);
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);
		if (id != emailTemplate.Id) return BadRequest(ValidationMessages.EmailTemplate_Mismatch);

		var emailTemplateToUpdate = await _emailTemplateRepository.GetEmailTemplateById(id);
		if (emailTemplateToUpdate == null)
			return NotFound(String.Format(ValidationMessages.EmailTemplate_NotFoundId, id));
		#endregion

		await _emailTemplateRepository.UpdateEmailTemplate(emailTemplate, logModel);
		return NoContent(); // success
	});
}