using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WebApp.Core.Model;

public class PieModel : AuditModel
{
	public int Id { get; set; }

	[Required(ErrorMessage = "Please enter 'Name'.")]
	[MinLength(3, ErrorMessage = "Minimum length of 'Name' is 3 characters.")]
	[MaxLength(150, ErrorMessage = "Maximum length of 'Name' is 150 characters.")]
	public string Name { get; set; }

	[DisplayName("Category")]
	[Range(1, int.MaxValue, ErrorMessage = "Please select a 'Category'.")]
	public int CategoryId { get; set; }

	public string CategoryName { get; set; }

	[DataType(DataType.Currency)]
	[Range(0, int.MaxValue, ErrorMessage = "Price can not be negative.")]
	public double Price { get; set; }

	[DisplayName("Expiry Date")]
	[Required(ErrorMessage = "Please enter 'Expiry Date'.")]
	[DisplayFormat(DataFormatString = "{0:dd/MMMM/yyyy}", ApplyFormatInEditMode = true)]
	public DateTime ExpiryDate { get; set; } = DateTime.Now.AddDays(30);

	[DisplayName("In Stock")]
	public bool InStock { get; set; }

	[Required(ErrorMessage = "Please enter 'Description'.")]
	[MinLength(50, ErrorMessage = "Minimum length of 'Description' is 50 characters.")]
	[MaxLength(4000, ErrorMessage = "Maximum length of 'Description' is 4000 characters.")]
	public string Description { get; set; }

	public string? ImageUrl { get; set; }
}