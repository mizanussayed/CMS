namespace WebApp.Core.Contract.Infrastructure;

public interface ICsvExporter
{
	byte[] ExportToCsv<T>(List<T> items);
}