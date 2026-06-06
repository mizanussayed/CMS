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
public partial class MobileAuthController : ControllerBase
{
	private readonly ILogger<MobileAuthController> _logger;
	private readonly IConfiguration _config;
	private readonly IMobileAuthRepository _mobileAuthRepository;
	private readonly ISecurityHelper _securityHelper;

	public MobileAuthController(ILogger<MobileAuthController> logger, IConfiguration config, IMobileAuthRepository mobileAuthRepository, ISecurityHelper securityHelper)
	{
		this._logger = logger;
		this._config = config;
		this._mobileAuthRepository = mobileAuthRepository;
		this._securityHelper = securityHelper;
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
			var userInfo = new UserInfoModel
			{
				Id = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value,
				UserName = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value,
				Email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value,
				Role = claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value
			};

			return Ok(userInfo);
		}

		return null;
	});

	[HttpPost("Register"), AllowAnonymous]
	public Task<IActionResult> Register([FromBody] UserInfoModel userInfo) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), userInfo.UserName))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (!ModelState.IsValid) return BadRequest(ModelState);
		if (userInfo == null) return BadRequest(ValidationMessages.MobileAuth_RegisterNull);
		#endregion

		RegisterResponseModel response = await _mobileAuthRepository.Register(userInfo);
		return (response == null) ? BadRequest() : Ok(response);
	});

	[HttpPost("Login"), AllowAnonymous]
	public Task<IActionResult> Login([FromBody] UserLoginModel userLogin) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), userLogin.UserName))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (!ModelState.IsValid) return BadRequest(ModelState);
		if (userLogin == null) return BadRequest(ValidationMessages.Auth_LoginNull);
		#endregion

		UserInfoModel userInfo = await _mobileAuthRepository.Login(userLogin);

		TokenModel token = new TokenModel();
		if (userInfo != null)
		{
			token.JwtToken = await Task.Run(() => _securityHelper.GenerateJSONWebToken(userInfo));
			token.Expires = DateTime.Now.AddMinutes(Convert.ToInt32(_config["JWT:Expires"]));
			token.RefreshToken = await Task.Run(() => _securityHelper.GenerateRefreshToken());
			token.RefreshTokenExpires = DateTime.Now.AddMinutes(Convert.ToInt32(_config["JWT:RefreshToken_Expires"]));

			await _mobileAuthRepository.UpdateRefreshToken(userInfo.Id, token);
		}

		return (userInfo == null) ? Unauthorized() : Ok(token);
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
		if (userInfo == null) return BadRequest(ValidationMessages.Auth_UserInfoNull);
		#endregion

		TokenModel token = new TokenModel
		{
			JwtToken = await Task.Run(() => _securityHelper.GenerateJSONWebToken(userInfo)),
			Expires = DateTime.Now.AddMinutes(Convert.ToInt32(_config["JWT:Expires"])),
			RefreshToken = await Task.Run(() => _securityHelper.GenerateRefreshToken()),
			RefreshTokenExpires = DateTime.Now.AddMinutes(Convert.ToInt32(_config["JWT:RefreshToken_Expires"]))
		};
		await _mobileAuthRepository.UpdateRefreshToken(userInfo.Id, token);
		return Ok(token);
	});

	[HttpPost("ChangePassword")]
	public Task<IActionResult> ChangePassword([FromBody] ChangePasswordModel changePassword) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString(), changePassword.CurrentPassword))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		if (!ModelState.IsValid) return BadRequest(ModelState);
		if (changePassword == null) return BadRequest(ValidationMessages.MobileAuth_ChangePasswordNull);
		#endregion

		UserInfoModel userInfo = await _mobileAuthRepository.ChangePassword(changePassword);
		return (userInfo == null) ? BadRequest() : Ok(userInfo);
	});

	[HttpPost("RefreshToken")]
	public Task<IActionResult> RefreshToken(string payLoad = null) =>
	TryCatch(async () =>
	{
		#region Validation
		if (Convert.ToBoolean(_config["Hash:HashChecking"]))
		{
			if (!_securityHelper.IsValidHash(Request.Headers["x-hash"].ToString()))
				return Unauthorized(ValidationMessages.InvalidHash);
		}

		var identity = HttpContext.User.Identity as ClaimsIdentity;
		if (identity == null) return Unauthorized();

		string userId = identity.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
		UserInfoModel userInfo = await _mobileAuthRepository.GetCurrentUser(userId);
		if (userInfo == null) return Unauthorized();

		TokenModel refreshToken = await _mobileAuthRepository.GetRefreshToken(userId);
		if (!refreshToken.RefreshToken.Equals(Request.Cookies["X-RefreshToken"].ToString()))
			return Unauthorized(ValidationMessages.Auth_InvalidRefreshToken);
		else if (refreshToken.RefreshTokenExpires < DateTime.Now)
			return Unauthorized(ValidationMessages.Auth_ExpiredRefreshToken);
		#endregion

		TokenModel token = new TokenModel
		{
			JwtToken = await Task.Run(() => _securityHelper.GenerateJSONWebToken(userInfo)),
			Expires = DateTime.Now.AddMinutes(Convert.ToInt32(_config["JWT:Expires"])),
			RefreshToken = await Task.Run(() => _securityHelper.GenerateRefreshToken()),
			RefreshTokenExpires = DateTime.Now.AddMinutes(Convert.ToInt32(_config["JWT:RefreshToken_Expires"]))
		};
		await _mobileAuthRepository.UpdateRefreshToken(userInfo.Id, token);
		return Ok(token);
	});
}