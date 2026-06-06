using FluentValidation;
using WebApp.Core.Model;

namespace WebApp.Core.Validator;

public class EmailTemplateModelValidator : AbstractValidator<EmailTemplateModel>
{
    public EmailTemplateModelValidator()
    {
        RuleFor(p => p.Subject)
            .NotEmpty().WithMessage("Please enter 'Subject'.")
            .MinimumLength(3).WithMessage("Minimum length of 'Subject' is 3 characters.")
            .MaximumLength(150).WithMessage("Maximum length of 'Subject' is 150 characters.");

        RuleFor(p => p.Template)
            .NotEmpty().WithMessage("Please enter 'Template'.")
            .MinimumLength(50).WithMessage("Minimum length of 'Template' is 50 characters.")
            .MaximumLength(4000).WithMessage("Maximum length of 'Template' is 4000 characters.");
    }
}