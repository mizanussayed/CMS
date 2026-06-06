/*
builder.Services.AddScoped<ISMSSender, SMSSenderAlpha>();

_ = Task.Run(async () => { await _smsSender.SendSMS(
    new SMSModel
    {
        To = new List<string> { 
            "01712925546",
            "01712920000"
        },
        Content = "SMS from Easy Payment System (EPS)."
    });
});
*/

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WebApp.Core.Contract.Infrastructure;
using WebApp.Core.Model;
using Polly;
using System.Net;
using System.Net.Http.Headers;

namespace WebApp.Infrastructure;

public class SMSSenderAlpha : ISMSSender
{
	private readonly ILogger<SMSSenderAlpha> _logger;
	private readonly SMSSettingsModel _smsSettings;
	private readonly HttpClient _httpClient;

	public SMSSenderAlpha(ILogger<SMSSenderAlpha> logger, IOptions<SMSSettingsModel> smsSettings, IHttpClientFactory httpClientFactory)
	{
		this._logger = logger;
		this._smsSettings = smsSettings.Value;
		this._httpClient = httpClientFactory.CreateClient("SMSAPI");
		this._httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
	}

	public async Task SendSMS(SMSModel sms)
	{
		try
		{
			var response = await Policy
			.Handle<HttpRequestException>(ex =>
			{
				_ = Task.Run(() => { _logger.LogError(ex, ex.Message); });
				return true;
			})
			.WaitAndRetryAsync
			(
				1, retryAttempt => TimeSpan.FromSeconds(2)
			)
			.ExecuteAsync(async () =>
				await _httpClient.GetAsync($"sendsms?api_key={_smsSettings.ApiKey}&msg={sms.Content}&to={string.Join(",", sms.To)}")
			);

			if (response.StatusCode != HttpStatusCode.OK)
				_ = Task.Run(() => { _logger.LogError("{SMS}: " + response.StatusCode.ToString(), string.Join(",", sms.To)); });
		}
		catch (Exception ex)
		{
			_ = Task.Run(() => { _logger.LogError(ex, ex.Message); });
		}
	}
}