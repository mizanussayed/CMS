using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WebApp.Core.Model;

public class UserLoginModel
{
	[Required(ErrorMessage = "Please enter 'User Name'.")]
	[MinLength(4, ErrorMessage = "Minimum length of 'User Name' is 4 characters.")]
	[MaxLength(200, ErrorMessage = "Maximum length of 'User Name' is 200 characters.")]
	[DisplayName("User Name")]
	public string UserName { get; set; }

	[Required(ErrorMessage = "Please enter 'Password'.")]
	[MinLength(6, ErrorMessage = "Minimum length of 'Password' is 6 characters.")]
	[MaxLength(50, ErrorMessage = "Maximum length of 'Password' is 50 characters.")]
	public string Password { get; set; }
}