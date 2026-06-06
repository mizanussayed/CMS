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
using System.Threading.Tasks;

namespace WebApp.API.Endpoint.Controllers.V1;

[ApiVersion("1.0")]
[Route("v{version:apiVersion}/[controller]")]
[ApiController]
public partial class ApplicationLogController : ControllerBase
{
	private readonly ISecurityHelper _securityHelper;
	private readonly ILogger<ApplicationLogController> _logger;
	private readonly IConfiguration _config;
	private readonly IApplicationLogRepository _applicationLogRepository;
	private readonly ICsvExporter _csvExporter;

	public ApplicationLogController(ISecurityHelper securityHelper, ILogger<ApplicationLogController> logger, IConfiguration config, IApplicationLogRepository applicationLogRepository, ICsvExporter csvExporter)
	{
		this._securityHelper = securityHelper;
		this._logger = logger;
		this._config = config;
		this._applicationLogRepository = applicationLogRepository;
		this._csvExporter = csvExporter;
	}

	[HttpGet, Authorize(Policy = Constants.SystemAdmin)]
	public Task<IActionResult> GetApplicationLogs(int pageNumber) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), pageNumber.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (pageNumber < 0)
			return BadRequest(String.Format(ValidationMessages.ApplicationLog_InvalidPageNumber, pageNumber));
		#endregion

		var result = await _applicationLogRepository.GetApplicationLogs(pageNumber);
		if (result == null)
			return NotFound(ValidationMessages.ApplicationLog_NotFoundList);

		return Ok(result);
	});

	[HttpGet("{id:int}"), Authorize(Policy = Constants.SystemAdmin)]
	public Task<IActionResult> GetApplicationLogById(int id) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (id <= 0)
			return BadRequest(String.Format(ValidationMessages.ApplicationLog_InvalidId, id));
		#endregion

		var result = await _applicationLogRepository.GetApplicationLogById(id);
		if (result == null)
			return NotFound(String.Format(ValidationMessages.ApplicationLog_NotFoundId, id));

		return Ok(result);
	});

	[HttpGet("Export"), Authorize(Policy = Constants.SystemAdmin)]
	public Task<IActionResult> Export() =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}
		#endregion

		var result = await _applicationLogRepository.Export();
		if (result == null)
			return NotFound(ValidationMessages.ApplicationLog_NotFoundList);

		return Ok(new ExportFileModel { FileName = $"{Guid.NewGuid()}.csv", ContentType = "text/csv", Data = _csvExporter.ExportToCsv(result) });
	});

	[HttpDelete("{id:int}"), Authorize(Policy = Constants.SystemAdmin)]
	public Task<IActionResult> DeleteApplicationLog(int id) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (id <= 0)
			return BadRequest(String.Format(ValidationMessages.ApplicationLog_InvalidId, id));

		var applicationLogToDelete = await _applicationLogRepository.GetApplicationLogById(id);
		if (applicationLogToDelete == null)
			return NotFound(String.Format(ValidationMessages.ApplicationLog_NotFoundId, id));
		#endregion

		await _applicationLogRepository.DeleteApplicationLog(id);
		return NoContent(); // success
	});
}