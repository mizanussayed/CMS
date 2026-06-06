namespace WebApp.Core.Model;

public class PaginatedListModel<T>
{
	public int PageIndex { get; set; }
	public int TotalRecords { get; set; }
	public int TotalPages { get; set; }
	public bool HasPreviousPage { get; set; }
	public bool HasNextPage { get; set; }
	public List<T> Items { get; set; }
}