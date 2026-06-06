using WebApp.Core.Model;

namespace WebApp.Core.Contract.Infrastructure;

public interface ISecurityHelper
{
	string GenerateHash(string payload = "Default Payload");
	string GenerateJSONWebToken(UserInfoModel userInfo);
	string GenerateRefreshToken();
	bool IsValidHash(string senderHash, string payLoad = "Default Payload");
}