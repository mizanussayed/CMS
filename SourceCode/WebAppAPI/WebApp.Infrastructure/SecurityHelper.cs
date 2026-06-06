using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using WebApp.Core.Contract.Infrastructure;
using WebApp.Core.Model;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace WebApp.Infrastructure;

public class SecurityHelper : ISecurityHelper
{
	private readonly IConfiguration _config;

	public SecurityHelper(IConfiguration config)
	{
		this._config = config;
	}

	public string GenerateJSONWebToken(UserInfoModel userInfo)
	{
		List<Claim> claims = new();
		claims.Add(new(JwtRegisteredClaimNames.Sub, userInfo.Id));
		claims.Add(new(JwtRegisteredClaimNames.Name, userInfo.UserName));
		claims.Add(new(JwtRegisteredClaimNames.GivenName, userInfo.Name));
		claims.Add(new(JwtRegisteredClaimNames.Email, userInfo.Email));
		claims.Add(new("UserRole", userInfo.Role));

		var credentials = new SigningCredentials(
			new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWT:SecretKey"])),
			SecurityAlgorithms.HmacSha256
		);

		var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
		   _config["JWT:Issuer"],
		   _config["JWT:Audience"],
		   claims,
		   DateTime.UtcNow, // When this token becomes valid
		   DateTime.UtcNow.AddMinutes(Convert.ToInt32(_config["JWT:Expires"])), // When token will expire
		   credentials
		   );

		return new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(token);
	}

	public string GenerateRefreshToken()
	{
		return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
	}

	public string GenerateHash(string payload = "Default Payload")
	{
		using (var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(_config["Hash:HashKey"])))
		{
			byte[] data = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
			return Convert.ToBase64String(data);
		}
	}

	public bool IsValidHash(string senderHash, string payLoad = "Default Payload")
	{
		return (senderHash == GenerateHash(payLoad));
	}
}