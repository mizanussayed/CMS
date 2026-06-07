using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WebApp.Core.Model;
using System.Net;
using System.Net.Http.Json;

namespace WebApp.User.Service;

public class EmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _config;

    public EmailService(ILogger<EmailService> logger, IHttpClientFactory httpClientFactory, IConfiguration config)
    {
        this._logger = logger;
        this._httpClientFactory = httpClientFactory;
        this._config = config;
    }

    /// <summary>
    /// Sends a simple notification email via the API email endpoint.
    /// </summary>
    public async Task SendEmailAsync(string to, string subject, string body)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient("ServiceAPI");
            var emailModel = new EmailModel { To = to, Subject = subject, Body = body };

            var response = await httpClient.PostAsJsonAsync("v1/email/send", emailModel);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Email send failed ({Status}): {Error}", response.StatusCode, error);
            }
        }
        catch (Exception ex)
        {
            // Non-fatal: log and continue — email failure should not block the user workflow
            _ = Task.Run(() => _logger.LogError(ex, "Error sending email notification to {To}", to));
        }
    }
}
