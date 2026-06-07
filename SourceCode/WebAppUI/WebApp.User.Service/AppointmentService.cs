using Microsoft.Extensions.Logging;
using WebApp.Core.Infrastructure;
using WebApp.Core.Model;
using WebApp.User.Service.Base;
using Polly;
using System.Net;
using System.Net.Http.Json;

namespace WebApp.User.Service;

public class AppointmentService : BaseService
{
    private readonly ILogger<AppointmentService> _logger;
    private readonly SecurityHelper _securityHelper;
    private readonly HttpClient _httpClient;

    public AppointmentService(ILogger<AppointmentService> logger, SecurityHelper securityHelper, IHttpClientFactory httpClientFactory) : base(httpClientFactory)
    {
        this._logger = logger;
        this._securityHelper = securityHelper;
        this._httpClient = ConfigureClient();
    }

    public async Task<List<DoctorModel>> GetDoctors(int pageNumber)
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
                return (await response.Content.ReadFromJsonAsync<PaginatedListModel<DoctorModel>>()).Items;
            default:
                throw new Exception(await response.Content.ReadAsStringAsync());
        }
    }

    public async Task<int> BookAppointment(AppointmentModel appointment, LogModel logModel)
    {
        _httpClient.DefaultRequestHeaders.Add("x-hash", _securityHelper.GenerateHash(appointment.DoctorId.ToString()));

        Dictionary<string, object> PostData = new Dictionary<string, object> {
            {"Data", appointment},
            {"Log", logModel}
        };

        var response = await Policy
                .Handle<HttpRequestException>(ex =>
                {
                    _ = Task.Run(() => { _logger.LogError(ex, ex.Message); });
                    return true;
                })
                .WaitAndRetryAsync(1, retryAttempt => TimeSpan.FromSeconds(2))
                .ExecuteAsync(async () => await _httpClient.PostAsJsonAsync($"v1/appointment/book", PostData));

        switch (response.StatusCode)
        {
            case HttpStatusCode.Created:
                var model = await response.Content.ReadFromJsonAsync<AppointmentModel>();
                return model.Id;
            default:
                throw new Exception(await response.Content.ReadAsStringAsync());
        }
    }

    public async Task<List<AppointmentModel>> GetMyAppointments(int userId)
    {
        _httpClient.DefaultRequestHeaders.Add("x-hash", _securityHelper.GenerateHash(userId.ToString()));

        var response = await Policy
                .Handle<HttpRequestException>(ex =>
                {
                    _ = Task.Run(() => { _logger.LogError(ex, ex.Message); });
                    return true;
                })
                .WaitAndRetryAsync(1, retryAttempt => TimeSpan.FromSeconds(2))
                .ExecuteAsync(async () => await _httpClient.GetAsync($"v1/appointment/{userId}"));

        switch (response.StatusCode)
        {
            case HttpStatusCode.OK:
                return await response.Content.ReadFromJsonAsync<List<AppointmentModel>>();
            default:
                throw new Exception(await response.Content.ReadAsStringAsync());
        }
    }

    public async Task CancelAppointment(int appointmentId, LogModel logModel)
    {
        _httpClient.DefaultRequestHeaders.Add("x-hash", _securityHelper.GenerateHash(appointmentId.ToString()));

        var response = await Policy
                .Handle<HttpRequestException>(ex =>
                {
                    _ = Task.Run(() => { _logger.LogError(ex, ex.Message); });
                    return true;
                })
                .WaitAndRetryAsync(1, retryAttempt => TimeSpan.FromSeconds(2))
                .ExecuteAsync(async () => await _httpClient.PutAsJsonAsync($"v1/appointment/cancel/{appointmentId}", logModel));

        if (response.StatusCode != HttpStatusCode.NoContent)
            throw new Exception(await response.Content.ReadAsStringAsync());
    }
}
