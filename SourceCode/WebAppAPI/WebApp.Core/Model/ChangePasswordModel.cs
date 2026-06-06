using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WebApp.Core.Model;

public class ChangePasswordModel
{
	[Required]
	public string UserId { get; set; }

	[Required]
	[DataType(DataType.Password)]
	[DisplayName("Current Password")]
	public string CurrentPassword { get; set; }

	[Required]
	[MaxLength(100, ErrorMessage = "Maximum length of 'New Password' is 100 characters.")]
	[DataType(DataType.Password)]
	[DisplayName("New Password")]
	public string NewPassword { get; set; }

	[Required]
	[Compare("NewPassword", ErrorMessage = "The new password and confirm new password do not match.")]
	[DataType(DataType.Password)]
	[DisplayName("Confirm New Password")]
	public string ConfirmNewPassword { get; set; }
}