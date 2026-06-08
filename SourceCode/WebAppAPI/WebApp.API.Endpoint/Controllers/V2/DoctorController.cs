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
public partial class DoctorController : ControllerBase
{
	private readonly ISecurityHelper _securityHelper;
	private readonly ILogger<DoctorController> _logger;
	private readonly IConfiguration _config;
	private readonly IDoctorRepository _doctorRepository;
	private readonly ICsvExporter _csvExporter;

	public DoctorController(ISecurityHelper securityHelper, ILogger<DoctorController> logger, IConfiguration config, IDoctorRepository doctorRepository, ICsvExporter csvExporter)
	{
		this._securityHelper = securityHelper;
		this._logger = logger;
		this._config = config;
		this._doctorRepository = doctorRepository;
		this._csvExporter = csvExporter;
	}

	[HttpGet, AllowAnonymous]
	public Task<IActionResult> GetDoctors(int pageNumber) =>
	TryCatch(async () =>
	{
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), pageNumber.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (pageNumber < 0)
			return BadRequest(String.Format(ValidationMessages.Doctor_InvalidPageNumber, pageNumber));

		var result = await _doctorRepository.GetDoctors(pageNumber);
		if (result == null)
			return NotFound(ValidationMessages.Doctor_NotFoundList);

		return Ok(result);
	});

	[HttpGet("{id:int}"), AllowAnonymous]
	public Task<IActionResult> GetDoctorById(int id)
	{
		return TryCatch(async () =>
	{
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (id < 1) return BadRequest(String.Format(ValidationMessages.Doctor_InvalidId, id));

		var result = await _doctorRepository.GetDoctorById(id);
		if (result == null) return NotFound(String.Format(ValidationMessages.Doctor_NotFoundId, id));

		return Ok(result);
	});
	}

	[HttpPost, Authorize(Policy = Constants.SystemAdmin)]
	public Task<IActionResult> InsertDoctor([FromBody] Dictionary<string, object> PostData) =>
	TryCatch(async () =>
	{
		DoctorModel doctor = PostData["Data"] == null ? null : JsonSerializer.Deserialize<DoctorModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		if (doctor == null) return BadRequest(ValidationMessages.Doctor_Null);
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);

		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), doctor.Name))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (!ModelState.IsValid) return BadRequest(ModelState);


		var existingDoctor = await _doctorRepository.GetDoctorByName(doctor.Name);
		if (existingDoctor != null)
		{
			ModelState.AddModelError("Duplicate Doctor", String.Format(ValidationMessages.Doctor_Duplicate, doctor.Name));
			return BadRequest(String.Format(ValidationMessages.Doctor_Duplicate, doctor.Name));
		}

		int id = await _doctorRepository.InsertDoctor(doctor, logModel);
		return Created(nameof(GetDoctorById), new { id = id });
	});

	[HttpPut("{id:int}"), Authorize(Policy = Constants.SystemAdmin)]
	public Task<IActionResult> UpdateDoctor(int id, [FromBody] Dictionary<string, object> PostData) =>
	TryCatch(async () =>
	{
		DoctorModel doctor = PostData["Data"] == null ? null : JsonSerializer.Deserialize<DoctorModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		if (id <= 0) return BadRequest(String.Format(ValidationMessages.Doctor_InvalidId, id));
		if (doctor == null) return BadRequest(ValidationMessages.Doctor_Null);
		if (logModel == null) return BadRequest(ValidationMessages.AuditLog_Null);
		if (id != doctor.DoctorID) return BadRequest(ValidationMessages.Doctor_Mismatch);

		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		var existingDoctor = await _doctorRepository.GetDoctorById(id);
		if (existingDoctor == null) return NotFound(String.Format(ValidationMessages.Doctor_NotFoundId, id));

		await _doctorRepository.UpdateDoctor(doctor, logModel);
		return NoContent();
	});


	[HttpDelete("{id:int}"), Authorize(Policy = Constants.SystemAdmin)]
	public Task<IActionResult> DeleteDoctor(int id, [FromBody] LogModel logModel) =>
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
			return BadRequest(String.Format(ValidationMessages.Doctor_InvalidId, id));

		var doctorToDelete = await _doctorRepository.GetDoctorById(id);
		if (doctorToDelete == null)
			return NotFound(String.Format(ValidationMessages.Doctor_NotFoundId, id));
		#endregion

		await _doctorRepository.DeleteDoctor(id, logModel);
		return NoContent(); // success
	});


	[HttpGet("Export"), AllowAnonymous]
	public Task<IActionResult> Export() =>
	TryCatch(async () =>
	{
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		var result = await _doctorRepository.Export();
		if (result == null)
			return NotFound(ValidationMessages.Doctor_NotFoundList);

		return Ok(new ExportFileModel { FileName = $"{Guid.NewGuid()}.csv", ContentType = "text/csv", Data = _csvExporter.ExportToCsv(result) });
	});
}
