using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WebApp.Core.Constant;
using WebApp.Core.Contract.Infrastructure;
using WebApp.Core.Contract.Persistence;
using WebApp.Core.Model;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace WebApp.API.Endpoint.Controllers.V1;

[ApiVersion("1.0")]
[Route("v{version:apiVersion}/[controller]")]
[ApiController]
public partial class DoctorController : ControllerBase
{
    private readonly ISecurityHelper _securityHelper;
    private readonly ILogger<DoctorController> _logger;
    private readonly IConfiguration _config;
    private readonly IDoctorRepository _doctorRepository;

    public DoctorController(ISecurityHelper securityHelper, ILogger<DoctorController> logger, IConfiguration config, IDoctorRepository doctorRepository)
    {
        this._securityHelper = securityHelper;
        this._logger = logger;
        this._config = config;
        this._doctorRepository = doctorRepository;
    }

    [HttpGet, AllowAnonymous]
    [EnableRateLimiting("LimiterPolicy")]
    public Task<IActionResult> GetDoctors(int pageNumber) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), pageNumber.ToString()))
                return Unauthorized("Invalid hash");
        }

        if (pageNumber < 0)
            return BadRequest("Invalid page number");

        var result = await _doctorRepository.GetDoctors(pageNumber);
        if (result == null)
            return NotFound("No doctors found");

        return Ok(result);
    });

    [HttpGet("{id:int}"), AllowAnonymous]
    public Task<IActionResult> GetDoctorById(int id) =>
    TryCatch(async () =>
    {
        if (Convert.ToBoolean(_config["Hash:HashChecking"]))
        {
            if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), id.ToString()))
                return Unauthorized("Invalid hash");
        }

        if (id < 1) return BadRequest("Invalid id");

        var result = await _doctorRepository.GetDoctorById(id);
        if (result == null) return NotFound($"Doctor not found for id {id}");

        return Ok(result);
    });

    [HttpPost, Authorize(Policy = Constants.SystemAdmin)]
    public Task<IActionResult> InsertDoctor([FromBody] Dictionary<string, object> PostData) =>
    TryCatch(async () =>
    {
        DoctorModel doctor = PostData["Data"] == null ? null : JsonSerializer.Deserialize<DoctorModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (doctor == null) return BadRequest("Doctor data is null");
        if (logModel == null) return BadRequest("Log model is null");

        int id = await _doctorRepository.InsertDoctor(doctor, logModel);
        return Created(nameof(GetDoctorById), new { id = id });
    });
}
