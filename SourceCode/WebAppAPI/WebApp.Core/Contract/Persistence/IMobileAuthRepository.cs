using WebApp.Core.Model;

namespace WebApp.Core.Contract.Persistence;

public interface IMobileAuthRepository
{
	Task<RegisterResponseModel> Register(UserInfoModel userInfo);
	Task<UserInfoModel> Login(UserLoginModel userLogin);
	Task<UserInfoModel> ChangePassword(ChangePasswordModel changePassword);
	Task UpdateRefreshToken(string userId, TokenModel token);
	Task<TokenModel> GetRefreshToken(string userId);
	Task<UserInfoModel> GetCurrentUser(string userId);
}