using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WebApp.API.Endpoint.Resources;
using WebApp.Core.Contract.Infrastructure;
using WebApp.Core.Contract.Persistence;
using WebApp.Core.Model;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace WebApp.API.Endpoint.Controllers.V1;

[ApiVersion("1.0")]
[Route("v{version:apiVersion}/[controller]")]
[ApiController]
public partial class AuthController : ControllerBase
{
	private readonly ILogger<AuthController> _logger;
	private readonly IConfiguration _config;
	private readonly ISecurityHelper _securityHelper;
	private readonly IAuthRepository _authRepository;

	public AuthController(ILogger<AuthController> logger, IConfiguration config, ISecurityHelper securityHelper, IAuthRepository authRepository)
	{
		this._logger = logger;
		this._config = config;
		this._securityHelper = securityHelper;
		this._authRepository = authRepository;
	}

	[HttpGet("GetCurrentUser")]
	public IActionResult GetCurrentUser() =>
	TryCatch(() =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}
		#endregion

		var identity = HttpContext.User.Identity as ClaimsIdentity;

		if (identity != null)
		{
			var claims = identity.Claims;
			var userModel = new UserInfoModel
			{
				Id = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value,
				UserName = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value,
				Email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value,
				Role = claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value
			};

			return Ok(userModel);
		}

		return null;
	});

	[HttpPost("GetToken"), AllowAnonymous]
	public Task<IActionResult> GetToken([FromBody] UserInfoModel userInfo) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), userInfo.Id))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (!ModelState.IsValid) return BadRequest(ModelState);

		if (userInfo == null)
			return BadRequest(ValidationMessages.Auth_UserInfoNull);
		#endregion

		TokenModel token = new TokenModel
		{
			JwtToken = await Task.Run(() => _securityHelper.GenerateJSONWebToken(userInfo)),
			Expires = DateTime.Now.AddMinutes(Convert.ToInt32(_config["JWT:Expires"])),
			RefreshToken = await Task.Run(() => _securityHelper.GenerateRefreshToken()),
			RefreshTokenExpires = DateTime.Now.AddMinutes(Convert.ToInt32(_config["JWT:RefreshToken_Expires"]))
		};
		await _authRepository.UpdateRefreshToken(userInfo.Id, token);
		return Ok(token);
	});

	[HttpPost("RefreshToken"), AllowAnonymous]
	public Task<IActionResult> RefreshToken([FromBody] string userId) =>
	TryCatch(async () =>
	{
		#region Validatation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		UserInfoModel userInfo = await _authRepository.GetCurrentUser(userId);
		if (userInfo == null) return Unauthorized();

		TokenModel refreshToken = await _authRepository.GetRefreshToken(userId);

		if (Request.Cookies["X-RefreshToken"] != null)
			if (!refreshToken.RefreshToken.Equals(Request.Cookies["X-RefreshToken"].ToString()))
				return Unauthorized(ValidationMessages.Auth_InvalidRefreshToken);

		if (refreshToken.RefreshTokenExpires < DateTime.Now)
			return Unauthorized(ValidationMessages.Auth_ExpiredRefreshToken);
		#endregion

		TokenModel token = new TokenModel
		{
			JwtToken = await Task.Run(() => _securityHelper.GenerateJSONWebToken(userInfo)),
			Expires = DateTime.Now.AddMinutes(Convert.ToInt32(_config["JWT:Expires"])),
			RefreshToken = await Task.Run(() => _securityHelper.GenerateRefreshToken()),
			RefreshTokenExpires = DateTime.Now.AddMinutes(Convert.ToInt32(_config["JWT:RefreshToken_Expires"]))
		};
		await _authRepository.UpdateRefreshToken(userInfo.Id, token);
		return Ok(token);
	});
}