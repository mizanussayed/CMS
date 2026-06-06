using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;

namespace WebApp.API.Endpoint.Tests.Base;

public class CustomWebApplicationFactory : WebApplicationFactory<Startup>
{
	protected override void ConfigureWebHost(IWebHostBuilder builder)
	{
		builder.ConfigureServices(services =>
		{
			services.AddSingleton(new ContextModel());
		});
	}

	protected override void ConfigureClient(HttpClient client)
	{
		client.DefaultRequestHeaders.Add("Accept", "application/json");
		base.ConfigureClient(client);
	}

	public HttpClient GetAnonymousClient()
	{
		return CreateClient();
	}
}