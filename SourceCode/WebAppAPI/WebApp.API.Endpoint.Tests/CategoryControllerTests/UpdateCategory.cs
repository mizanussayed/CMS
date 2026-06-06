using Microsoft.Extensions.DependencyInjection;
using WebApp.API.Endpoint.Resources;
using WebApp.API.Endpoint.Tests.Base;
using WebApp.Core.Contract.Infrastructure;
using WebApp.Core.Model;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace WebApp.API.Endpoint.Tests.CategoryControllerTests;

public class UpdateCategory_Should : BaseService, IClassFixture<CustomWebApplicationFactory>
{
	private HttpResponseMessage response;

	private readonly HttpClient _httpClient;
	private readonly ContextModel _context;
	private readonly ISecurityHelper _securityHelper;

	public UpdateCategory_Should(CustomWebApplicationFactory factory)
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
	public async Task ReturnBadRequest_WhenIdIsZeroOrNegative()
	{
		if (_httpClient.DefaultRequestHeaders.Contains("x-hash")) _httpClient.DefaultRequestHeaders.Remove("x-hash");
		_httpClient.DefaultRequestHeaders.Add("x-hash", _securityHelper.GenerateHash("-56"));
		Dictionary<string, object> postData = new Dictionary<string, object> {
			{"Data", new CategoryModel {Id = 16, Name = "Pie_Updated"} },
			{"Log", new LogModel {UserName = "TestUser", UserRole = "Tester", IP = "0.0.0.0"} }
		};

		var response = await _httpClient.PutAsync($"v1/Category/Update/-56", new StringContent(JsonSerializer.Serialize(postData), Encoding.UTF8, "application/json"));
		var result = JsonSerializer.Deserialize<string>(await response.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
		result.ShouldBe(String.Format(ValidationMessages.Category_InvalidId, -56));
	}

	[Fact]
	public async Task ReturnBadRequest_WhenCategoryIsNull()
	{
		if (_httpClient.DefaultRequestHeaders.Contains("x-hash")) _httpClient.DefaultRequestHeaders.Remove("x-hash");
		_httpClient.DefaultRequestHeaders.Add("x-hash", _securityHelper.GenerateHash("16"));
		Dictionary<string, object> postData = new Dictionary<string, object> {
			{"Data", null },
			{"Log", new LogModel {UserName = "TestUser", UserRole = "Tester", IP = "0.0.0.0"} }
		};

		var response = await _httpClient.PutAsync($"v1/Category/Update/16", new StringContent(JsonSerializer.Serialize(postData), Encoding.UTF8, "application/json"));
		var result = JsonSerializer.Deserialize<string>(await response.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
		result.ShouldBe(ValidationMessages.Category_Null);
	}

	[Fact]
	public async Task ReturnBadRequest_WhenIdDoesNotMatch()
	{
		if (_httpClient.DefaultRequestHeaders.Contains("x-hash")) _httpClient.DefaultRequestHeaders.Remove("x-hash");
		_httpClient.DefaultRequestHeaders.Add("x-hash", _securityHelper.GenerateHash("17"));
		Dictionary<string, object> postData = new Dictionary<string, object> {
			{"Data", new CategoryModel {Id = 16, Name = "Pie_Updated"} },
			{"Log", new LogModel {UserName = "TestUser", UserRole = "Tester", IP = "0.0.0.0"} }
		};

		var response = await _httpClient.PutAsync($"v1/Category/Update/17", new StringContent(JsonSerializer.Serialize(postData), Encoding.UTF8, "application/json"));
		var result = JsonSerializer.Deserialize<string>(await response.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
		result.ShouldBe(ValidationMessages.Category_Mismatch);
	}

	[Fact]
	public async Task ReturnNotFound_WhenIdDoesNotExist()
	{
		if (_httpClient.DefaultRequestHeaders.Contains("x-hash")) _httpClient.DefaultRequestHeaders.Remove("x-hash");
		_httpClient.DefaultRequestHeaders.Add("x-hash", _securityHelper.GenerateHash("98"));
		Dictionary<string, object> postData = new Dictionary<string, object> {
			{"Data", new CategoryModel {Id = 98, Name = "Pie_Updated"} },
			{"Log", new LogModel {UserName = "TestUser", UserRole = "Tester", IP = "0.0.0.0"} }
		};

		var response = await _httpClient.PutAsync($"v1/Category/Update/98", new StringContent(JsonSerializer.Serialize(postData), Encoding.UTF8, "application/json"));
		var result = JsonSerializer.Deserialize<string>(await response.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
		result.ShouldBe(String.Format(ValidationMessages.Category_NotFoundId, 98));
	}

	[Fact]
	public async Task UpdateCategory_WhenIdAndCategoryExists()
	{
		// Update
		if (_httpClient.DefaultRequestHeaders.Contains("x-hash")) _httpClient.DefaultRequestHeaders.Remove("x-hash");
		_httpClient.DefaultRequestHeaders.Add("x-hash", _securityHelper.GenerateHash("16"));
		Dictionary<string, object> postData = new Dictionary<string, object> {
			{"Data", new CategoryModel {Id = 16, Name = "Pie_Updated"} },
			{"Log", new LogModel {UserName = "TestUser", UserRole = "Tester", IP = "0.0.0.0"} }
		};

		response = await _httpClient.PutAsync($"v1/Category/Update/16", new StringContent(JsonSerializer.Serialize(postData), Encoding.UTF8, "application/json"));

		response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

		// Get and check if the value changed
		if (_httpClient.DefaultRequestHeaders.Contains("x-hash"))
			_httpClient.DefaultRequestHeaders.Remove("x-hash");
		_httpClient.DefaultRequestHeaders.Add("x-hash", _securityHelper.GenerateHash("16"));

		response = await _httpClient.GetAsync($"v1/Category/16");
		var result = JsonSerializer.Deserialize<CategoryModel>(await response.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		response.StatusCode.ShouldBe(HttpStatusCode.OK);
		result.ShouldBeOfType<CategoryModel>();
		result.Name.ShouldBe("Pie_Updated");
	}
}