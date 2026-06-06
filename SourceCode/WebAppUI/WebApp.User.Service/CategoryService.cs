using Microsoft.Extensions.Logging;
using Polly;
using System.Net;
using System.Net.Http.Json;
using WebApp.Core.Model;
using WebApp.User.Service.Base;
using WebApp.Core.Infrastructure;

namespace WebApp.User.Service;

public class CategoryService : BaseService
{
    private readonly ILogger<CategoryService> _logger;
    private readonly SecurityHelper _securityHelper;
    private readonly HttpClient _httpClient;

    public CategoryService(ILogger<CategoryService> logger, SecurityHelper securityHelper, IHttpClientFactory httpClientFactory) : base(httpClientFactory)
    {
        this._logger = logger;
        this._securityHelper = securityHelper;
        this._httpClient = ConfigureClient();
    }

    public async Task<PaginatedListModel<CategoryModel>> GetCategories(int pageNumber)
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
                await _httpClient.GetAsync($"v1/category?pagenumber={pageNumber}")
            );

        switch (response.StatusCode)
        {
            case HttpStatusCode.OK:
                return await response.Content.ReadFromJsonAsync<PaginatedListModel<CategoryModel>>();
            default:
                throw new Exception(await response.Content.ReadAsStringAsync());
        }
    }

    public async Task<List<CategoryModel>> GetDistinctCategories()
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
                await _httpClient.GetAsync($"v1/category/GetDistinctCategories")
            );

        switch (response.StatusCode)
        {
            case HttpStatusCode.OK:
                return await response.Content.ReadFromJsonAsync<List<CategoryModel>>();
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
                await _httpClient.GetAsync($"v1/category/Export")
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