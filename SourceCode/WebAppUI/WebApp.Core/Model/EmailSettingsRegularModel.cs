namespace WebApp.Core.Model;

public class EmailSettingsRegularModel
{
    public string SMTPHost { get; set; }
    public int Port { get; set; }
    public bool SSL { get; set; }
    public bool Authentication { get; set; }
    public string Password { get; set; }
    public string ReplyToEmail { get; set; }
    public string DisplayName { get; set; }
    public string DisplayEmail { get; set; }
}