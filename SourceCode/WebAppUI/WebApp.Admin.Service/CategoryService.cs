using Microsoft.Extensions.Logging;
using WebApp.Admin.Service.Base;
using WebApp.Core.Infrastructure;
using WebApp.Core.Model;
using Polly;
using System.Net;
using System.Net.Http.Json;

namespace WebApp.Admin.Service;

public class CategoryService : BaseService
{
    private readonly ILogger<CategoryService> _logger;
    private readonly SecurityHelper _securityHelper;
    private readonly HttpClient _httpClient;

    public CategoryService(ILogger<CategoryService> logger, SecurityHelper securityHelper, IHttpClientFactory httpClientFactory, IContextAccessor contextAccessor) : base(securityHelper, httpClientFactory, contextAccessor)
    {
        this._logger = logger;
        this._securityHelper = securityHelper;
        this._httpClient = ConfigureClient().GetAwaiter().GetResult();
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

    public async Task<CategoryModel> GetCategoryById(int categoryId)
    {
        _httpClient.DefaultRequestHeaders.Add("x-hash", _securityHelper.GenerateHash(categoryId.ToString()));

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
                    await _httpClient.GetAsync($"v1/Category/{categoryId}")
                );

        switch (response.StatusCode)
        {
            case HttpStatusCode.OK:
                return await response.Content.ReadFromJsonAsync<CategoryModel>();
            default:
                throw new Exception(await response.Content.ReadAsStringAsync());
        }
    }

    public async Task<CategoryModel> InsertCategory(CategoryModel category, LogModel logModel)
    {
        _httpClient.DefaultRequestHeaders.Add("x-hash", _securityHelper.GenerateHash(category.Name));

        Dictionary<string, object> PostData = new Dictionary<string, object> {
            {"Data", category},
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
                    await _httpClient.PostAsJsonAsync($"v1/Category", PostData)
                );

        switch (response.StatusCode)
        {
            case HttpStatusCode.Created:
                return await response.Content.ReadFromJsonAsync<CategoryModel>();
            default:
                throw new Exception(await response.Content.ReadAsStringAsync());
        }
    }

    public async Task UpdateCategory(int categoryId, CategoryModel category, LogModel logModel)
    {
        _httpClient.DefaultRequestHeaders.Add("x-hash", _securityHelper.GenerateHash(categoryId.ToString()));

        Dictionary<string, object> PostData = new Dictionary<string, object> {
            {"Data", category},
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
                    await _httpClient.PutAsJsonAsync($"v1/Category/Update/{categoryId}", PostData)
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

    public async Task DeleteCategory(int categoryId, LogModel logModel)
    {
        _httpClient.DefaultRequestHeaders.Add("x-hash", _securityHelper.GenerateHash(categoryId.ToString()));

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
                    await _httpClient.PutAsJsonAsync($"v1/Category/Delete/{categoryId}", logModel)
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