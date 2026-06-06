using System.ComponentModel.DataAnnotations;

namespace WebApp.Core.Model;

public class CategoryModel : AuditModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Please enter 'Name'.")]
    [MinLength(3, ErrorMessage = "Minimum length of 'Name' is 3 characters.")]
    [MaxLength(150, ErrorMessage = "Maximum length of 'Name' is 150 characters.")]
    public string Name { get; set; }

    public List<PieModel> Pies { get; } = new List<PieModel>();
}