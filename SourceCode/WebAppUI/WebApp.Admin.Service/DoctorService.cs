using Microsoft.Extensions.Logging;
using WebApp.Admin.Service.Base;
using WebApp.Core.Infrastructure;
using WebApp.Core.Model;
using Polly;
using System.Net;
using System.Net.Http.Json;

namespace WebApp.Admin.Service;

public class DoctorService : BaseService
{
    private readonly ILogger<DoctorService> _logger;
    private readonly SecurityHelper _securityHelper;
    private readonly HttpClient _httpClient;

    public DoctorService(ILogger<DoctorService> logger, SecurityHelper securityHelper, IHttpClientFactory httpClientFactory, IContextAccessor contextAccessor) : base(securityHelper, httpClientFactory, contextAccessor)
    {
        this._logger = logger;
        this._securityHelper = securityHelper;
        this._httpClient = ConfigureClient().GetAwaiter().GetResult();
    }

    public async Task<PaginatedListModel<DoctorModel>> GetDoctors(int pageNumber)
    {
        _httpClient.DefaultRequestHeaders.Add("x-hash", _securityHelper.GenerateHash(pageNumber.ToString()));

        var response = await Policy
                .Handle<HttpRequestException>(ex =>
                {
                    _ = Task.Run(() => { _logger.LogError(ex, ex.Message); });
                    return true;
                })
                .WaitAndRetryAsync(1, retryAttempt => TimeSpan.FromSeconds(2))
                .ExecuteAsync(async () => await _httpClient.GetAsync($"v1/doctor?pagenumber={pageNumber}"));

        switch (response.StatusCode)
        {
            case HttpStatusCode.OK:
                return await response.Content.ReadFromJsonAsync<PaginatedListModel<DoctorModel>>();
            default:
                throw new Exception(await response.Content.ReadAsStringAsync());
        }
    }

    public async Task<DoctorModel> GetDoctorById(int doctorId)
    {
        _httpClient.DefaultRequestHeaders.Add("x-hash", _securityHelper.GenerateHash(doctorId.ToString()));

        var response = await Policy
                .Handle<HttpRequestException>(ex =>
                {
                    _ = Task.Run(() => { _logger.LogError(ex, ex.Message); });
                    return true;
                })
                .WaitAndRetryAsync(1, retryAttempt => TimeSpan.FromSeconds(2))
                .ExecuteAsync(async () => await _httpClient.GetAsync($"v1/doctor/{doctorId}"));

        switch (response.StatusCode)
        {
            case HttpStatusCode.OK:
                return await response.Content.ReadFromJsonAsync<DoctorModel>();
            default:
                throw new Exception(await response.Content.ReadAsStringAsync());
        }
    }

    public async Task<DoctorModel> InsertDoctor(DoctorModel doctor, LogModel logModel)
    {
        _httpClient.DefaultRequestHeaders.Add("x-hash", _securityHelper.GenerateHash(doctor.Name));

        Dictionary<string, object> PostData = new Dictionary<string, object> {
            {"Data", doctor},
            {"Log", logModel}
        };

        var response = await Policy
                .Handle<HttpRequestException>(ex =>
                {
                    _ = Task.Run(() => { _logger.LogError(ex, ex.Message); });
                    return true;
                })
                .WaitAndRetryAsync(1, retryAttempt => TimeSpan.FromSeconds(2))
                .ExecuteAsync(async () => await _httpClient.PostAsJsonAsync($"v1/doctor", PostData));

        switch (response.StatusCode)
        {
            case HttpStatusCode.Created:
                return await response.Content.ReadFromJsonAsync<DoctorModel>();
            default:
                throw new Exception(await response.Content.ReadAsStringAsync());
        }
    }

    public async Task UpdateDoctor(int doctorId, DoctorModel doctor, LogModel logModel)
    {
        _httpClient.DefaultRequestHeaders.Add("x-hash", _securityHelper.GenerateHash(doctorId.ToString()));

        Dictionary<string, object> PostData = new Dictionary<string, object> {
            {"Data", doctor},
            {"Log", logModel}
        };

        var response = await Policy
                .Handle<HttpRequestException>(ex =>
                {
                    _ = Task.Run(() => { _logger.LogError(ex, ex.Message); });
                    return true;
                })
                .WaitAndRetryAsync(1, retryAttempt => TimeSpan.FromSeconds(2))
                .ExecuteAsync(async () => await _httpClient.PutAsJsonAsync($"v1/doctor/Update/{doctorId}", PostData));

        if (response.StatusCode != HttpStatusCode.NoContent)
            throw new Exception(await response.Content.ReadAsStringAsync());
    }
}
