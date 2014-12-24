using Business.Models;
using FluentValidation;

namespace Moovers.WebModels.Validators
{
    public class PhoneNumberValidator : AbstractValidator<PhoneNumber>
    {
        private static readonly string phoneRegex = @"^\(?([0-9]{3})\)?.?([0-9]{3}).?([0-9]{4})$";

        public PhoneNumberValidator()
        {
            RuleFor(p => p.Number).NotEmpty().Matches(phoneRegex).WithMessage("Please enter a valid phone number");
        }
    }
}
