using FluentValidation;
using WebApp.Core.Model;

namespace WebApp.Core.Validator;

public class DoctorModelValidator : AbstractValidator<DoctorModel>
{
    public DoctorModelValidator()
    {
        RuleFor(p => p.Name)
            .NotEmpty().WithMessage("Please enter 'Name'.")
            .MinimumLength(3).WithMessage("Minimum length of 'Name' is 3 characters.")
            .MaximumLength(150).WithMessage("Maximum length of 'Name' is 150 characters.");

        RuleFor(p => p.Specialization)
            .MaximumLength(150).WithMessage("Maximum length of 'Specialization' is 150 characters.");
    }
}
