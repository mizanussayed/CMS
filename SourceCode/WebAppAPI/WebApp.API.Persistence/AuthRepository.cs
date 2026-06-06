using Dapper;
using Microsoft.AspNetCore.Identity;
using WebApp.API.Persistence.Identity;
using WebApp.Core.Contract.Persistence;
using WebApp.Core.Model;

namespace WebApp.API.Persistence;

public class AuthRepository : IAuthRepository
{
	private readonly UserManager<ApplicationUser> _userManager;
	private readonly IDataAccessHelper _dataAccessHelper;

	public AuthRepository(UserManager<ApplicationUser> userManager, IDataAccessHelper dataAccessHelper)
	{
		this._userManager = userManager;
		this._dataAccessHelper = dataAccessHelper;
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

	public async Task<TokenModel> GetRefreshToken(string userId)
	{
		return (await _dataAccessHelper.QueryData<TokenModel, dynamic>("USP_AspNetUsers_GetRefreshToken", new { UserId = userId })).FirstOrDefault();
	}

	public async Task UpdateRefreshToken(string userId, TokenModel token)
	{
		DynamicParameters p = new DynamicParameters();
		p.Add("UserId", userId);
		p.Add("RefreshToken", token.RefreshToken);
		p.Add("RefreshTokenExpires", token.RefreshTokenExpires);

		await _dataAccessHelper.ExecuteData("USP_AspNetUsers_TokenUpdate", p);
	}
}