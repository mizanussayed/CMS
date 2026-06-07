using Microsoft.Extensions.Logging;
using WebApp.Admin.Service.Base;
using WebApp.Core.Infrastructure;
using WebApp.Core.Model;
using Polly;
using System.Net;
using System.Net.Http.Json;

namespace WebApp.Admin.Service;

public class AppointmentService : BaseService
{
    private readonly ILogger<AppointmentService> _logger;
    private readonly SecurityHelper _securityHelper;
    private readonly HttpClient _httpClient;

    public AppointmentService(ILogger<AppointmentService> logger, SecurityHelper securityHelper, IHttpClientFactory httpClientFactory, IContextAccessor contextAccessor) : base(securityHelper, httpClientFactory, contextAccessor)
    {
        this._logger = logger;
        this._securityHelper = securityHelper;
        this._httpClient = ConfigureClient().GetAwaiter().GetResult();
    }

    public async Task<List<AppointmentModel>> GetAllAppointments()
    {
        _httpClient.DefaultRequestHeaders.Add("x-hash", _securityHelper.GenerateHash());

        var response = await Policy
                .Handle<HttpRequestException>(ex =>
                {
                    _ = Task.Run(() => { _logger.LogError(ex, ex.Message); });
                    return true;
                })
                .WaitAndRetryAsync(1, retryAttempt => TimeSpan.FromSeconds(2))
                .ExecuteAsync(async () => await _httpClient.GetAsync($"v1/appointment/GetAll"));

        switch (response.StatusCode)
        {
            case HttpStatusCode.OK:
                return await response.Content.ReadFromJsonAsync<List<AppointmentModel>>();
            default:
                throw new Exception(await response.Content.ReadAsStringAsync());
        }
    }

    public async Task UpdateAppointmentStatus(int appointmentId, string newStatus, LogModel logModel)
    {
        _httpClient.DefaultRequestHeaders.Add("x-hash", _securityHelper.GenerateHash(appointmentId.ToString()));

        var response = await Policy
                .Handle<HttpRequestException>(ex =>
                {
                    _ = Task.Run(() => { _logger.LogError(ex, ex.Message); });
                    return true;
                })
                .WaitAndRetryAsync(1, retryAttempt => TimeSpan.FromSeconds(2))
                .ExecuteAsync(async () => await _httpClient.PutAsJsonAsync($"v1/appointment/UpdateStatus/{appointmentId}", new { Status = newStatus, Log = logModel }));

        switch (response.StatusCode)
        {
            case HttpStatusCode.NoContent:
                return;
            default:
                throw new Exception(await response.Content.ReadAsStringAsync());
        }
    }
}
