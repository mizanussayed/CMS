namespace WebApp.Core.Model;

public class LogModel
{
    public int Id { get; set; }
    public string UserName { get; set; }
    public string UserRole { get; set; }
    public string IP { get; set; }
    public string TableName { get; set; }
    public string Action { get; set; }
    public DateTime ActionDateTime { get; set; }
    public string OldData { get; set; }
    public string NewData { get; set; }
}