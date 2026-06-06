using Microsoft.Extensions.Logging;
using WebApp.Admin.Service.Base;
using WebApp.Core.Infrastructure;
using WebApp.Core.Model;
using Polly;
using System.Net;
using System.Net.Http.Json;

namespace WebApp.Admin.Service;

public class ApplicationLogService : BaseService
{
    private readonly ILogger<ApplicationLogService> _logger;
    private readonly SecurityHelper _securityHelper;
    private readonly HttpClient _httpClient;

    public ApplicationLogService(ILogger<ApplicationLogService> logger, SecurityHelper securityHelper, IHttpClientFactory httpClientFactory, IContextAccessor contextAccessor) : base(securityHelper, httpClientFactory, contextAccessor)
    {
        this._logger = logger;
        this._securityHelper = securityHelper;
        this._httpClient = ConfigureClient().GetAwaiter().GetResult();
    }

    public async Task<PaginatedListModel<ApplicationLogModel>> GetApplicationLogs(int pageNumber)
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
                    await _httpClient.GetAsync($"v1/ApplicationLog?pagenumber={pageNumber}")
                );

        switch (response.StatusCode)
        {
            case HttpStatusCode.OK:
                return await response.Content.ReadFromJsonAsync<PaginatedListModel<ApplicationLogModel>>();
            default:
                throw new Exception(await response.Content.ReadAsStringAsync());
        }
    }

    public async Task<ApplicationLogModel> GetAuditLogById(int logId)
    {
        _httpClient.DefaultRequestHeaders.Add("x-hash", _securityHelper.GenerateHash(logId.ToString()));

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
                await _httpClient.GetAsync($"v1/ApplicationLog/{logId}")
            );

        switch (response.StatusCode)
        {
            case HttpStatusCode.OK:
                return await response.Content.ReadFromJsonAsync<ApplicationLogModel>();
            default:
                throw new Exception(await response.Content.ReadAsStringAsync());
        }
    }

    public async Task DeleteApplicationLog(int auditLogId)
    {
        _httpClient.DefaultRequestHeaders.Add("x-hash", _securityHelper.GenerateHash(auditLogId.ToString()));

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
                    await _httpClient.DeleteAsync($"v1/ApplicationLog/{auditLogId}")
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

    public async Task<ExportFileModel> Export()
    {
        _httpClient.DefaultRequestHeaders.Add("x-hash", _securityHelper.GenerateHash());

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
                await _httpClient.GetAsync($"v1/ApplicationLog/Export")
            );

        switch (response.StatusCode)
        {
            case HttpStatusCode.OK:
                return await response.Content.ReadFromJsonAsync<ExportFileModel>();
            default:
                throw new Exception(await response.Content.ReadAsStringAsync());
        }
    }
}