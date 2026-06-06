using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.API.Endpoint.Controllers;

[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
[ApiVersionNeutral]
public class HealthController : ControllerBase
{
	[HttpGet("ping"), AllowAnonymous]
	public IActionResult Ping()
	{
		return Ok("Everything seems great!");
	}
}