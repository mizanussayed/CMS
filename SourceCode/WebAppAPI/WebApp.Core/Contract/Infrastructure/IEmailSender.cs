using WebApp.Core.Model;

namespace WebApp.Core.Contract.Infrastructure;

public interface IEmailSender
{
	Task SendEmail(EmailModel email);
}