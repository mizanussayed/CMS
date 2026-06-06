using FluentValidation;
using WebApp.Core.Model;

namespace WebApp.Core.Validator;

public class PieModelValidator : AbstractValidator<PieModel>
{
    public PieModelValidator()
    {
        RuleFor(p => p.Name)
            .NotEmpty().WithMessage("Please enter 'Name'.")
            .MinimumLength(3).WithMessage("Minimum length of 'Name' is 3 characters.")
            .MaximumLength(150).WithMessage("Maximum length of 'Name' is 150 characters.");

        RuleFor(p => p.CategoryId)
            .GreaterThan(0).WithMessage("Please select a 'Category'.");

        RuleFor(p => p.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Price can not be negative.");

        RuleFor(p => p.ExpiryDate)
            .NotEmpty().WithMessage("Please enter 'Expiry Date'.");

        RuleFor(p => p.Description)
            .NotEmpty().WithMessage("Please enter 'Description'.")
            .MinimumLength(50).WithMessage("Minimum length of 'Description' is 50 characters.")
            .MaximumLength(4000).WithMessage("Maximum length of 'Description' is 4000 characters.");
    }
}