using Microsoft.Extensions.Logging;
using WebApp.Admin.Service.Base;
using WebApp.Core.Infrastructure;
using WebApp.Core.Model;
using Polly;
using System.Net;
using System.Net.Http.Json;

namespace WebApp.Admin.Service;

public class PieService : BaseService
{
    private readonly ILogger<PieService> _logger;
    private readonly SecurityHelper _securityHelper;
    private readonly HttpClient _httpClient;

    public PieService(ILogger<PieService> logger, SecurityHelper securityHelper, IHttpClientFactory httpClientFactory, IContextAccessor contextAccessor) : base(securityHelper, httpClientFactory, contextAccessor)
    {
        this._logger = logger;
        this._securityHelper = securityHelper;
        this._httpClient = ConfigureClient().GetAwaiter().GetResult();
    }

    public async Task<PaginatedListModel<PieModel>> GetPies(int pageNumber)
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
                    await _httpClient.GetAsync($"v1/pie?pagenumber={pageNumber}")
                );

        switch (response.StatusCode)
        {
            case HttpStatusCode.OK:
                return await response.Content.ReadFromJsonAsync<PaginatedListModel<PieModel>>();
            default:
                throw new Exception(await response.Content.ReadAsStringAsync());
        }
    }

    public async Task<PieModel> GetPieById(int pieId)
    {
        _httpClient.DefaultRequestHeaders.Add("x-hash", _securityHelper.GenerateHash(pieId.ToString()));

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
                    await _httpClient.GetAsync($"v1/Pie/{pieId}")
                );

        switch (response.StatusCode)
        {
            case HttpStatusCode.OK:
                return await response.Content.ReadFromJsonAsync<PieModel>();
            default:
                throw new Exception(await response.Content.ReadAsStringAsync());
        }
    }

    public async Task<List<PieModel>> GetPieByCategoryId(int categoryId)
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
                    await _httpClient.GetAsync($"v1/Pie/GetPieByCategoryId/{categoryId}")
                );

        switch (response.StatusCode)
        {
            case HttpStatusCode.OK:
                return await response.Content.ReadFromJsonAsync<List<PieModel>>();
            default:
                throw new Exception(await response.Content.ReadAsStringAsync());
        }
    }

    public async Task<PieModel> InsertPie(PieModel pie, LogModel logModel)
    {
        _httpClient.DefaultRequestHeaders.Add("x-hash", _securityHelper.GenerateHash(pie.Name));

        Dictionary<string, object> PostData = new Dictionary<string, object> {
            {"Data", pie},
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
                    await _httpClient.PostAsJsonAsync($"v1/Pie", PostData)
                );

        switch (response.StatusCode)
        {
            case HttpStatusCode.Created:
                return await response.Content.ReadFromJsonAsync<PieModel>();
            default:
                throw new Exception(await response.Content.ReadAsStringAsync());
        }
    }

    public async Task UpdatePie(int pieId, PieModel pie, LogModel logModel)
    {
        _httpClient.DefaultRequestHeaders.Add("x-hash", _securityHelper.GenerateHash(pieId.ToString()));

        Dictionary<string, object> PostData = new Dictionary<string, object> {
            {"Data", pie},
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
                    await _httpClient.PutAsJsonAsync($"v1/Pie/Update/{pieId}", PostData)
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

    public async Task DeletePie(int pieId, LogModel logModel)
    {
        _httpClient.DefaultRequestHeaders.Add("x-hash", _securityHelper.GenerateHash(pieId.ToString()));

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
                    await _httpClient.PutAsJsonAsync($"v1/Pie/Delete/{pieId}", logModel)
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
                await _httpClient.GetAsync($"v1/Pie/Export")
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