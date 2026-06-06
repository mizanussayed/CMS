using Microsoft.Extensions.Logging;
using WebApp.Admin.Service.Base;
using WebApp.Core.Infrastructure;
using WebApp.Core.Model;
using Polly;
using System.Net;
using System.Net.Http.Json;

namespace WebApp.Admin.Service;

public class EmailTemplateService : BaseService
{
    private readonly ILogger<EmailTemplateService> _logger;
    private readonly SecurityHelper _securityHelper;
    private readonly HttpClient _httpClient;

    public EmailTemplateService(ILogger<EmailTemplateService> logger, SecurityHelper securityHelper, IHttpClientFactory httpClientFactory, IContextAccessor contextAccessor) : base(securityHelper, httpClientFactory, contextAccessor)
    {
        this._logger = logger;
        this._securityHelper = securityHelper;
        this._httpClient = ConfigureClient().GetAwaiter().GetResult();
    }

    public async Task<PaginatedListModel<EmailTemplateModel>> GetEmailTemplates(int pageNumber)
    {
        _httpClient.DefaultRequestHeaders.Add("x-hash", _securityHelper.GenerateHash(pageNumber.ToString()));

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
                    await _httpClient.GetAsync($"v1/EmailTemplate?pagenumber={pageNumber}")
                );

        switch (response.StatusCode)
        {
            case HttpStatusCode.OK:
                return await response.Content.ReadFromJsonAsync<PaginatedListModel<EmailTemplateModel>>();
            default:
                throw new Exception(await response.Content.ReadAsStringAsync());
        }
    }

    public async Task<EmailTemplateModel> GetEmailTemplateById(int emailTemplateId)
    {
        _httpClient.DefaultRequestHeaders.Add("x-hash", _securityHelper.GenerateHash(emailTemplateId.ToString()));

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
                    await _httpClient.GetAsync($"v1/EmailTemplate/{emailTemplateId}")
                );

        switch (response.StatusCode)
        {
            case HttpStatusCode.OK:
                return await response.Content.ReadFromJsonAsync<EmailTemplateModel>();
            default:
                throw new Exception(await response.Content.ReadAsStringAsync());
        }
    }

    public async Task<EmailTemplateModel> GetEmailTemplateByName(string name)
    {
        _httpClient.DefaultRequestHeaders.Add("x-hash", _securityHelper.GenerateHash(name));

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
                    await _httpClient.GetAsync($"v1/EmailTemplate/GetByName/{name}")
                );

        switch (response.StatusCode)
        {
            case HttpStatusCode.OK:
                return await response.Content.ReadFromJsonAsync<EmailTemplateModel>();
            default:
                throw new Exception(await response.Content.ReadAsStringAsync());
        }
    }

    public async Task UpdateEmailTemplate(int emailTemplateId, EmailTemplateModel emailTemplate, LogModel logModel)
    {
        _httpClient.DefaultRequestHeaders.Add("x-hash", _securityHelper.GenerateHash(emailTemplateId.ToString()));

        Dictionary<string, object> PostData = new Dictionary<string, object> {
            {"Data", emailTemplate},
            {"Log", logModel}
        };

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
                    await _httpClient.PutAsJsonAsync($"v1/EmailTemplate/Update/{emailTemplateId}", PostData)
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