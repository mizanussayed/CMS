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
public partial class AuditLogController : ControllerBase
{
	private readonly ISecurityHelper _securityHelper;
	private readonly ILogger<AuditLogController> _logger;
	private readonly IConfiguration _config;
	private readonly IAuditLogRepository _auditLogRepository;
	private readonly ICsvExporter _csvExporter;

	public AuditLogController(ISecurityHelper securityHelper, ILogger<AuditLogController> logger, IConfiguration config, IAuditLogRepository auditLogRepository, ICsvExporter csvExporter)
	{
		this._securityHelper = securityHelper;
		this._logger = logger;
		this._config = config;
		this._auditLogRepository = auditLogRepository;
		this._csvExporter = csvExporter;
	}

	[HttpGet, Authorize(Policy = Constants.SystemAdmin)]
	public Task<IActionResult> GetAuditLogs(int pageNumber) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), pageNumber.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (pageNumber < 0)
			return BadRequest(String.Format(ValidationMessages.AuditLog_InvalidPageNumber, pageNumber));
		#endregion

		var result = await _auditLogRepository.GetAuditLogs(pageNumber);
		if (result == null)
			return NotFound(ValidationMessages.AuditLog_NotFoundList);

		return Ok(result);
	});

	[HttpGet("{id:int}"), Authorize(Policy = Constants.SystemAdmin)]
	public Task<IActionResult> GetAuditLogById(int id) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (id <= 0)
			return BadRequest(String.Format(ValidationMessages.AuditLog_InvalidId, id));
		#endregion

		var result = await _auditLogRepository.GetAuditLogById(id);
		if (result == null)
			return NotFound(String.Format(ValidationMessages.AuditLog_NotFoundId, id));

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

		var result = await _auditLogRepository.Export();
		if (result == null)
			return NotFound(ValidationMessages.AuditLog_NotFoundList);

		return Ok(new ExportFileModel { FileName = $"{Guid.NewGuid()}.csv", ContentType = "text/csv", Data = _csvExporter.ExportToCsv(result) });
	});

	[HttpPost("InsertAuditLog"), Authorize(Policy = Constants.SystemAdmin)]
	public Task<IActionResult> InsertAuditLog([FromBody] LogModel logModel) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), logModel.NewData))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (!ModelState.IsValid) return BadRequest(ModelState);

		if (logModel == null)
			return BadRequest(ValidationMessages.AuditLog_Null);
		#endregion

		int insertedAuditLogId = await _auditLogRepository.InsertAuditLog(logModel);
		return Created(nameof(GetAuditLogById), new { id = insertedAuditLogId });
	});

	[HttpDelete("{id:int}"), Authorize(Policy = Constants.SystemAdmin)]
	public Task<IActionResult> DeleteAuditLog(int id) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (id <= 0)
			return BadRequest(String.Format(ValidationMessages.AuditLog_InvalidId, id));

		var auditLogToDelete = await _auditLogRepository.GetAuditLogById(id);
		if (auditLogToDelete == null)
			return NotFound(String.Format(ValidationMessages.AuditLog_NotFoundId, id));
		#endregion

		await _auditLogRepository.DeleteAuditLog(id);
		return NoContent(); // success
	});
}