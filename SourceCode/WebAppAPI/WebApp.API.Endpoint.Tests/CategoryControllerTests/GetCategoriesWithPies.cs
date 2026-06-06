using Microsoft.Extensions.DependencyInjection;
using WebApp.API.Endpoint.Tests.Base;
using WebApp.Core.Contract.Infrastructure;
using WebApp.Core.Model;
using Shouldly;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace WebApp.API.Endpoint.Tests.CategoryControllerTests;

public class GetCategoriesWithPies_Should : IClassFixture<CustomWebApplicationFactory>
{
	private readonly HttpClient _httpClient;
	private readonly ISecurityHelper _securityHelper;

	public GetCategoriesWithPies_Should(CustomWebApplicationFactory factory)
	{
		_httpClient = factory.GetAnonymousClient();

		using (var scope = factory.Services.CreateScope())
		{
			var scopedServices = scope.ServiceProvider;
			_securityHelper = scopedServices.GetRequiredService<ISecurityHelper>();
		};
	}

	[Fact]
	public async Task ReturnAllCategoriesWithPies()
	{
		if (_httpClient.DefaultRequestHeaders.Contains("x-hash")) _httpClient.DefaultRequestHeaders.Remove("x-hash");
		_httpClient.DefaultRequestHeaders.Add("x-hash", _securityHelper.GenerateHash());

		var response = await _httpClient.GetAsync($"v1/Category/GetCategoriesWithPies");
		var result = JsonSerializer.Deserialize<List<CategoryModel>>(await response.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		response.StatusCode.ShouldBe(HttpStatusCode.OK);
		result.ShouldBeOfType<List<CategoryModel>>();
	}
}