using Microsoft.Extensions.Logging;
using Polly;
using System.Net;
using System.Net.Http.Json;
using WebApp.Core.Model;
using WebApp.Admin.Service.Base;
using WebApp.Core.Infrastructure;

namespace WebApp.Admin.Service;

public class AuthService : BaseService
{
    private readonly ILogger<AuthService> _logger;
    private readonly SecurityHelper _securityHelper;
    private readonly HttpClient _httpClient;

    public AuthService(ILogger<AuthService> logger, SecurityHelper securityHelper, IHttpClientFactory httpClientFactory, IContextAccessor contextAccessor) : base(securityHelper, httpClientFactory, contextAccessor)
    {
        this._logger = logger;
        this._securityHelper = securityHelper;
        this._httpClient = ConfigureClient().GetAwaiter().GetResult();
    }

    public async Task<TokenModel> GetToken(UserInfoModel userInfo)
    {
        _httpClient.DefaultRequestHeaders.Add("x-hash", _securityHelper.GenerateHash(userInfo.Id));

        var response = await Policy
                .Handle<HttpRequestException>(ex =>
                {
                    _ = Task.Run(() => { _logger.LogError(ex, ex.Message); });
                    return true;
                })
                .WaitAndRetryAsync
                (
                    1, retryAttempt => TimeSpan.FromSeconds(2)
                )
                .ExecuteAsync(async () =>
                    await _httpClient.PostAsJsonAsync($"v1/Auth/GetToken", userInfo)
                );

        switch (response.StatusCode)
        {
            case HttpStatusCode.OK:
                return await response.Content.ReadFromJsonAsync<TokenModel>();
            default:
                throw new Exception(await response.Content.ReadAsStringAsync());
        }
    }
}