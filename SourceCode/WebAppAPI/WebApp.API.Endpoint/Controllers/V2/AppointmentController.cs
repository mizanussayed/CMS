using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using WebApp.API.Endpoint.Resources;
using WebApp.Core.Constant;
using WebApp.Core.Contract.Infrastructure;
using WebApp.Core.Contract.Persistence;
using WebApp.Core.Model;

namespace WebApp.API.Endpoint.Controllers.V2;

[ApiVersion("2.0")]
[Route("v{version:apiVersion}/[controller]")]
[ApiController]
public partial class AppointmentController : ControllerBase
{
	private readonly ISecurityHelper _securityHelper;
	private readonly ILogger<AppointmentController> _logger;
	private readonly IConfiguration _config;
	private readonly IAppointmentRepository _appointmentRepository;

	public AppointmentController(ISecurityHelper securityHelper, ILogger<AppointmentController> logger, IConfiguration config, IAppointmentRepository appointmentRepository)
	{
		this._securityHelper = securityHelper;
		this._logger = logger;
		this._config = config;
		this._appointmentRepository = appointmentRepository;
	}


	[HttpGet, AllowAnonymous]
	public Task<IActionResult> GetAppointments(int pageNumber) =>
	TryCatch(async () =>
	{
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), pageNumber.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (pageNumber < 0)
			return BadRequest(String.Format(ValidationMessages.AuditLog_InvalidPageNumber, pageNumber));

		var result = await _appointmentRepository.GetAllAppointments(pageNumber);
		if (result == null)
			return NotFound(ValidationMessages.Appointment_NotFoundList);

		return Ok(result);
	});

	[HttpGet("{userId:int}"), AllowAnonymous]
	public Task<IActionResult> GetAppointmentsByUser(int userId) =>
	TryCatch(async () =>
	{
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), userId.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (userId <= 0) return BadRequest(String.Format(ValidationMessages.Appointment_InvalidId, userId));

		var result = await _appointmentRepository.GetAppointmentsByUser(userId);
		return Ok(result);
	});


	[HttpPost("book"), AllowAnonymous]
	//[Authorize]
	public Task<IActionResult> BookAppointment([FromBody] Dictionary<string, object> PostData) =>
	TryCatch(async () =>
	{
		AppointmentModel appointment = PostData["Data"] == null ? null : JsonSerializer.Deserialize<AppointmentModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		if (appointment == null) return BadRequest(ValidationMessages.Appointment_Null);
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		int id = await _appointmentRepository.BookAppointment(appointment, logModel);
		return Created(nameof(BookAppointment), new { id = id });
	});

	[HttpPut("UpdateStatus/{appointmentId:int}"), Authorize(Policy = Constants.SystemAdmin)]
	public Task<IActionResult> UpdateAppointmentStatus(int appointmentId, [FromBody] Dictionary<string, object> PostData) =>
	TryCatch(async () =>
	{
		string status = PostData["Status"]?.ToString();
		LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		if (appointmentId <= 0) return BadRequest(String.Format(ValidationMessages.Appointment_InvalidId, appointmentId));
		if (string.IsNullOrWhiteSpace(status)) return BadRequest(ValidationMessages.Appointment_StatusNullOrEmpty);
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), appointmentId.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		var appointment = await _appointmentRepository.GetAppointmentById(appointmentId);
		if (appointment == null) return NotFound(String.Format(ValidationMessages.Appointment_NotFoundId, appointmentId));

		await _appointmentRepository.UpdateAppointmentStatus(appointmentId, status, logModel);
		return NoContent();
	});

	[HttpPut("cancel/{appointmentId:int}"), AllowAnonymous]
	//[HttpPut("cancel/{appointmentId:int}"), Authorize]
	public Task<IActionResult> CancelAppointment(int appointmentId, [FromBody] LogModel logModel) =>
	TryCatch(async () =>
	{
		if (appointmentId <= 0) return BadRequest(String.Format(ValidationMessages.Appointment_InvalidId, appointmentId));
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), appointmentId.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		// ownership check should be enforced in repository/stored procedure based on logModel.UserName or provided user id
		// ownership check is enforced by repository/stored procedure using the user name
		await _appointmentRepository.CancelAppointment(appointmentId, logModel.UserName, logModel);
		return NoContent();
	});


	[HttpDelete("{id:int}"), Authorize(Policy = Constants.SystemAdmin)]
	public Task<IActionResult> DeleteAppointment(int id, [FromBody] LogModel logModel) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (!ModelState.IsValid) return BadRequest(ModelState);

		if (id < 1)
			return BadRequest(String.Format(ValidationMessages.Appointment_InvalidId, id));

		var appointmentToDelete = await _appointmentRepository.GetAppointmentById(id);
		if (appointmentToDelete == null)
			return NotFound(String.Format(ValidationMessages.Appointment_NotFoundId, id));
		#endregion

		await _appointmentRepository.DeleteAppointment(id, logModel.UserName, logModel);
		return NoContent(); // success
	});
}
