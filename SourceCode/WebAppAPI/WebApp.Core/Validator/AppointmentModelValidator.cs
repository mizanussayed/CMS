using FluentValidation;
using WebApp.Core.Model;

namespace WebApp.Core.Validator;

public class AppointmentModelValidator : AbstractValidator<AppointmentModel>
{
    public AppointmentModelValidator()
    {
        RuleFor(p => p.UserId)
            .GreaterThan(0).WithMessage("Please provide a valid 'UserId'.");

        RuleFor(p => p.DoctorId)
            .GreaterThan(0).WithMessage("Please provide a valid 'DoctorId'.");

        RuleFor(p => p.AppointmentDate)
            .GreaterThan(DateTime.Now.AddMinutes(-5)).WithMessage("Appointment date must be in the future.");
    }
}
