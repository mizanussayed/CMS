namespace WebApp.User.Service.Base;

public class BaseService
{
    private readonly HttpClient _httpClient;

    public BaseService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("ServiceAPI");
    }

    public HttpClient ConfigureClient()
    {
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        return _httpClient;
    }
}