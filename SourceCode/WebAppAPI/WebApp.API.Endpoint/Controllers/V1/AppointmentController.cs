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

    [HttpPost("book")] 
    [Authorize]
    public Task<IActionResult> BookAppointment([FromBody] Dictionary<string, object> PostData) =>
    TryCatch(async () =>
    {
        AppointmentModel appointment = PostData["Data"] == null ? null : JsonSerializer.Deserialize<AppointmentModel>(PostData["Data"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        LogModel logModel = PostData["Log"] == null ? null : JsonSerializer.Deserialize<LogModel>(PostData["Log"].ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (appointment == null) return BadRequest("Appointment data is null");
        if (logModel == null) return BadRequest("Log model is null");

        int id = await _appointmentRepository.BookAppointment(appointment, logModel);
        return Created(nameof(BookAppointment), new { id = id });
    });

    [HttpGet("{userId:int}"), Authorize]
    public Task<IActionResult> GetAppointmentsByUser(int userId) =>
    TryCatch(async () =>
    {
        if (userId <= 0) return BadRequest("Invalid user id");

        var result = await _appointmentRepository.GetAppointmentsByUser(userId);
        return Ok(result);
    });

    [HttpPut("cancel/{appointmentId:int}"), Authorize]
    public Task<IActionResult> CancelAppointment(int appointmentId, [FromBody] LogModel logModel) =>
    TryCatch(async () =>
    {
        if (appointmentId <= 0) return BadRequest("Invalid appointment id");
        if (logModel == null) return BadRequest("Log model is null");

        // ownership check should be enforced in repository/stored procedure based on logModel.UserName or provided user id
        // ownership check is enforced by repository/stored procedure using the user name
        await _appointmentRepository.CancelAppointment(appointmentId, logModel.UserName, logModel);
        return NoContent();
    });
}
