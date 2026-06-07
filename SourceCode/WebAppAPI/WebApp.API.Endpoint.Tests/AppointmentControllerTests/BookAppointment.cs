using Microsoft.Extensions.DependencyInjection;
using WebApp.API.Endpoint.Tests.Base;
using WebApp.Core.Contract.Infrastructure;
using WebApp.Core.Model;
using Shouldly;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace WebApp.API.Endpoint.Tests.AppointmentControllerTests;

public class BookAppointment_Should : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _httpClient;
    private readonly ISecurityHelper _securityHelper;

    public BookAppointment_Should(CustomWebApplicationFactory factory)
    {
        _httpClient = factory.GetAnonymousClient();

        using (var scope = factory.Services.CreateScope())
        {
            var scopedServices = scope.ServiceProvider;
            _securityHelper = scopedServices.GetRequiredService<ISecurityHelper>();
        };
    }

    [Fact]
    public async Task BookAndReturnCreated()
    {
        // Prepare basic appointment payload
        var appointment = new AppointmentModel { UserId = 1, DoctorId = 1, AppointmentDate = System.DateTime.Now.AddDays(1) };
        var postData = new { Data = appointment, Log = new LogModel { UserName = "patient1@example.com", IP = "127.0.0.1" } };

        if (_httpClient.DefaultRequestHeaders.Contains("x-hash")) _httpClient.DefaultRequestHeaders.Remove("x-hash");
        _httpClient.DefaultRequestHeaders.Add("x-hash", _securityHelper.GenerateHash());

        var response = await _httpClient.PostAsJsonAsync($"v2/appointment/book", postData);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
    }
}
