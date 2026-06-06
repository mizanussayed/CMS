/*
builder.Services.AddScoped<IEmailSender, EmailSenderSendGrid>();

_ = Task.Run(async () => { await _emailSender.SendEmail(
    new Email
    {
        To = "sharif2kb@yahoo.com",
        Subject = "Subject",
        Body = "Message"
    });
});
*/

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WebApp.Core.Contract.Infrastructure;
using WebApp.Core.Model;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace WebApp.Infrastructure;

public class EmailSenderSendGrid : IEmailSender
{
	private readonly ILogger<EmailSenderSendGrid> _logger;
	private readonly EmailSettingsSendGridModel _emailSettings;

	public EmailSenderSendGrid(ILogger<EmailSenderSendGrid> logger, IOptions<EmailSettingsSendGridModel> mailSettings)
	{
		this._logger = logger;
		this._emailSettings = mailSettings.Value;
	}

	public async Task SendEmail(EmailModel email)
	{
		var from = new EmailAddress
		{
			Email = _emailSettings.DisplayEmail,
			Name = _emailSettings.DisplayName
		};

		var to = new EmailAddress
		{
			Email = email.To
		};

		try
		{
			var client = new SendGridClient(_emailSettings.ApiKey);
			var sendGridMessage = MailHelper.CreateSingleEmail(from, to, email.Subject, email.Body, email.Body);
			var response = await client.SendEmailAsync(sendGridMessage);

			if (!(response.StatusCode == System.Net.HttpStatusCode.Accepted || response.StatusCode == System.Net.HttpStatusCode.OK))
				_ = Task.Run(() => { _logger.LogError("{Email}: " + response.StatusCode.ToString(), to.Email); });
		}
		catch (Exception ex)
		{
			_ = Task.Run(() => { _logger.LogError(ex, ex.Message); });
		}
	}
}