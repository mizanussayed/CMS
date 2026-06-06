using WebApp.Core.Contract.Infrastructure;
using WebApp.Core.Model;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace WebApp.API.Endpoint.Tests.Base;

public class BaseService
{
	public async Task<HttpClient> AuthenticateClient(HttpClient httpClient, ContextModel context, ISecurityHelper securityHelper)
	{
		if (!string.IsNullOrEmpty(context.JwtToken))
		{
			httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", context.JwtToken);
		}
		else
		{
			UserInfoModel userInfo = new UserInfoModel
			{
				Id = "e1ae1f42-75b2-4604-97ec-10f844b1962f",
				UserName = "sharif2kb@yahoo.com",
				Name = "Shariful Islam",
				Email = "sharif2kb@yahoo.com",
				Role = "SystemAdmin"
			};

			if (httpClient.DefaultRequestHeaders.Contains("x-hash"))
				httpClient.DefaultRequestHeaders.Remove("x-hash");
			httpClient.DefaultRequestHeaders.Add("x-hash", securityHelper.GenerateHash(userInfo.Id));
			HttpResponseMessage response = await httpClient.PostAsync($"v1/Auth/GetToken", new StringContent(JsonSerializer.Serialize(userInfo), Encoding.UTF8, "application/json"));
			TokenModel token = JsonSerializer.Deserialize<TokenModel>(await response.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

			if (token != null)
			{
				context.JwtToken = token.JwtToken;
				httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.JwtToken);
			}
		}

		return httpClient;
	}
}