using Microsoft.Extensions.DependencyInjection;
using WebApp.API.Endpoint.Resources;
using WebApp.API.Endpoint.Tests.Base;
using WebApp.Core.Contract.Infrastructure;
using WebApp.Core.Model;
using Shouldly;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace WebApp.API.Endpoint.Tests.CategoryControllerTests;

public class DeleteCategory_Should : BaseService, IClassFixture<CustomWebApplicationFactory>
{
	private readonly HttpClient _httpClient;
	private readonly ContextModel _context;
	private readonly ISecurityHelper _securityHelper;

	public DeleteCategory_Should(CustomWebApplicationFactory factory)
	{
		_httpClient = factory.GetAnonymousClient();

		using (var scope = factory.Services.CreateScope())
		{
			var scopedServices = scope.ServiceProvider;

			_context = scopedServices.GetRequiredService<ContextModel>();
			_securityHelper = scopedServices.GetRequiredService<ISecurityHelper>();
		};

		_httpClient = AuthenticateClient(_httpClient, _context, _securityHelper).GetAwaiter().GetResult();
	}

	[Fact]
	public async Task ReturnNotFound_WhenIdDoesNotExist()
	{
		if (_httpClient.DefaultRequestHeaders.Contains("x-hash")) _httpClient.DefaultRequestHeaders.Remove("x-hash");
		_httpClient.DefaultRequestHeaders.Add("x-hash", _securityHelper.GenerateHash("168896"));
		LogModel logModel = new LogModel { UserName = "TestUser", UserRole = "Tester", IP = "0.0.0.0" };

		var response = await _httpClient.PutAsync($"v1/Category/Delete/168896", new StringContent(JsonSerializer.Serialize(logModel), Encoding.UTF8, "application/json"));
		var result = JsonSerializer.Deserialize<string>(await response.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
		result.ShouldBe(String.Format(ValidationMessages.Category_NotFoundId, "168896"));
	}


	[Fact]
	public async Task ReturnBadRequest_WhenIdIsZeroOrNegative()
	{
		if (_httpClient.DefaultRequestHeaders.Contains("x-hash")) _httpClient.DefaultRequestHeaders.Remove("x-hash");
		_httpClient.DefaultRequestHeaders.Add("x-hash", _securityHelper.GenerateHash("-56"));
		LogModel logModel = new LogModel { UserName = "TestUser", UserRole = "Tester", IP = "0.0.0.0" };

		var response = await _httpClient.PutAsync($"v1/Category/Delete/-56", new StringContent(JsonSerializer.Serialize(logModel), Encoding.UTF8, "application/json"));
		var result = JsonSerializer.Deserialize<string>(await response.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
		result.ShouldBe(String.Format(ValidationMessages.Category_InvalidId, "-56"));
	}
}