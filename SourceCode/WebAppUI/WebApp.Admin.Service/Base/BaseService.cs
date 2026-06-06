using Microsoft.AspNetCore.Http;
using WebApp.Core.Infrastructure;
using WebApp.Core.Model;
using Polly;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;

namespace WebApp.Admin.Service.Base;

public class BaseService
{
    private readonly SecurityHelper _securityHelper;
    private readonly HttpClient _httpClient;
    private readonly HttpContext _context;

    public BaseService(SecurityHelper securityHelper, IHttpClientFactory httpClientFactory, IContextAccessor contextAccessor)
    {
        this._securityHelper = securityHelper;
        this._httpClient = httpClientFactory.CreateClient("ServiceAPI");
        this._context = contextAccessor.GetContext();
    }

    public async Task<HttpClient> ConfigureClient()
    {
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

        if (_context.Request.Cookies.TryGetValue("X-Token", out string authToken))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
        }
        else
        {
            var identity = _context.User.Identity as ClaimsIdentity;
            string userId = identity.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (userId != null)
            {
                TokenModel token = await RefreshToken(userId);
                if (token != null)
                {
                    _context.Response.Cookies.Append("X-Token", token.JwtToken, new CookieOptions { Expires = token.Expires, HttpOnly = true });
                    _context.Response.Cookies.Append("X-RefreshToken", token.RefreshToken, new CookieOptions { Expires = token.RefreshTokenExpires, HttpOnly = true });

                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.JwtToken);
                }
            }
        }

        return _httpClient;
    }

    public async Task<TokenModel> RefreshToken(string userId)
    {
        _httpClient.DefaultRequestHeaders.Add("x-hash", _securityHelper.GenerateHash());

        var response = await Policy
            .Handle<HttpRequestException>(ex =>
            {
                return true;
            })
            .WaitAndRetryAsync
            (
                1, retryAttempt => TimeSpan.FromSeconds(2)
            )
            .ExecuteAsync(async () =>
                await _httpClient.PostAsJsonAsync($"v1/Auth/RefreshToken", userId)
            );

        _httpClient.DefaultRequestHeaders.Remove("x-hash");
        if (response.StatusCode == HttpStatusCode.OK)
            return await response.Content.ReadFromJsonAsync<TokenModel>();

        return null;
    }
}