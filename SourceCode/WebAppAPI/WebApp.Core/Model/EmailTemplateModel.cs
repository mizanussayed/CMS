using System.ComponentModel.DataAnnotations;

namespace WebApp.Core.Model;

public class EmailTemplateModel : AuditModel
{
	public int Id { get; set; }
	public string Name { get; set; }

	[Required(ErrorMessage = "Please enter 'Subject'.")]
	[MinLength(3, ErrorMessage = "Minimum length of 'Subject' is 3 characters.")]
	[MaxLength(150, ErrorMessage = "Maximum length of 'Subject' is 150 characters.")]
	public string Subject { get; set; }

	[Required(ErrorMessage = "Please enter 'Template'.")]
	[MinLength(50, ErrorMessage = "Minimum length of 'Template' is 50 characters.")]
	[MaxLength(4000, ErrorMessage = "Maximum length of 'Template' is 4000 characters.")]
	public string Template { get; set; }

	public string Variables { get; set; }
}