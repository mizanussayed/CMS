using Microsoft.Extensions.Logging;
using WebApp.Admin.Service.Base;
using WebApp.Core.Infrastructure;
using WebApp.Core.Model;
using Polly;
using System.Net;
using System.Net.Http.Json;

namespace WebApp.Admin.Service;

public class EmailService : BaseService
{
    private readonly ILogger<EmailService> _logger;
    private readonly SecurityHelper _securityHelper;
    private readonly HttpClient _httpClient;

    public EmailService(ILogger<EmailService> logger, SecurityHelper securityHelper, IHttpClientFactory httpClientFactory, IContextAccessor contextAccessor) : base(securityHelper, httpClientFactory, contextAccessor)
    {
        this._logger = logger;
        this._securityHelper = securityHelper;
        this._httpClient = ConfigureClient().GetAwaiter().GetResult();
    }

    public async Task SendEmail(EmailModel email)
    {
        _httpClient.DefaultRequestHeaders.Add("x-hash", _securityHelper.GenerateHash(email.To));

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
                    await _httpClient.PostAsJsonAsync($"v1/email/send", email)
                );

        switch (response.StatusCode)
        {
            case HttpStatusCode.NoContent:
                // Success
                break;
            default:
                throw new Exception(await response.Content.ReadAsStringAsync());
        }
    }
}