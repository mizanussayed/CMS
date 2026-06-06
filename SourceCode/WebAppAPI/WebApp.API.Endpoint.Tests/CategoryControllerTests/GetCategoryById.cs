using Microsoft.Extensions.DependencyInjection;
using WebApp.API.Endpoint.Resources;
using WebApp.API.Endpoint.Tests.Base;
using WebApp.Core.Contract.Infrastructure;
using WebApp.Core.Model;
using Shouldly;
using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace WebApp.API.Endpoint.Tests.CategoryControllerTests;

public class GetCategoryById_Should : IClassFixture<CustomWebApplicationFactory>
{
	private readonly HttpClient _httpClient;
	private readonly ISecurityHelper _securityHelper;

	public GetCategoryById_Should(CustomWebApplicationFactory factory)
	{
		_httpClient = factory.GetAnonymousClient();

		using (var scope = factory.Services.CreateScope())
		{
			var scopedServices = scope.ServiceProvider;
			_securityHelper = scopedServices.GetRequiredService<ISecurityHelper>();
		};
	}

	[Fact]
	public async Task ReturnBadRequest_WhenIdIsZeroOrNegative()
	{
		if (_httpClient.DefaultRequestHeaders.Contains("x-hash")) _httpClient.DefaultRequestHeaders.Remove("x-hash");
		_httpClient.DefaultRequestHeaders.Add("x-hash", _securityHelper.GenerateHash("-56"));

		var response = await _httpClient.GetAsync($"v1/Category/-56");
		var result = JsonSerializer.Deserialize<string>(await response.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
		result.ShouldBe(String.Format(ValidationMessages.Category_InvalidId, -56));
	}

	[Fact]
	public async Task ReturnNotFound_WhenIdDoesNotExist()
	{
		if (_httpClient.DefaultRequestHeaders.Contains("x-hash")) _httpClient.DefaultRequestHeaders.Remove("x-hash");
		_httpClient.DefaultRequestHeaders.Add("x-hash", _securityHelper.GenerateHash("168896"));

		var response = await _httpClient.GetAsync($"v1/Category/168896");
		var result = JsonSerializer.Deserialize<string>(await response.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
		result.ShouldBe(String.Format(ValidationMessages.Category_NotFoundId, 168896));
	}

	[Fact]
	public async Task ReturnCategory_WhenIdExists()
	{
		if (_httpClient.DefaultRequestHeaders.Contains("x-hash")) _httpClient.DefaultRequestHeaders.Remove("x-hash");
		_httpClient.DefaultRequestHeaders.Add("x-hash", _securityHelper.GenerateHash("17"));

		var response = await _httpClient.GetAsync($"v1/Category/17");
		var result = JsonSerializer.Deserialize<CategoryModel>(await response.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		response.StatusCode.ShouldBe(HttpStatusCode.OK);
		result.ShouldBeOfType<CategoryModel>();
	}
}