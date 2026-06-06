namespace WebApp.Core.Model;

public class ExportFileModel
{
    public string FileName { get; set; }
    public string ContentType { get; set; }
    public byte[] Data { get; set; }
}