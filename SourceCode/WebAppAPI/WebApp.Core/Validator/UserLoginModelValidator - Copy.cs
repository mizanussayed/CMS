using FluentValidation;
using WebApp.Core.Model;

namespace WebApp.Core.Validator;

public class ChangePasswordModelValidator : AbstractValidator<ChangePasswordModel>
{
	public ChangePasswordModelValidator()
	{
		RuleFor(p => p.UserId)
			.NotEmpty().WithMessage("'User Id' can not be empty.");

		RuleFor(p => p.CurrentPassword)
			.NotEmpty().WithMessage("Please enter 'Current Password'.");

		RuleFor(p => p.NewPassword)
			.NotEmpty().WithMessage("Please enter 'New Password'.")
			.MaximumLength(100).WithMessage("Maximum length of 'New Password' is 100 characters.");

		RuleFor(p => p.ConfirmNewPassword)
			.NotEmpty().WithMessage("Please enter 'Confirm New Password'.");
	}
}