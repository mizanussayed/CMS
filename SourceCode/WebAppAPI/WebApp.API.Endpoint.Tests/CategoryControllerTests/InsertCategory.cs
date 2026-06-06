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

public class InsertCategory_Should : BaseService, IClassFixture<CustomWebApplicationFactory>
{
	private HttpResponseMessage response;

	private readonly HttpClient _httpClient;
	private readonly ContextModel _context;
	private readonly ISecurityHelper _securityHelper;

	public InsertCategory_Should(CustomWebApplicationFactory factory)
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
	public async Task ReturnBadRequest_WhenCategoryIsNull()
	{
		Dictionary<string, object> postData = new Dictionary<string, object> {
			{"Data", null },
			{"Log", new LogModel {UserName = "TestUser", UserRole = "Tester", IP = "0.0.0.0"} }
		};

		response = await _httpClient.PostAsync($"v1/Category", new StringContent(JsonSerializer.Serialize(postData), Encoding.UTF8, "application/json"));
		var result = JsonSerializer.Deserialize<string>(await response.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
		result.ShouldBe(ValidationMessages.Category_Null);
	}

	[Fact]
	public async Task ReturnBadRequest_WhenNameIsDuplicate()
	{
		if (_httpClient.DefaultRequestHeaders.Contains("x-hash")) _httpClient.DefaultRequestHeaders.Remove("x-hash");
		_httpClient.DefaultRequestHeaders.Add("x-hash", _securityHelper.GenerateHash("Cake"));
		Dictionary<string, object> postData = new Dictionary<string, object> {
			{"Data", new CategoryModel {Name = "Cake"} },
			{"Log", new LogModel {UserName = "TestUser", UserRole = "Tester", IP = "0.0.0.0"} }
		};

		response = await _httpClient.PostAsync($"v1/Category", new StringContent(JsonSerializer.Serialize(postData), Encoding.UTF8, "application/json"));
		var result = JsonSerializer.Deserialize<string>(await response.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
		result.ShouldBe(String.Format(ValidationMessages.Category_Duplicate, "Cake"));
	}

	[Fact(Skip = "true", DisplayName = "Skipping for execute query")]
	public async Task InsertCategory_WhenCategoryIsNotNullAndNotDuplicate()
	{
		if (_httpClient.DefaultRequestHeaders.Contains("x-hash")) _httpClient.DefaultRequestHeaders.Remove("x-hash");
		_httpClient.DefaultRequestHeaders.Add("x-hash", _securityHelper.GenerateHash("Fruit"));
		Dictionary<string, object> postData = new Dictionary<string, object> {
			{"Data", new CategoryModel {Name = "Fruit"} },
			{"Log", new LogModel {UserName = "TestUser", UserRole = "Tester", IP = "0.0.0.0"} }
		};

		response = await _httpClient.PostAsync($"v1/Category", new StringContent(JsonSerializer.Serialize(postData), Encoding.UTF8, "application/json"));

		response.StatusCode.ShouldBe(HttpStatusCode.Created);
	}
}