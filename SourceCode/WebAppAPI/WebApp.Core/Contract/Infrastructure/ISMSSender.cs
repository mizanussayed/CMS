using WebApp.Core.Model;

namespace WebApp.Core.Contract.Infrastructure;

public interface ISMSSender
{
	Task SendSMS(SMSModel sms);
}