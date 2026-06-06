/*
builder.Services.AddScoped<IEmailSender, EmailSenderRegular>();

_ = Task.Run(async () => { await _emailSender.SendEmail(
    new EmailModel
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
using System.Net;
using System.Net.Mail;

namespace WebApp.Infrastructure;

public class EmailSenderRegular : IEmailSender
{
    private string recipient;

    private readonly ILogger<EmailSenderRegular> _logger;
    private readonly EmailSettingsRegularModel _emailSettings;

    public EmailSenderRegular(ILogger<EmailSenderRegular> logger, IOptions<EmailSettingsRegularModel> mailSettings)
    {
        this._logger = logger;
        this._emailSettings = mailSettings.Value;
    }

#pragma warning disable CS1998 // This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.
    public async Task SendEmail(EmailModel email)
#pragma warning restore CS1998 // This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.
    {
        MailMessage mailMessage;
        SmtpClient client;
        recipient = email.To;

        try
        {
            mailMessage = new MailMessage(new MailAddress(_emailSettings.DisplayEmail, _emailSettings.DisplayName), new MailAddress(email.To));
            mailMessage.ReplyToList.Add(_emailSettings.ReplyToEmail);
            mailMessage.IsBodyHtml = true;
            mailMessage.Subject = email.Subject;
            mailMessage.Body = email.Body;
            mailMessage.Priority = MailPriority.Normal;

            client = new SmtpClient();
            client.Port = _emailSettings.Port;
            client.EnableSsl = _emailSettings.SSL;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = _emailSettings.Authentication;
            client.Timeout = 10000; // Millisecond
            client.Host = _emailSettings.SMTPHost;
            if (_emailSettings.Authentication)
                client.Credentials = new NetworkCredential(_emailSettings.DisplayEmail, _emailSettings.Password);

            client.SendCompleted += new SendCompletedEventHandler(Client_SendCompleted);
            client.SendAsync(mailMessage, null);

            mailMessage.Dispose();
        }
        catch (Exception ex)
        {
            _ = Task.Run(() => { _logger.LogError(ex, ex.Message); });
        }
    }

    private void Client_SendCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
    {
        if (!e.Cancelled && e.Error == null)
        {
            _ = Task.Run(() => { _logger.LogInformation("Email sent to {Recipient} successfully.", recipient); });
        }
        else
        {
            _ = Task.Run(() => { _logger.LogError("Email could not be sent to {Recipient} due to: {ErrorMessage}", recipient, e.Error.Message); });
        }
    }
}