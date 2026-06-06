using FluentValidation;
using WebApp.Core.Model;

namespace WebApp.Core.Validator;

public class UserLoginModelValidator : AbstractValidator<UserLoginModel>
{
	public UserLoginModelValidator()
	{
		RuleFor(p => p.UserName)
			.NotEmpty().WithMessage("Please enter 'User Name'.")
			.MinimumLength(4).WithMessage("Minimum length of 'User Name' is 4 characters.")
			.MaximumLength(200).WithMessage("Maximum length of 'User Name' is 200 characters.");

		RuleFor(p => p.Password)
			.NotEmpty().WithMessage("Please enter 'Password'.")
			.MinimumLength(6).WithMessage("Minimum length of 'Password' is 6 characters.")
			.MaximumLength(50).WithMessage("Maximum length of 'Password' is 50 characters.");
	}
}