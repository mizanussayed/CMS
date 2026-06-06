using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using WebApp.API.Persistence.Identity;
using WebApp.Core.Contract.Infrastructure;
using WebApp.Core.Contract.Persistence;
using WebApp.Core.Model;
using WebApp.Infrastructure;
using System.Text;
using System.Text.Encodings.Web;

namespace WebApp.API.Persistence;

public class MobileAuthRepository : IMobileAuthRepository
{
	private readonly ILogger<MobileAuthRepository> _logger;
	private readonly SignInManager<ApplicationUser> _signInManager;
	private readonly UserManager<ApplicationUser> _userManager;
	private readonly HttpContext _context;
	private readonly IAuditLogRepository _auditLogRepository;
	private readonly IEmailSender _emailSender;
	private readonly IEmailTemplateRepository _emailTemplateRepository;
	private readonly IDataAccessHelper _dataAccessHelper;

	public MobileAuthRepository(ILogger<MobileAuthRepository> logger, SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, IHttpContextAccessor accessor, IAuditLogRepository auditLogRepository, IEmailSender emailSender, IEmailTemplateRepository emailTemplateRepository, IDataAccessHelper dataAccessHelper)
	{
		this._logger = logger;
		this._signInManager = signInManager;
		this._userManager = userManager;
		this._context = accessor.HttpContext;
		this._auditLogRepository = auditLogRepository;
		this._emailSender = emailSender;
		this._emailTemplateRepository = emailTemplateRepository;
		this._dataAccessHelper = dataAccessHelper;
	}

	public async Task<RegisterResponseModel> Register(UserInfoModel userInfo)
	{
		ApplicationUser applicationUser = new ApplicationUser
		{
			UserName = userInfo.UserName,
			FullName = userInfo.Name,
			Email = userInfo.Email,
			PhoneNumber = userInfo.PhoneNumber
		};

		var result = await _userManager.CreateAsync(applicationUser, userInfo.Password);

		if (result.Succeeded)
		{
			await _userManager.AddToRoleAsync(applicationUser, userInfo.Role);
			await _userManager.AddClaimAsync(applicationUser, new System.Security.Claims.Claim("FullName", userInfo.Name));

			#region Applicaion Log
			_logger.LogInformation("User created a new account with password.");
			#endregion

			#region Audit Log
			var log = new LogModel
			{
				UserName = _context.User.Identity.Name == null ? applicationUser.UserName : _context.User.Identity.Name,
				UserRole = _context.User.Identity.Name == null ? "SystemAdmin" : _context.User.Claims.First(c => c.Type.Contains("role")).Value,
				IP = Utility.GetIPAddress(_context.Request),
				TableName = "AspNetUsers",
				OldData = _context.User.Identity.Name == null ? null : $"<deleted Id=\"{applicationUser.Id}\" Name=\"{applicationUser.UserName}\" Feature=\"Register\" />",
				NewData = $"<inserted Id=\"{applicationUser.Id}\" Name=\"{applicationUser.UserName}\" Feature=\"Register\" />"
			};
			_ = Task.Run(async () => { await _auditLogRepository.InsertAuditLog(log); });
			#endregion

			#region Send Email
			//TODO: Make URL come from settings, also check if email confirmation works
			var code = await _userManager.GenerateEmailConfirmationTokenAsync(applicationUser);
			code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
			var callbackUrl = $@"http://localhost:5122/Identity/Account/ConfirmEmail?userId={applicationUser.Id}&code={code}";
			callbackUrl = HtmlEncoder.Default.Encode(callbackUrl);

			var emailTemplate = await _emailTemplateRepository.GetEmailTemplateByName("Confirm Email");
			emailTemplate.Template = emailTemplate.Template.Replace("$fullName", userInfo.Name);
			emailTemplate.Template = emailTemplate.Template.Replace("$callbackUrl", callbackUrl);

			_ = Task.Run(async () =>
			{
				await _emailSender.SendEmail(new EmailModel { To = userInfo.Email, Subject = emailTemplate.Subject, Body = emailTemplate.Template });
			});
			#endregion

			if (_userManager.Options.SignIn.RequireConfirmedAccount)
			{
				return new RegisterResponseModel { RequireConfirmedAccount = true };
			}
			else
			{
				await _signInManager.SignInAsync(applicationUser, isPersistent: false);
				return new RegisterResponseModel { SignedIn = true };
			}
		}
		else
		{
			return null;
		}
	}

	public async Task<UserInfoModel> Login(UserLoginModel userLogin)
	{
		var result = await _signInManager.PasswordSignInAsync(userLogin.UserName, userLogin.Password, isPersistent: false, lockoutOnFailure: false);

		if (result.Succeeded)
		{
			var user = await _userManager.FindByNameAsync(userLogin.UserName);
			UserInfoModel userInfo = new UserInfoModel
			{
				Id = user.Id,
				UserName = user.UserName,
				Name = user.FullName,
				Email = user.Email,
				Role = (await _userManager.GetRolesAsync(user)).First()
			};

			return userInfo;
		}
		else if (result.RequiresTwoFactor)
		{
			return null;
		}
		else if (result.IsLockedOut)
		{
			return null;
		}
		else
		{
			return null;
		}
	}

	public async Task<UserInfoModel> ChangePassword(ChangePasswordModel changePassword)
	{
		var applicationUser = await _userManager.FindByIdAsync(changePassword.UserId);

		if (applicationUser != null)
		{
			var changePasswordResult = await _userManager.ChangePasswordAsync(applicationUser, changePassword.CurrentPassword, changePassword.NewPassword);

			if (changePasswordResult.Succeeded)
			{
				await _signInManager.RefreshSignInAsync(applicationUser);
				_logger.LogInformation("User changed their password successfully.");

				#region Audit Log
				var log = new LogModel
				{
					UserName = _context.User.Identity.Name,
					UserRole = _context.User.Claims.First(c => c.Type.Contains("role")).Value,
					IP = Utility.GetIPAddress(_context.Request),
					TableName = "AspNetUsers",
					OldData = $"<deleted Id=\"{applicationUser.Id}\" Name=\"{applicationUser.UserName}\" Feature=\"ChangePassword\" />",
					NewData = $"<inserted Id=\"{applicationUser.Id}\" Name=\"{applicationUser.UserName}\" Feature=\"ChangePassword\" />"
				};
				_ = Task.Run(async () => { await _auditLogRepository.InsertAuditLog(log); });
				#endregion

				#region Send Email
				var emailTemplate = await _emailTemplateRepository.GetEmailTemplateByName("Reset Password");
				emailTemplate.Template = emailTemplate.Template.Replace("$fullName", applicationUser.FullName);
				emailTemplate.Template = emailTemplate.Template.Replace("$password", changePassword.NewPassword);

				_ = Task.Run(async () =>
				{
					await _emailSender.SendEmail(new EmailModel { To = applicationUser.Email, Subject = emailTemplate.Subject, Body = emailTemplate.Template });
				});
				#endregion

				UserInfoModel userInfo = new UserInfoModel
				{
					Id = applicationUser.Id,
					UserName = applicationUser.UserName,
					Name = applicationUser.FullName,
					Email = applicationUser.Email,
					Role = _context.User.Claims.First(c => c.Type.Contains("role")).Value
				};

				return userInfo;
			}
			else
			{
				return null;
			}
		}
		else
		{
			return null;
		}
	}

	public async Task UpdateRefreshToken(string userId, TokenModel token)
	{
		DynamicParameters p = new DynamicParameters();
		p.Add("UserId", userId);
		p.Add("RefreshToken", token.RefreshToken);
		p.Add("RefreshTokenExpires", token.RefreshTokenExpires);

		await _dataAccessHelper.ExecuteData("USP_AspNetUsers_TokenUpdate", p);
	}

	public async Task<TokenModel> GetRefreshToken(string userId)
	{
		return (await _dataAccessHelper.QueryData<TokenModel, dynamic>("USP_AspNetUsers_GetRefreshToken", new { UserId = userId })).FirstOrDefault();
	}

	public async Task<UserInfoModel> GetCurrentUser(string userId)
	{
		var user = await _userManager.FindByIdAsync(userId);
		if (user == null) return null;

		UserInfoModel userInfo = new UserInfoModel
		{
			Id = user.Id,
			UserName = user.UserName,
			Name = user.FullName,
			Email = user.Email,
			Role = (await _userManager.GetRolesAsync(user)).First()
		};

		return userInfo;
	}
}